import { NeoModel, Validation, ValueObject } from '@singularsystems/neo-core';

@NeoModel
export default class UserManagementActionLogLookupCriteria extends ValueObject {

    public userId: string = "";

    // Client only properties / methods

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return "User Management Action Log Lookup Criteria";
    }
}