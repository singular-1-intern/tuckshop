import { Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import OrderLookupCriteria from '../../Models/Orders/Queries/OrderLookupCriteria';
import OrderLookup from '../../Models/Orders/Queries/OrderLookup';
import List from '@singularsystems/neo-core/dist/Model/List';
import Customer from '../../Models/Customer';

export default class ViewOrdersVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private ordersQueryApiClient = AppService.get(DomainTypes.ApiClients.OrdersQueryApiClient),
        private ordersCommandApiClient = AppService.get(DomainTypes.ApiClients.OrdersCommandApiClient),
        private customersApiClient = AppService.get(DomainTypes.ApiClients.CustomersApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }    

    public criteria = new OrderLookupCriteria();
    public foundOrders = new List(OrderLookup);

    public async findOrders() {
        const response = await this.taskRunner.waitFor(this.ordersQueryApiClient.getOrderLookupAsync(this.criteria.toQueryObject()));
        this.foundOrders.set(response.data);
    }

    public completeOrder(order: OrderLookup) {
        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.completeOrder({ orderId: order.orderId });
            order.completedOn = new Date();
        })
    }

    public cancelOrder(order: OrderLookup, reason: string) {
        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.cancelOrder({ orderId: order.orderId, reason });
            order.cancelledOn = new Date();
            order.cancelledReason = reason;
        });
    }

    public customers = new List(Customer);

    public async getCustomers() {
        const response = await this.taskRunner.waitFor(this.customersApiClient.get());
        this.customers.set(response.data);
    }

    public async initialise() {
        await this.getCustomers();
    }
}