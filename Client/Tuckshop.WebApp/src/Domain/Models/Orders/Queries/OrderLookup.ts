import { Attributes, List, LookupBase } from '@singularsystems/neo-core';
import OrderDetail from '../OrderDetail';
import OrderDetailLookup from './OrderDetailLookup';

export default class OrderLookup extends LookupBase {

    constructor() {
        super();
        this.makeBindable();
    }

    public readonly orderId: number = 0;

    public readonly customerName: string = "";

    @Attributes.Date()
    public readonly orderedOn: Date = new Date();

    @Attributes.Observable()
    @Attributes.Date()
    public completedOn: Date | null = null;

    @Attributes.Observable()
    @Attributes.Date()
    public cancelledOn: Date | null = null;

    public cancelledReason: string = "";

    public readonly completedBy: string = "";

    public readonly cancelledBy: string = "";

    @Attributes.Float()
    public readonly orderTotalExcl: number = 0;

    @Attributes.Float()
    public readonly orderTotal: number = 0;

    public readonly items = new List(OrderDetailLookup);

    // Client only properties / methods
    @Attributes.Observable()
    public isExpanded = false;

    public get canAction() {
        return !this.completedOn && !this.cancelledOn;
    }
}