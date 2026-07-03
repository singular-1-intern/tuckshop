import { Data, Model, Utils } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../../IdentityTypes';
import PerformUserActionCommand from '../../Models/UserManagement/PerformUserActionCommand';
import UserLookupCriteria from '../../Models/UserManagement/Queries/UserLookupCriteria';
import UserLookup from '../../Models/UserManagement/Queries/UserLookup';
import UserManagementActionLogLookupCriteria from '../../Models/UserManagement/Queries/UserManagementActionLogLookupCriteria';
import UserManagementActionLogLookup from '../../Models/UserManagement/Queries/UserManagementActionLogLookup';
import UserInviteCriteria from '../../Models/UserManagement/Queries/UserInviteCriteria';
import UserInvite from '../../Models/UserManagement/UserInvite';
import UserInviteLookup from '../../Models/UserManagement/Queries/UserInviteLookup';

export interface IUserManagementApiClient {

    findUsers(request: Model.PartialPlainObject<Data.PageRequest<UserLookupCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<UserLookup>>>;

    performAction(command: Model.PartialPlainObject<PerformUserActionCommand>): AxiosPromise;

    logHistory(request: Model.PartialPlainNonTrackedObject<Data.PageRequest<UserManagementActionLogLookupCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<UserManagementActionLogLookup>>>;

    /** 
     * Gets a list of user invites.
     */
    getUserInvites(pageRequest: Model.PartialPlainNonTrackedObject<Data.PageRequest<UserInviteCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<UserInviteLookup>>>;

    /** 
     * Creates an invited user.
     * @param invitedUser Invited user.
     * @returns Lookup with id.
     */
    saveUserInvite(invitedUser: Model.PartialPlainObject<UserInvite>): AxiosPromise<Model.PlainObject<UserInviteLookup>>;

    /** 
     * Revokes a user invite if the user has not already registered.
     * @param userInviteId User invite id.
     * @returns Task.
     */
    revokeUserInvite(userInviteId: number): AxiosPromise;

    // Client only properties / methods
}

@injectable()
export default class UserManagementApiClient extends Data.ApiClientBase implements IUserManagementApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.identityConfig.identityApiPath}/user-management`);
    }

    public findUsers(request: Model.PartialPlainObject<Data.PageRequest<UserLookupCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<UserLookup>>> {
        return this.axios.post(`${this.apiPath}/find`, request);
    }

    public performAction(command: Model.PartialPlainObject<PerformUserActionCommand>): AxiosPromise {
        return this.axios.post(`${this.apiPath}/perform-action`, command);
    }

    public logHistory(request: Model.PartialPlainNonTrackedObject<Data.PageRequest<UserManagementActionLogLookupCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<UserManagementActionLogLookup>>> {
        return this.axios.get(`${this.apiPath}/log-history?${Utils.getQueryString(request)}`);
    }

    public getUserInvites(pageRequest: Model.PartialPlainNonTrackedObject<Data.PageRequest<UserInviteCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<UserInviteLookup>>> {
        return this.axios.get(`${this.apiPath}/user-invites?${Utils.getQueryString(pageRequest)}`);
    }

    public saveUserInvite(invitedUser: Model.PartialPlainObject<UserInvite>): AxiosPromise<Model.PlainObject<UserInviteLookup>> {
        return this.axios.post(`${this.apiPath}/user-invite`, invitedUser);
    }

    public revokeUserInvite(userInviteId: number): AxiosPromise {
        return this.axios.delete(`${this.apiPath}/user-invite/${userInviteId}`);
    }

    // Client only properties / methods
}