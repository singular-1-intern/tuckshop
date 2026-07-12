import { Attributes, LookupBase } from '@singularsystems/neo-core';

export default class OrderDetailLookup extends LookupBase {

    constructor() {
        super();
        this.makeBindable();
    }

    public readonly product: string = "";

    @Attributes.Float()
    public readonly price: number = 0;

    @Attributes.Float()
    public readonly value: number = 0;

    @Attributes.Float()
    public readonly vat: number = 0;

    @Attributes.Integer()
    public readonly quantity: number = 0;

    // Client only properties / methods
}