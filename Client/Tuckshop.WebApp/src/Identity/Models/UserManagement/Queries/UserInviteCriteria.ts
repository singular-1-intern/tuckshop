import { NeoModel, Validation, ValueObject } from '@singularsystems/neo-core';

@NeoModel
export default class UserInviteCriteria extends ValueObject {

    public includeRegistered: boolean = false;

    // Client only properties / methods

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return "User Invite Criteria";
    }
}