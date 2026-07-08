import { Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import { CreateOrder } from '../../Models/Orders/Commands/CreateOrder';
import Customer from '../../Models/Customer';
import List from '@singularsystems/neo-core/dist/Model/List';
import PaystackPop from '@paystack/inline-js';

type WalletAction = 'deposit' | 'withdraw';


export default class CreateOrderVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private appDataCache = AppService.get(DomainTypes.Services.DataCache),
        private ordersCommandApiClient = AppService.get(DomainTypes.ApiClients.OrdersCommandApiClient),
        private customersApiClient = AppService.get(DomainTypes.ApiClients.CustomersApiClient),
        private customersCommandApiClient = AppService.get(DomainTypes.ApiClients.CustomersCommandApiClient)
    ) {

        super(taskRunner);
        this.makeObservable();
    }

    public async initialise() {
        await this.getCustomers();
        await this.setupOrder(); 
    }

    public newOrder: CreateOrder | null = null;

    public customers = new List(Customer);

    public async getCustomers() {
        const response = await this.taskRunner.waitFor(this.customersApiClient.get());
        this.customers.set(response.data);
    }

    // SETUP NEW ORDER

    public async setupOrder() {
        const newOrder = new CreateOrder();

        const products = await this.taskRunner.waitFor(this.appDataCache.products.getDataAsync());

        for (const product of products) {
            const orderDetail = newOrder.orderDetails.addNew();
            orderDetail.productId = product.productId;
            orderDetail.productName = product.productName;
            orderDetail.price = product.price;
        }

        this.newOrder = newOrder;
    }

    // SUBMIT ORDERS

    public submitOrder() {
        const orderData = this.newOrder!.toJSObject();

        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.createOrder(orderData);
            this.newOrder = null;
        });
    }

    // DEPOSIT & WITHDRAWAL OF FUNDS

    public showBasicModal: boolean = false;
    public walletAmount: number = 0;

    private walletCustomerId: number = 0;
    private walletAction: WalletAction = 'deposit';

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

        this.walletAction = action;
        this.walletCustomerId = customerId;
        this.walletAmount = 0;
        this.showBasicModal = true;
    }

    public submitWalletAction(amount = this.walletAmount) {
        if (!this.walletCustomerId || amount <= 0) {
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

    public startPaystackCheckout(customerId: number, amount = this.walletAmount) {
        if (!customerId || amount <= 0) {
            return;
        }

        this.taskRunner.run(async () => {
            await this.initPaystackPopup();

            const publicKey = process.env.REACT_APP_PAYSTACK_PK;
            if (!publicKey) {
                console.error('Missing REACT_APP_PAYSTACK_PK environment variable.');
                return;
            }

            this.popup.newTransaction({
                key: publicKey,
                email: "test-customer@example.com",
                amount: amount * 100,
                onSuccess: (transaction: any) => {
                    console.log("Payment successful! Reference:", transaction.reference);
                },
                onCancel: () => {
                    console.log("User closed the checkout popup.");
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