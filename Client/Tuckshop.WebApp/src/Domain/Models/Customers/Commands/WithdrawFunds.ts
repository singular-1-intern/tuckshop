import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class WithdrawFunds extends ModelBase {
    static typeName = "WithdrawFunds";

    constructor() {
        super();
        this.makeObservable();
    }

    @Rules.Required()
    public customerId: number = 0;

    @Attributes.Float()
    public amount: number = 0;

    @Rules.StringLength(255)
    public reason: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<WithdrawFunds>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.reason) {
            return "New withdraw funds";
        } else {
            return this.reason;
        }
    }
}