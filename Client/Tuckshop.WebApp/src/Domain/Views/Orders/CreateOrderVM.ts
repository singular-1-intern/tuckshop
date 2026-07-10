import { Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import { CreateOrder } from '../../Models/Orders/Commands/CreateOrder';
import { NewOrderDetail } from '../../Models/Orders/Commands/CreateOrder';
import Customer from '../../Models/Customer';
import List from '@singularsystems/neo-core/dist/Model/List';
import PaystackPop from '@paystack/inline-js';
import { Attributes, Rules, Validation } from '@singularsystems/neo-core';
import OrderLookupCriteria from '../../Models/Orders/Queries/OrderLookupCriteria';
import OrderLookup from '../../Models/Orders/Queries/OrderLookup';

type WalletAction = 'deposit' | 'withdraw';


export default class CreateOrderVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private appDataCache = AppService.get(DomainTypes.Services.DataCache),
        private ordersCommandApiClient = AppService.get(DomainTypes.ApiClients.OrdersCommandApiClient),
        private customersApiClient = AppService.get(DomainTypes.ApiClients.CustomersApiClient),
        private customersCommandApiClient = AppService.get(DomainTypes.ApiClients.CustomersCommandApiClient),
        private ordersQueryApiClient = AppService.get(DomainTypes.ApiClients.OrdersQueryApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }

    public async initialise() {
        await this.getCustomers();
        await this.setupOrder(); 
    }

    public criteria = new OrderLookupCriteria();

    public foundOrders = new List(OrderLookup);

    public myOrdersDisplay: boolean = false;

    public async findOrders() {
        if (this.selectedCustomer) {
            this.criteria.customerName = this.selectedCustomer;
        }
        const response = await this.taskRunner.waitFor(this.ordersQueryApiClient.getOrderLookupAsync(this.criteria.toQueryObject()));
        this.foundOrders.set(response.data);
    }

    public newOrder: CreateOrder | null = null;

    public customers = new List(Customer);

    public selectedCustomer: string | null = null;

    public selectedCustomerId: number = 0;

    public isOrderSuccessful: boolean = false;

    public viewOrders: boolean = false;

    public async getCustomers() {
        const response = await this.taskRunner.waitFor(this.customersApiClient.get());
        this.customers.set(response.data);
    }

    public showMyOrdersForCustomer(customerId: number) {
        const customer = this.customers.find(c => c.customerId === customerId);
        if (!customer) {
            return;
        }

        this.selectedCustomerId = customer.customerId;
        this.selectedCustomer = customer.customerName;
        this.criteria.customerName = customer.customerName;
        this.myOrdersDisplay = true;

        this.viewOrders = true;
        this.taskRunner.run(async () => {
            await this.findOrders();
        });
    }

    public backToShop() {
        this.myOrdersDisplay = false;
        if (!this.newOrder) {
            this.isOrderSuccessful = false;
            this.taskRunner.run(async () => {
                await this.setupOrder();
            });
        }
    }

    public successAlert() {
        this.isOrderSuccessful = true;
    }

    public showSelectedCustomerOrders() {
        if (this.selectedCustomerId <= 0) {
            return;
        }

        this.isOrderSuccessful = false;
        this.showMyOrdersForCustomer(this.selectedCustomerId);
    }

    public createAnotherOrder() {
        this.myOrdersDisplay = false;
        this.isOrderSuccessful = false;
        this.taskRunner.run(async () => {
            await this.setupOrder();
        });
    }

    public clearSelectedCustomer() {
        this.selectedCustomerId = 0;
        this.selectedCustomer = null;
        this.criteria.customerName = null;
        this.myOrdersDisplay = false;
    }

    // SETUP NEW ORDER

    public async setupOrder() {
        const newOrder = new CreateOrder();

        const products = await this.taskRunner.waitFor(this.appDataCache.products.getDataAsync());

        for (const product of products) {
            const orderDetail = newOrder.orderDetails.addNew();
            orderDetail.productId = product.productId;
            orderDetail.productName = product.productName;
            orderDetail.imageUrl = product.imageUrl;
            orderDetail.price = product.price;
        }

        this.newOrder = newOrder;
    }

    public incrementOrderDetailQuantity(orderDetail: NewOrderDetail) {
        orderDetail.quantity += 1;
    }

    public decrementOrderDetailQuantity(orderDetail: NewOrderDetail) {
        orderDetail.quantity = Math.max(0, orderDetail.quantity - 1);
    }

    // SUBMIT ORDERS

    public submitOrder() {
        if (!this.newOrder) {
            return;
        }
        
        const orderData = this.newOrder.toJSObject();
        const customerId = this.newOrder.customerId;
        const selectedCustomer = this.customers.find(c => c.customerId === customerId);
        this.selectedCustomerId = customerId;
        this.selectedCustomer = selectedCustomer?.customerName ?? null;
        this.criteria.customerName = this.selectedCustomer;
        const totalAmount = this.orderTotalAmount;

        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.createOrder(orderData);

            if (totalAmount > 0) {
                const customer = this.customers.find(c => c.customerId === customerId);
                const walletBalance = customer?.walletBalance ?? 0;
                if (totalAmount > walletBalance) {
                    this.notifications.addDanger(
                        'Order Payment Failed',
                        `Order total exceeds wallet balance. Please deposit atleast R ${totalAmount - walletBalance}  to complete the order.`,
                        4
                    );
                    return;
                }
                await this.customersCommandApiClient.withdrawFunds({
                    customerId,
                    amount: totalAmount
                });
                await this.getCustomers();
            }
            this.newOrder = null;
            this.isOrderSuccessful = true;
        });
    }

    // DEPOSIT & WITHDRAWAL OF FUNDS

    public showBasicModal: boolean = false;

    @Attributes.Float()
    @Rules.Required()
    public walletAmount: number = 0;

    public currentWalletBalance: number = 0;

    private walletCustomerId: number = 0;
    public walletAction: WalletAction = 'deposit';

    public depositFunds(customerId: number) {
        this.openWalletModal(customerId, 'deposit');
    }

    public withdrawFunds(customerId: number) {
        this.openWalletModal(customerId, 'withdraw');
    }

    private openWalletModal(customerId: number, action: WalletAction) {
        if (!customerId) {
            return;
        }

        const customer = this.customers.find(c => c.customerId === customerId);

        this.walletAction = action;
        this.walletCustomerId = customerId;
        this.currentWalletBalance = customer?.walletBalance ?? 0;
        this.walletAmount = 0;
        this.showBasicModal = true;
    }

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);
        rules.failWhen(c => (c.walletAmount ?? 0) <= 0, "Amount is required.");
        rules.failWhen(c => (c.walletAmount ?? 0) > 0 && c.walletAmount < 10, "Minimum amount is R10.");
    }

    public submitWalletAction(amount = this.walletAmount) {
        if (!this.walletCustomerId || amount < 10) {
            return;
        }

        const action = this.walletAction;
        const actionText = action === 'deposit' ? 'Deposit' : 'Withdrawal';

        this.taskRunner.run(async () => {
            if (action === 'deposit') {
                await this.customersCommandApiClient.depositFunds({ customerId: this.walletCustomerId, amount });
            } else {
                await this.customersCommandApiClient.withdrawFunds({ customerId: this.walletCustomerId, amount });
            }

            await this.getCustomers();
            this.showBasicModal = false;
            this.walletAmount = 0;

            this.notifications.addSuccess(
                `${actionText} successful`,
                `${actionText} of ${amount} successful.`,
                4
            );
        });
    }

    public get orderTotalAmount() {
        if (!this.newOrder) {
            return 0;
        }

        return this.newOrder.orderDetails.reduce((sum, item) => sum + item.value, 0);
    }

    public startPaystackCheckout(customerId: number, amount = this.walletAmount) {
        if (!customerId || amount <= 0 ) {
            this.notifications.addDanger('Payment Failed', 'Minimum Deposit/Withdrawal amount is R10.', 4);
            return;
        }

        if (this.walletAction === 'withdraw' && amount > this.currentWalletBalance) {
            this.notifications.addDanger(
                'Withdrawal Failed',
                'Withdrawal amount exceeds wallet balance.',
                4
            );
            this.showBasicModal = false;
            this.walletAmount = 0;
            return;
        }

        this.taskRunner.run(async () => {
            await this.initPaystackPopup();

            const publicKey = process.env.REACT_APP_PAYSTACK_PK;
            if (!publicKey) {
                this.notifications.addDanger('Payment Failed', 'Paystack public API key is missing.', 4);
                return;
            }
            
            this.popup.newTransaction({
                key: publicKey,
                email: "test-customer@example.com",
                amount: amount * 100,
                onSuccess: () => {
                    this.submitWalletAction(amount);
                },
                onCancel: () => {
                    this.notifications.addDanger('Payment cancelled', 'The payment was cancelled by the user.', 4);
                    this.showBasicModal = false;
                }
            });
        });
    }

    private _popup?: PaystackPop;

    public get popup() {
        if (!this._popup) {
            throw new Error('Paystack popup not initialized yet');
        }
        return this._popup;
    }

    public async initPaystackPopup() {
        if (!this._popup) {
            const { default: PaystackPopCtor } = await import('@paystack/inline-js');
            this._popup = new PaystackPopCtor() as any;
        }
    }

}