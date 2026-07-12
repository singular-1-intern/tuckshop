import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class Customer extends ModelBase {
    static typeName = "Customer";

    constructor() {
        super();
        this.makeObservable();
    }

    public customerId: number = 0;

    @Rules.Required()
    @Rules.StringLength(100)
    public customerName: string = "";

    @Attributes.Float()
    public walletBalance: number = 0;

    @Rules.Required()
    @Rules.StringLength(10)
    public customerCellNo: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<Customer>) {
        super.addBusinessRules(rules);
        rules.failWhen(c => c.customerCellNo.length !== 10, "Customer cell number must be 10 digits.");
        rules.failWhen(c => c.customerCellNo.length > 0 && !/^\d+$/.test(c.customerCellNo), "Customer cell number must be numeric.");
    }

    public toString(): string {
        if (this.isNew || !this.customerName) {
            return "New customer";
        } else {
            return this.customerName;
        }
    }
}