import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class DepositFunds extends ModelBase {
    static typeName = "DepositFunds";

    constructor() {
        super();
        this.makeObservable();
    }

    @Rules.Required()
    public customerId: number = 0;

    @Attributes.Float()
    @Rules.Required()
    public amount: number = 0;

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<DepositFunds>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew) {
            return "New deposit funds";
        } else {
            return "Deposit Funds";
        }
    }
}