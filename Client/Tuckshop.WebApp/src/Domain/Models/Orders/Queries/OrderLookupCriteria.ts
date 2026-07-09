import { Attributes, Misc, Validation, ValueObject } from '@singularsystems/neo-core';
import { OrderStatus } from '../Enums/OrderStatus';

export default class OrderLookupCriteria extends ValueObject {

    constructor() {
        super();
        this.makeObservable();
    }

    public orderStatus: OrderStatus | null = null;

    @Attributes.Date(Misc.TimeZoneFormat.None)
    public startDate: Date | null = null;

    @Attributes.Date(Misc.TimeZoneFormat.None)
    public endDate: Date | null = null;

    public customerName: String | null = null;

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<OrderLookupCriteria>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return "Order Lookup Criteria";
    }
}