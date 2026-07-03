import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';
import Customer from '../Models/Customer';
import List from '@singularsystems/neo-core/dist/Model/List';

export default class CustomersVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private customersApiClient = AppService.get(Types.Domain.ApiClients.CustomersApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }

    public customers = new List(Customer);

    public async initialise() {
        const response = await this.taskRunner.waitFor(this.customersApiClient.get());
        this.customers.set(response.data);
    }

    public saveCustomers() {
        this.taskRunner.run(async () => {
            const response = await this.customersApiClient.saveList(this.customers.toJSArray());
            this.customers.update(response.data);
            this.notifications.addSuccess(
                "Customers saved",
                "Customers saved successfully",
                4,
            );
        });
    }
}