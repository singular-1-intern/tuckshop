import { Attributes, ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class Product extends ModelBase {
    static typeName = "Product";

    constructor() {
        super();
        this.makeObservable();
    }

    public productId: number = 0;

    @Rules.Required()
    @Rules.StringLength(100)
    public productName: string = "";

    @Attributes.Float()
    @Rules.Required()
    public price: number = 0;

    @Rules.StringLength(300)
    public imageUrl: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<Product>) {
        super.addBusinessRules(rules);
        rules.failWhen(c => c.price <= 0, "Price must be above zero.");
    }

    public toString(): string {
        if (this.isNew || !this.productName) {
            return "New product";
        } else {
            return this.productName;
        }
    }
}