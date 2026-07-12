import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import CustomersVM from './CustomersVM';
import { observer } from 'mobx-react';
import { Misc } from '@singularsystems/neo-core';

class CustomersParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class CustomersView extends Views.ViewBase<CustomersVM, CustomersParams> {
   public static params = new CustomersParams();

    constructor(props: unknown) {
        super("Customers", CustomersVM, props);
    }

    protected viewParamsUpdated() {}

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
                            <NeoGrid.Column bind={customerMeta.customerName} alignment={'left'} />
                            <NeoGrid.Column title={'Cellphone Number'} bind={customerMeta.customerCellNo} alignment={'left'} />
                            <NeoGrid.Column display={customerMeta.walletBalance} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} alignment={'center'} />
                            <NeoGrid.ButtonColumn showDelete deleteButton={{ disabled: true }} alignment={'center'} />
                            </NeoGrid.Row>
                        )}
                        </NeoGrid.Grid>
                    </Neo.Form>
                </Neo.Card>
            </div>
        );
    }
}