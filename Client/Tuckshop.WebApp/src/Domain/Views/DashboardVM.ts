import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../DomainTypes';

export default class DashboardVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications)) {

        super(taskRunner);
        this.makeObservable();
    }

    public async initialise() {

    }
}