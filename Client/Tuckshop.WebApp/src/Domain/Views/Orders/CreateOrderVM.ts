import { Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import { CreateOrder } from '../../Models/Orders/Commands/CreateOrder';

export default class CreateOrderVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private appDataCache = AppService.get(DomainTypes.Services.DataCache),
        private ordersCommandApiClient = AppService.get(DomainTypes.ApiClients.OrdersCommandApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }

    public newOrder: CreateOrder | null = null;

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

    public async initialise() {
        await this.setupOrder();
    }
}