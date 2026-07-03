import React from 'react';
import { observer } from 'mobx-react';
import { Misc, ModalUtils } from '@singularsystems/neo-core';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../../IdentityTypes';
import UserManagementVM from './UserManagementVM';
import UserLookup from '../../Models/UserManagement/Queries/UserLookup';
import { UserManagementAction } from '../../Models/UserManagement/UserManagementAction';
import UserManagementActionLogLookup from '../../Models/UserManagement/Queries/UserManagementActionLogLookup';
import * as Roles from '../../Models/Security/Roles';
import { ScreenSize } from '../../../App/Services/AppLayout';
import UserInvite from '../../Models/UserManagement/UserInvite';
import UserInviteLookup from '../../Models/UserManagement/Queries/UserInviteLookup';

@observer
export default class UserManagementView extends Views.ViewBase<UserManagementVM> {

    private appLayout = AppService.get(Types.App.Services.AppLayout);
    private authorisationService = AppService.get(Types.Neo.Security.AuthorisationService);

    constructor(props: unknown) {
        super("User Management", UserManagementVM, props);
    }

    private async confirmAction(action: UserManagementAction, user: UserLookup) {
        const actionDescription = this.viewModel.getActionDescription(action, user);
        if (await ModalUtils.showYesNoDismissible(actionDescription, `Are you sure you want to ${actionDescription}?`) === Misc.ModalResult.Yes) {
            await this.viewModel.performAction(action, user);
        }
    }

    public render() {
        const hasInviteUserRole = this.authorisationService.hasRole(Roles.UserManagement.InviteUser);

        return (
            <div className="pt-3">

                {hasInviteUserRole && 
                    <Neo.TabContainer>
                        <Neo.Tab header="Users">
                            {this.renderUsers()}
                        </Neo.Tab>
                        <Neo.Tab header="Invited users">
                            {this.renderUserInvites()}
                        </Neo.Tab>
                    </Neo.TabContainer>}
                
                {!hasInviteUserRole && 
                    this.renderUsers()}
                
                <Neo.Modal title={`Action Log: ${this.viewModel.historyUser?.toString()}`}
                    size="lg"
                    bind={this.viewModel.meta.historyUser}>
                    <Neo.Pager pageManager={this.viewModel.historyPageManager} pageControls="top">
                        <NeoGrid.Grid<UserManagementActionLogLookup>>
                            {(item, meta) => (
                                <NeoGrid.Row>
                                    <NeoGrid.Column display={meta.action} />
                                    <NeoGrid.Column display={meta.actionedBy} />
                                    <NeoGrid.Column display={meta.actionedOn} dateProps={{ formatString: "dd MMM yy HH:mm" }} />
                                </NeoGrid.Row>
                            )}
                        </NeoGrid.Grid>
                    </Neo.Pager>
                </Neo.Modal>

                <Neo.Modal 
                    title="Invite User" 
                    bindModel={this.viewModel.meta.newUserInvite}
                    acceptButton={{ text: "Invite User", onClick: () => this.viewModel.createUserInvite() }}
                    closeButton={false}>
                    {(invite: UserInvite) => (
                        <div>
                            <Neo.FormGroup bind={invite.meta.emailAddress} />
                            <Neo.FormGroup bind={invite.meta.addToUserGroupId} 
                                label="User group (optional)"
                                select={{ itemSource: this.viewModel.authorisationDataCache.userGroups, displayMember: "userGroupName" }} />
                        </div>
                    )}
                </Neo.Modal>
            </div>
        );
    }

