import { Attributes, ModelBase, NeoModel, Rules, Validation, Misc } from '@singularsystems/neo-core';
import { IdentityProviderType } from './IdentityProviderType';
import IdentityProviderTypeLookup from './IdentityProviderTypeLookup';
import OidcProviderConfig from './OidcProviderConfig';
import { AppService, Types } from '../../IdentityTypes';

@NeoModel
export default class IdentityProvider extends ModelBase {

    public identityProviderId: number = 0;

    @Rules.Required()
    @Rules.StringLength(40)
    private nameSuffix: string = "";

    @Attributes.Display("Name Suffix")
    public get editNameSuffix() { return this.nameSuffix; }
    public set editNameSuffix(value: string) { this.nameSuffix = value.toLocaleLowerCase().replace(" ", "-"); }

    @Rules.StringLength(75)
    @Rules.Required()
    public name: string = "";

    @Rules.StringLength(100)
    @Rules.Required()
    public displayName: string = "";

    @Rules.Required()
    public identityProviderType: IdentityProviderType | null = null;

    @Rules.StringLength(100)
    public buttonImageUrl: string = "";

    @Attributes.ChildObject(OidcProviderConfig)
    public oidcConfig: OidcProviderConfig | null = null;

    // Client only properties / methods

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);

        rules.failWhen(
            c => !c.nameSuffix.match(/^[a-z-]+$/g),
            `Identity Provider name suffix must contain only lower case letters or dashes.`).onProperties(c => { return c.editNameSuffix });
    }

    public toString(): string {
        if (this.isNew || !this.name) {
            return "New identity provider";
        } else {
            return this.name;
        }
    }

    // This is a getter on the server model which the generator isn't bringing in
    @Attributes.NoTracking(Misc.SerialiseType.FullOnly)
    public isLocked: boolean = false;

    @Attributes.NoTracking()
    public providerType: IdentityProviderTypeLookup | null = null;

    @Attributes.NoTracking()
    private config = AppService.get(Types.App.Config);

    public get RedirectUrl()  {
        return this.getBasePathUrl(this.providerType?.callbackPath ?? "");
    }

    public get LoggedOutUrl(): string {
        return this.getBasePathUrl(this.providerType?.signedOutCallbackPath ?? "");
    }

    private getBasePathUrl(callbackPath: string) {
        return (new URL(callbackPath.replace("{ProviderName}", this.name), this.config.identityConfig.basePath)).href;
    }

    public get appSsoUrl(): string {
        return `${window.location.origin}?st_sso=${this.name}`;
    }

    public get isExternalProvider(): boolean {
        return this.identityProviderType !== IdentityProviderType.LoginCredentials;
    }

    public setProviderType(providerTypeLookup: IdentityProviderTypeLookup) {
        this.providerType = providerTypeLookup;
    }
}