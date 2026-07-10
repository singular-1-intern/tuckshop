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
        <Neo.Card title="Products">
          <Neo.Form
            model={this.viewModel.products}
            onSubmit={() => this.viewModel.saveProducts()}
            showSummaryModal
          >
            <div className="mb-3">
              <Neo.Button isSubmit variant="success" icon="check">
                Save
              </Neo.Button>
            </div>

            <NeoGrid.Grid items={this.viewModel.products} showAddButton>
              {(product, productMeta, rowIndex) => (
                <NeoGrid.Row key={`${product.productId || 'new'}-${rowIndex}`}>
                  {/* <NeoGrid.Column display={productMeta.productId} /> */}
                  <NeoGrid.Column bind={productMeta.productName}  />
                  <NeoGrid.Column
                    bind={productMeta.price}
                    numProps={{ format: Misc.NumberFormat.CurrencyDecimals }}
                  />
                  <NeoGrid.Column bind={productMeta.imageUrl} />
                  <NeoGrid.Column
                    display={productMeta.imageUrl}
                    label="Image"
                    width={70}
                    suppressDefaultContent={!!product.imageUrl}
                  >
                    {product.imageUrl && (
                      <img
                        src={product.imageUrl}
                        alt={product.productName || "Product image"}
                        style={{
                          width: "50px",
                          height: "50px",
                          objectFit: "cover",
                          borderRadius: "4px"
                        }}
                      />
                    )}
                  </NeoGrid.Column>
                  <NeoGrid.ButtonColumn deleteButton={{ disabled: true }} />
                </NeoGrid.Row>
              )}
            </NeoGrid.Grid>
          </Neo.Form>
        </Neo.Card>
      </div>
    );
  }
}