    private renderUsers() {
        const allowedToResendEmailVerification = this.authorisationService.hasRole(Roles.UserManagement.EmailVerificationLinkResend);
        const allowedToEnableMFA = this.authorisationService.hasRole(Roles.UserManagement.EnableMFA);
        const allowedToResetMFA = this.authorisationService.hasRole(Roles.UserManagement.ResetMFA);
        const allowedToDisableMFA = this.authorisationService.hasRole(Roles.UserManagement.DisableMFA);
        const allowedToClearLockout = this.authorisationService.hasRole(Roles.UserManagement.ClearLockout);
        const allowedToDeactivate = this.authorisationService.hasRole(Roles.UserManagement.DeactivateUser);
        const allowedToActivate = this.authorisationService.hasRole(Roles.UserManagement.ActivateUser);

        return (
            <div>
                <Neo.Card title="Search" icon="search">
                    <Neo.Form model={this.viewModel.criteria} onSubmit={() => this.viewModel.search()}>
                        <Neo.GridLayout lg={4} >
                            <Neo.FormGroup bind={this.viewModel.criteria.meta.firstName} />
                            <Neo.FormGroup bind={this.viewModel.criteria.meta.lastName} />
                            <Neo.FormGroup bind={this.viewModel.criteria.meta.userName} />
                            <Neo.FormGroup bind={this.viewModel.criteria.meta.isActive}
                                editorProps={{ className: "mt-1" }}
                                radioList={{ items: [{ value: null, text: "All" }, { value: true, text: "Active" }, { value: false, text: "Inactive" }], inline: true, valueMember: "value" }} />
                        </Neo.GridLayout>
                        {this.viewModel.triedFetch &&
                            <div className='mb-3'>
                                <Neo.ValidationSummary model={this.viewModel.criteria} hideTopLevelTitle />
                            </div>}

                        <div className="mt-3">
                            <Neo.Button icon="search" text="Search" className="me-2" isSubmit />
                            <Neo.Button variant='secondary' icon="eraser" text="Clear" onClick={() => this.viewModel.clear()} />
                        </div>
                    </Neo.Form>
                </Neo.Card>

                <Neo.Card title="Users" icon="users">
                    {!this.viewModel.fetched &&
                        <Neo.Alert variant="info" className="mb-0">Type some criteria and hit search.</Neo.Alert>}
                    {this.viewModel.fetched && this.viewModel.pageManager.data.length === 0 &&
                        <Neo.Alert variant="info" className="mb-0">No users found for specified criteria, change your criteria and hit search.</Neo.Alert>}
                    {this.viewModel.fetched && this.viewModel.pageManager.data.length > 0 &&
                        <Neo.Pager pageManager={this.viewModel.pageManager} pageControls="top">
                            <NeoGrid.Grid<UserLookup>>
                                {(item, meta) => <NeoGrid.Row>
                                    <NeoGrid.Column display={meta.firstName} hideBelow="xl" />
                                    <NeoGrid.Column display={meta.lastName} hideBelow="xl" />
                                    <NeoGrid.Column display={meta.userName} />
                                    <NeoGrid.Column display={meta.isActive} hideBelow="md" />
                                    <NeoGrid.Column display={meta.emailConfirmed} label="Email Confirmed" hideBelow="md" />
                                    <NeoGrid.Column display={meta.twoFactorEnabled} label="MFA Enabled" hideBelow="lg" />
                                    <NeoGrid.Column display={meta.twoFactorConfigured} label="MFA Configured" hideBelow="lg" />
                                    <NeoGrid.Column display={meta.lockoutEnd} hideBelow="xl" dateProps={{ formatString: "dd MMM yyyy HH:mm" }} />
                                    <NeoGrid.Column display={meta.identityProvider} label="Provider" hideBelow="xxl" />

                                    {allowedToResendEmailVerification && this.viewModel.someCanSendVerificationLink &&
                                        <NeoGrid.ButtonColumn>
                                            {item.canSendVerificationLink &&
                                                <Neo.Button size='sm' icon="paper-plane" tooltip="Resend verification link"
                                                    onClick={() => this.confirmAction(UserManagementAction.ResendEmailVerificationLink, item)} />}
                                        </NeoGrid.ButtonColumn>}

                                    {allowedToEnableMFA && this.viewModel.someCanEnableMFA &&
                                        <NeoGrid.ButtonColumn>
                                            {item.canEnableMFA &&
                                                <Neo.Button size='sm' variant="success" icon="user-shield" tooltip="Enable MFA."
                                                    onClick={() => this.confirmAction(UserManagementAction.EnableMFA, item)} />}
                                        </NeoGrid.ButtonColumn>}

                                    {((allowedToResetMFA && this.viewModel.someCanResetMFA) || (allowedToDisableMFA && this.viewModel.someCanDisableMFA)) &&
                                        <NeoGrid.ButtonColumn>
                                            {allowedToDisableMFA && item.canDisableMFA &&
                                                <Neo.Button size='sm' icon="user-times" variant='warning' tooltip="WARNING! Disables the user's MFA so that they no longer have MFA."
                                                    onClick={() => this.confirmAction(UserManagementAction.DisableMFA, item)} />}
                                            {allowedToResetMFA && item.canResetMFA &&
                                                <Neo.Button size='sm' icon="key" variant='secondary' tooltip="Resets the user's MFA so that they can reconfigure it on their next login."
                                                    onClick={() => this.confirmAction(UserManagementAction.ResetMFA, item)} />}
                                        </NeoGrid.ButtonColumn>}

                                    {allowedToClearLockout && this.viewModel.someCanClearLockout &&
                                        <NeoGrid.ButtonColumn>
                                            {item.canClearLockout &&
                                                <Neo.Button size='sm' icon="eraser" variant='info' tooltip="Clear's the user's lockout (if they have entered the wrong password too many times)."
                                                    onClick={() => this.confirmAction(UserManagementAction.ClearLockout, item)} />}
                                        </NeoGrid.ButtonColumn>}

                                    {(allowedToActivate || allowedToDeactivate) &&
                                        <NeoGrid.ButtonColumn>
                                            {allowedToActivate && !item.isActive &&
                                                <Neo.Button size='sm' icon="unlock" variant='success' tooltip="Activate user."
                                                    onClick={() => this.confirmAction(UserManagementAction.Activate, item)} />}
                                            {allowedToDeactivate && item.isActive &&
                                                <Neo.Button size='sm' icon="lock" variant='danger' tooltip="Deactivate user."
                                                    onClick={() => this.confirmAction(UserManagementAction.Deactivate, item)} />}
                                        </NeoGrid.ButtonColumn>}

                                    <NeoGrid.ButtonColumn hideBelow="md">
                                        <Neo.Button size="sm" icon="history" isOutline onClick={() => this.viewModel.showHistory(item)} />
                                    </NeoGrid.ButtonColumn>
                                </NeoGrid.Row>}
                            </NeoGrid.Grid>
                        </Neo.Pager>}
                </Neo.Card>
            </div>
        )
    }

