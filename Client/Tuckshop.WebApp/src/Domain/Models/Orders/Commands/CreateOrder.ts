import { Attributes, List, ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export class CreateOrder extends ModelBase {
  static typeName = "CreateOrder";

  constructor() {
    super();
    this.makeObservable();
  }

    @Rules.StringLength(50)
    @Rules.Required()
    public customerName: string = "";

    public orderDetails = new List(NewOrderDetail);

  // Client only properties / methods

  protected static addBusinessRules(rules: Validation.Rules<CreateOrder>) {
    super.addBusinessRules(rules);
    rules.failWhen(c => !c.orderDetails.find(od => od.quantity > 0), "You must order at least one item.");
  }

  public toString(): string {
    if (this.isNew || !this.customerName) {
      return "New create order";
    } else {
      return this.customerName;
    }
  }
}

export class NewOrderDetail extends ModelBase {
  static typeName = "NewOrderDetail";

  constructor() {
      super();
      this.makeObservable();
  }
  public productId: number = 0;

  @Attributes.Integer()
  public quantity: number = 0;

  // Client only properties / methods
  @Attributes.NoTracking()
  public productName: string = "";

  @Attributes.NoTracking()
  @Attributes.Float()
  public price: number = 0;

  @Attributes.Float()
  public get value() {
      return this.quantity * this.price;
  }

  protected canSerialise(shouldSerialise: boolean) {
    return this.quantity > 0;
  }

  protected static addBusinessRules(rules: Validation.Rules<NewOrderDetail>) {
      super.addBusinessRules(rules);
  }

  public toString(): string {
      if (this.isNew) {
          return "New new order detail";
      } else {
          return "New Order Detail";
      }
  }
}