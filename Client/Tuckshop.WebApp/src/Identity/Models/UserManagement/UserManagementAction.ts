import { EnumHelper } from '@singularsystems/neo-core';

export enum UserManagementAction {
    ResendEmailVerificationLink,
    ResetMFA,
    ClearLockout,
    Activate,
    Deactivate,
    EnableMFA,
    DisableMFA,
}

EnumHelper.decorateEnum(UserManagementAction, decorator => {
    decorator.describe(UserManagementAction.ResendEmailVerificationLink, "ResendEmailVerificationLink", "Resend the email verification link for {User}");
    decorator.describe(UserManagementAction.ResetMFA, "ResetMFA", "Reset MFA for {User}");
    decorator.describe(UserManagementAction.ClearLockout, "ClearLockout", "Clear lockout for {User}");
    decorator.describe(UserManagementAction.Activate, "Activate", "Activate {User}");
    decorator.describe(UserManagementAction.Deactivate, "Deactivate", "Deactivate {User}");
    decorator.describe(UserManagementAction.EnableMFA, "EnableMFA", "Enable MFA for {User}");
    decorator.describe(UserManagementAction.DisableMFA, "DisableMFA", "Disable MFA for {User}");
});