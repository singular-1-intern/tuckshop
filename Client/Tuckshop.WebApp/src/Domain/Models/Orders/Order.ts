import { Attributes, List, Model, ModelBase, Rules, Validation } from '@singularsystems/neo-core';
import OrderDetail from './OrderDetail';

export default class Order extends ModelBase {
    static typeName = "Order";

    constructor() {
        super();
        this.makeObservable();
    }

    public orderId: number = 0;

    @Rules.Required()
    @Attributes.Date()
    public orderedOn: Date | null = new Date();

    @Rules.Required()
    @Rules.StringLength(100)
    public customerName: string = "";

    @Attributes.ChildObject(Model.UserEvent, true)
    public completed: Model.UserEvent = Model.UserEvent.None();

    @Attributes.ChildObject(Model.ReasonedUserEvent, true)
    public cancelled: Model.ReasonedUserEvent = Model.ReasonedUserEvent.None();

    public orderDetails = new List(OrderDetail);

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<Order>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.customerName) {
            return "New order";
        } else {
            return this.customerName;
        }
    }
}