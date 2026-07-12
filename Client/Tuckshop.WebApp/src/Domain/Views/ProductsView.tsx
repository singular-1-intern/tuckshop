import React from "react";
import { Neo, NeoGrid, Views } from "@singularsystems/neo-react";
import ProductsVM from "./ProductsVM";
import { observer } from "mobx-react";
import { Misc } from "@singularsystems/neo-core";

class ProductsParams {
  // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class ProductsView extends Views.ViewBase<
  ProductsVM,
  ProductsParams
> {
  public static params = new ProductsParams();

  constructor(props: unknown) {
    super("Products", ProductsVM, props);
  }

  protected viewParamsUpdated() {}

  public render() {
    return (
      <div style={{ transition: "opacity 0.5s" }}>
        <Neo.Card title={<span style={{ fontSize: '2rem' }}>Products</span>}>
          <Neo.Form
            model={this.viewModel.products}
            onSubmit={() => this.viewModel.saveProducts()}
            showSummaryModal
          >
            <div className="mb-3">
              <Neo.Button isSubmit variant="primary" icon="check">
                Save
              </Neo.Button>
            </div>

            <NeoGrid.Grid items={this.viewModel.products} showAddButton>
              {(product, productMeta, rowIndex) => (
                <NeoGrid.Row key={`${product.productId || 'new'}-${rowIndex}`}>
                  {/* <NeoGrid.Column display={productMeta.productId} /> */}
                  <NeoGrid.Column bind={productMeta.productName} width={400}/>
                  <NeoGrid.Column
                    bind={productMeta.price} numProps={{ format: Misc.NumberFormat.CurrencyDecimals}} alignment={'left'} width={150} />
                  <NeoGrid.Column
                    display={productMeta.imageUrl}
                    label="Image Url"
                    width={180}
                    suppressDefaultContent
                    alignment={'center'}
                  >
                    {product.imageUrl ? (
                      <img
                        src={product.imageUrl}
                        alt={product.productName || "Product image"}
                        className="product-grid-thumbnail"
                      />
                    ) : (
                      <span
                        className="product-image-url-text"
                        title={product.imageUrl || "No image url"}
                      >
                        {product.imageUrl || "No image url"}
                      </span>
                    )}
                  </NeoGrid.Column>
                  <NeoGrid.Column bind={productMeta.stockCount} alignment={'left'} width={150}/>
                  <NeoGrid.ButtonColumn deleteButton={{ disabled: true }} alignment={'center'} width={150}/>
                </NeoGrid.Row>
              )}
            </NeoGrid.Grid>
          </Neo.Form>
        </Neo.Card>
      </div>
    );
  }
}
