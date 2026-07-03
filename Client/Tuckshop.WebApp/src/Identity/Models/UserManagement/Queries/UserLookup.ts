import { Attributes, ModelBase, NeoModel, StringUtils } from '@singularsystems/neo-core';

@NeoModel
export default class UserLookup extends ModelBase {

    public id: string = "";

    public firstName: string = "";

    public lastName: string = "";

    public userName: string = "";

    public isActive: boolean = false;

    public emailConfirmed: boolean = false;

    public twoFactorEnabled: boolean = false;

    public twoFactorConfigured: boolean = false;

    @Attributes.Date()
    public lockoutEnd: Date | null = null;

    public identityProvider: string = "";

    public isExternalIdentityProvider: boolean = false;

    public providerRequiresMFA: boolean = false;

    // Client only properties / methods

    public get canSendVerificationLink() {
        return !this.isExternalIdentityProvider && !this.emailConfirmed && this.isActive;
    }

    public get canEnableMFA() {
        return !this.isExternalIdentityProvider && !this.twoFactorEnabled && this.isActive
    }

    public get canDisableMFA() {
        return !this.providerRequiresMFA && !this.isExternalIdentityProvider && this.twoFactorEnabled && this.isActive
    }

    public get canResetMFA() {
        return !this.isExternalIdentityProvider && this.twoFactorEnabled && this.twoFactorConfigured && this.isActive
    }
    
    public get canClearLockout() {
        return !this.isExternalIdentityProvider && this.lockoutEnd && this.lockoutEnd > new Date() && this.isActive;
    }

    public toString() {
        return `${this.firstName} ${this.lastName}`;
    }
}