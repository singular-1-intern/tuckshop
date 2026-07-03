import { Data, EnumHelper, NeoModel } from '@singularsystems/neo-core';
import { Views } from '@singularsystems/neo-react';
import { AuthorisationTypes } from '@singularsystems/neo-authorisation';
import { NotificationDuration } from '../../../App/Models/NotificationDuration';
import { AppService, Types } from '../../IdentityTypes';
import PerformUserActionCommand from '../../Models/UserManagement/PerformUserActionCommand';
import UserLookup from '../../Models/UserManagement/Queries/UserLookup';
import UserLookupCriteria from '../../Models/UserManagement/Queries/UserLookupCriteria';
import UserManagementActionLogLookup from '../../Models/UserManagement/Queries/UserManagementActionLogLookup';
import UserManagementActionLogLookupCriteria from '../../Models/UserManagement/Queries/UserManagementActionLogLookupCriteria';
import UserInvite from '../../Models/UserManagement/UserInvite';
import UserInviteCriteria from '../../Models/UserManagement/Queries/UserInviteCriteria';
import UserInviteLookup from '../../Models/UserManagement/Queries/UserInviteLookup';
import { UserManagementAction } from '../../Models/UserManagement/UserManagementAction';

@NeoModel
export default class UserManagementVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private apiClient = AppService.get(Types.Identity.ApiClients.UserManagementApiClient),
        public authorisationDataCache = AppService.get(AuthorisationTypes.DataCache)) {

        super(taskRunner);

        this.autoDispose(this.userInviteCriteria.onAnyPropertyChanged(() => this.userInvitesPageManager.refreshData()));
    }

    public async initialise() {
    }

    public criteria = new UserLookupCriteria();

    private historyCriteria = new UserManagementActionLogLookupCriteria();

    public historyUser: UserLookup | null = null;

    public historyPageManager = new Data.PageManager(this.historyCriteria, UserManagementActionLogLookup, this.apiClient.logHistory, {
        pageSize: 10,
        initialTaskRunner: this.taskRunner,
        fetchInitial: false,
        beforeFetch: request => request.criteria!.userId = this.historyUser!.id
    });

    public fetched = false;
    public triedFetch = false;

    public pageManager = new Data.PageManager(this.criteria, UserLookup, this.apiClient.findUsers, {
        pageSize: 10,
        initialTaskRunner: this.taskRunner,
        fetchInitial: false
    });

    public userInviteCriteria = new UserInviteCriteria();
    public userInvitesPageManager = new Data.PageManager(this.userInviteCriteria, UserInviteLookup, this.apiClient.getUserInvites, { fetchInitial: true });
    public newUserInvite: UserInvite | null = null;

    public async search() {
        if (this.criteria.isValid) {
            await this.pageManager.refreshData();
            this.fetched = true;
        }
        this.triedFetch = true;
    }

    public clear() {
        this.criteria.clear();
    }

    public async performAction(action: UserManagementAction, user: UserLookup) {
        const command = new PerformUserActionCommand();
        command.userId = user.id;
        command.action = action;
        await this.taskRunner.run(() => this.apiClient.performAction(command.toJSObject()));
        
        // refresh the page
        await this.pageManager.refreshData();

        this.notifications.addSuccess(`${this.getActionDescription(action, user)} completed`, null, NotificationDuration.Standard);
    }

    public getActionDescription(actionType: UserManagementAction, user: UserLookup) {
        return EnumHelper.getItemMetadata(UserManagementAction, actionType).description!.replace("{User}", user.toString());
    }

    public showHistory(item: UserLookup) {
        this.historyUser = item;
        this.historyPageManager.reset();
        this.historyPageManager.refreshData();
    }

    public get someCanSendVerificationLink() {
        return this.pageManager.data.some(u => u.canSendVerificationLink);
    }

    public get someCanEnableMFA() {
        return this.pageManager.data.some(u => u.canEnableMFA);
    }

    public get someCanDisableMFA() {
        return this.pageManager.data.some(u => u.canDisableMFA);
    }

    public get someCanResetMFA() {
        return this.pageManager.data.some(u => u.canResetMFA);
    }

    public get someCanClearLockout() {
        return this.pageManager.data.some(u => u.canClearLockout);
    }

    public get someActive() {
        return this.pageManager.data.some(u => u.isActive);
    }

    public get someInactive() {
        return this.pageManager.data.some(u => !u.isActive);
    }

    public createUserInvite() {
        this.taskRunner.run(async () => {

            const response = await this.apiClient.saveUserInvite(this.newUserInvite!.toJSObject());

            const userInvite = UserInviteLookup.fromJSObject<UserInviteLookup>(response.data);
            this.userInvitesPageManager.data.push(userInvite);

            this.notifications.addSuccess("User Invite", "User invite saved successfully.", NotificationDuration.Standard);

            this.newUserInvite = null;
        });
    }

    public revokeUserInvite(userInvite: UserInviteLookup) {

        this.taskRunner.run(async () => {
            await this.apiClient.revokeUserInvite(userInvite.userInviteId);

            this.notifications.addSuccess("Revoke invite", userInvite.emailAddress + " removed from invite list.", NotificationDuration.Standard);
            this.userInvitesPageManager.data.removeWithoutTracking(userInvite);
        });
    }
}