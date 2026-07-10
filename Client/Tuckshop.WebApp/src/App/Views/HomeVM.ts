import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../Services/AppService';
import { DomainTypes } from '../../Domain/DomainTypes';
import OrderLookup from '../../Domain/Models/Orders/Queries/OrderLookup';
import List from '@singularsystems/neo-core/dist/Model/List';
import OrderLookupCriteria from '../../Domain/Models/Orders/Queries/OrderLookupCriteria';

export default class HomeVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private ordersQueryApiClient = AppService.get(DomainTypes.ApiClients.OrdersQueryApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }
    
    public criteria = new OrderLookupCriteria();

    public foundOrders = new List(OrderLookup);

    public async initialise() {

    }
}