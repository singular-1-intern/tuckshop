import { Views } from "@singularsystems/neo-react";
import { AppService, Types } from "../DomainTypes";
import Product from "../Models/Product";
import { List } from "@singularsystems/neo-core";

export default class ProductsVM extends Views.ViewModelBase {
  public get productsApiClient() {
    return this._productsApiClient;
  }
  public set productsApiClient(value) {
    this._productsApiClient = value;
  }
  constructor(
    taskRunner = AppService.get(Types.Neo.TaskRunner),
    private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
    private _productsApiClient = AppService.get(Types.Domain.ApiClients.ProductsApiClient),
    private dataCache = AppService.get(Types.Domain.Services.DataCache)

  ) {
    super(taskRunner);
    this.makeObservable();
  }

  public products = new List(Product);

  public async initialise() {
    const response = await this.taskRunner.waitFor(this.productsApiClient.get());
    this.products.set(response.data);
  }

  public saveProducts() {
    this.taskRunner.run(async () => {
      // const productData = this.products.toJSArray();
      const response = await this.productsApiClient.saveList(this.products.toJSArray());
      this.products.update(response.data);
      this.notifications.addSuccess(
        "Products saved",
        "Products saved successfully",
        4,
      );
      this.dataCache.products.expire();
    });
  }
}
