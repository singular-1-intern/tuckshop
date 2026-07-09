import { Views } from '@singularsystems/neo-react';
import { AppService, DomainTypes, Types } from '../../DomainTypes';
import OrderLookupCriteria from '../../Models/Orders/Queries/OrderLookupCriteria';
import OrderLookup from '../../Models/Orders/Queries/OrderLookup';
import List from '@singularsystems/neo-core/dist/Model/List';

export default class ViewOrdersVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private ordersQueryApiClient = AppService.get(DomainTypes.ApiClients.OrdersQueryApiClient),) {

        super(taskRunner);
        this.makeObservable();
    }    

    public criteria = new OrderLookupCriteria();

    public foundOrders = new List(OrderLookup);

    public async findOrders() {
        const response = await this.taskRunner.waitFor(this.ordersQueryApiClient.getOrderLookupAsync(this.criteria.toQueryObject()));
        this.foundOrders.set(response.data);
    }

    public async initialise() {

    }
}