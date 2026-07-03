import { NeoModel, Rules, Validation, ValueObject } from '@singularsystems/neo-core';

@NeoModel
export default class ExampleCriteria extends ValueObject {

    @Rules.Required()
    public searchString: string | null = null;

    // Client only properties / methods

    protected addBusinessRules(rules: Validation.Rules<this>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return "Example Criteria";
    }
}