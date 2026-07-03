import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import CustomersVM from './CustomersVM';
import { observer } from 'mobx-react';

class CustomersParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class CustomersView extends Views.ViewBase<CustomersVM, CustomersParams> {
   public static params = new CustomersParams();

    constructor(props: unknown) {
        super("Customers", CustomersVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {
        return (
            <div>
			    <Neo.Card title="Customers">
                    <Neo.Form
                        model={this.viewModel.customers}
                        onSubmit={() => this.viewModel.saveCustomers()}
                        showSummaryModal
                    >
                        <div className="mb-3">
                            <Neo.Button isSubmit variant="success" icon="check">
                                Save
                            </Neo.Button>
                        </div>

                        <NeoGrid.Grid items={this.viewModel.customers} showAddButton>
                        {(customer, customerMeta) => (
                            <NeoGrid.Row>
                            <NeoGrid.Column display={customerMeta.customerId} />
                            <NeoGrid.Column bind={customerMeta.customerName} />
                            <NeoGrid.ButtonColumn showDelete />
                            </NeoGrid.Row>
                        )}
                        </NeoGrid.Grid>
                    </Neo.Form>
                </Neo.Card>
            </div>
        );
    }
}