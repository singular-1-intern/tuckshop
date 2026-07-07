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

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<Customer>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.customerName) {
            return "New customer";
        } else {
            return this.customerName;
        }
    }
}