import { Neo, Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import { CreateOrder } from '../../Models/Orders/Commands/CreateOrder';
import Customer from '../../Models/Customer';
import List from '@singularsystems/neo-core/dist/Model/List';
import { Misc, ModalUtils, Model } from '@singularsystems/neo-core';


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


    // Fetch Customers.
    public customers = new List(Customer);

    public async getCustomers() {
        const response = await this.taskRunner.waitFor(this.customersApiClient.get());
        this.customers.set(response.data);
    }

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

    public submitOrder() {
        const orderData = this.newOrder!.toJSObject();

        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.createOrder(orderData);
            this.newOrder = null;
        });
    }

    public depositAmount: number = 0;
    public withdrawAmount: number = 0;

    public depositFunds(customerId: number, amount = this.depositAmount) {
        if (!customerId || amount <= 0) {
            return;
        }

        this.taskRunner.run(async () => {
            await this.customersCommandApiClient.depositFunds({ customerId, amount });

            this.showBasicModal = false;
            this.depositAmount = 0;

            this.notifications.addSuccess(
                'Deposit successful',
                `Deposited ${amount.toFixed(2)} successfully.`,
                4
            );
        });
    }

    public withdrawFunds(customerId: number, amount = this.withdrawAmount) {
        if (!customerId || amount <= 0) {
            return;
        }

        this.taskRunner.run(async () => {
            await this.customersCommandApiClient.withdrawFunds({ customerId, amount });

            this.showBasicModal = false;
            this.withdrawAmount = 0;

            this.notifications.addSuccess(
                'Withdrawal successful',
                `Withdrew ${amount.toFixed(2)} successfully.`,
                4
            );
        });
    }

    private async showInput() {

    // Create a temporary observable to bind to.
    const nameProperty = Model.ObservableProperty.required("Name", "");

    const result = await ModalUtils.showOkCancel(
        "What is your name?",
        <Neo.FormGroup label="Type in your name below:" bind={nameProperty} />, 
        nameProperty); // Pass the property / modal to make sure it is validated before the modal is accepted.

    if (result === Misc.ModalResult.Yes) {
        ModalUtils.showMessage("Name", "Your name is " + nameProperty.value);
    }
}

    public showBasicModal: boolean = false;
}