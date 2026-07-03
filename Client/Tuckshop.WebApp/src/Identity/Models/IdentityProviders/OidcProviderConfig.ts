import { NeoModel, Rules, Validation, ValueObject } from '@singularsystems/neo-core';

@NeoModel
export default class OidcProviderConfig extends ValueObject {

    @Rules.StringLength(250)
    @Rules.Required()
    public authority: string = "";

    @Rules.StringLength(100)
    @Rules.Required()
    public clientId: string = "";

    @Rules.StringLength(100)
    public clientSecret: string = "";

    @Rules.StringLength(100)
    @Rules.Required()
    public nameClaimType: string = "name";

    @Rules.StringLength(100)
    @Rules.Required()
    public roleClaimType: string = "role";

    @Rules.StringLength(100)
    @Rules.Required()
    public scopes: string = "openid profile email";

    // Client only properties / methods

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);

        rules.failWhen(
            c => !c.authority.match(/^(?:(?:(?:https):)?\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z0-9\u00a1-\uffff][a-z0-9\u00a1-\uffff_-]{0,62})?[a-z0-9\u00a1-\uffff]\.)+(?:[a-z\u00a1-\uffff]{2,}\.?))(?::\d{2,5})?(?:[/?#]\S*)?$/i),
            `Identity Provider authority must be an https url.`);
    }

    public toString(): string {
        return "Oidc Provider Config";
    }
}