import { Attributes, ModelBase, NeoModel, Rules, Validation } from '@singularsystems/neo-core';

@NeoModel
export default class UserInvite extends ModelBase {

    public userInviteId: number = 0;

    @Rules.Required()
    @Rules.EmailAddress()
    @Rules.StringLength(250)
    public emailAddress: string = "";

    @Attributes.Nullable()
    public addToUserGroupId: number | null = null;

    public createdBy: number = 0;

    @Attributes.Date()
    public createdOn: Date = new Date();

    // Client only properties / methods

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.emailAddress) {
            return "New invited user";
        } else {
            return this.emailAddress;
        }
    }
}