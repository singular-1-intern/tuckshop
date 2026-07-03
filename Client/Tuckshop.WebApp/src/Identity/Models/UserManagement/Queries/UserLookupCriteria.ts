import { Attributes, NeoModel, Validation, ValueObject } from '@singularsystems/neo-core';

@NeoModel
export default class UserLookupCriteria extends ValueObject {

    public userId: string = "";
    
    public firstName: string = "";

    public lastName: string = "";

    public userName: string = "";

    @Attributes.NullableBoolean()
    public isActive: boolean | null = null;

    // Client only properties / methods

    public clear() {
        this.firstName = "";
        this.lastName = "";
        this.userName = "";
    }

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return "User Lookup Criteria";
    }
}