    private renderUserInvites() {
        return (
            <div>
                <div className="mb-3 d-flex justify-content-start align-items-center">
                    <Neo.Button icon="user-plus" onClick={() => this.viewModel.newUserInvite = new UserInvite()}>Invite User</Neo.Button>

                    <Neo.FormGroup label="Show registered users?" bind={this.viewModel.userInviteCriteria.meta.includeRegistered} className="mb-0 ms-4" />
                </div>

                {!this.viewModel.userInviteCriteria.includeRegistered && this.viewModel.userInvitesPageManager.data.length === 0 && 
                    <Neo.Alert variant="info" className="mb-0">No pending user invites</Neo.Alert>}

                    <Neo.Pager pageManager={this.viewModel.userInvitesPageManager}>
                        <>
                            {this.viewModel.userInvitesPageManager.data.length > 0 &&
                                <NeoGrid.Grid<UserInviteLookup>>
                                    {(item, meta) => (
                                        <NeoGrid.Row>
                                            <NeoGrid.Column display={meta.emailAddress} />
                                            <NeoGrid.Column display={meta.createdOn} label="Invited on" />
                                            <NeoGrid.Column display={meta.status} />
                                            <NeoGrid.ButtonColumn>
                                                {!item.hasRegistered && <Neo.Button icon="trash" variant="danger" size="sm" tooltip="Revoke invite" onClick={() => this.revokeUserInvite(item)}></Neo.Button>}
                                            </NeoGrid.ButtonColumn>
                                        </NeoGrid.Row>
                                    )}
                                </NeoGrid.Grid>}
                        </>
                    </Neo.Pager>
                
            </div>)
    }

    private revokeUserInvite(userInvite: UserInviteLookup) {
        ModalUtils.showYesNoDismissible("Revoke", 
            <div>
                <div>Are you sure you want to revoke this user invite? </div>
                <div className="mt-3">The invited user may have already received their invite, if their invite is revoked then they will not be able to register.</div>
            </div>, 
            () => this.viewModel.revokeUserInvite(userInvite));
    }
}