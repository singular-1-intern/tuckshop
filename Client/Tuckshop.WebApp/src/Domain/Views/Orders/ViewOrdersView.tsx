import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import ViewOrdersVM from './ViewOrdersVM';
import { observer } from 'mobx-react';
import { OrderStatus } from '../../Models/Orders/Enums/OrderStatus';
import { Data, Misc } from '@singularsystems/neo-core';

class ViewOrdersParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class ViewOrdersView extends Views.ViewBase<ViewOrdersVM, ViewOrdersParams> {
   public static params = new ViewOrdersParams();

    constructor(props: unknown) {
        super("View Orders", ViewOrdersVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {
        return (
            <div>
                <Neo.Card title="Criteria">
                    <Neo.Form model={this.viewModel.criteria} onSubmit={() => this.viewModel.findOrders()}>
                    {(crit, critMeta) => (
                        <Neo.GridLayout md={2} lg={4}>
                            <Neo.FormGroup bind={critMeta.orderStatus} select={{itemSource: Data.StaticDataSource.fromEnum(OrderStatus)}} />
                            <Neo.FormGroup bind={critMeta.startDate} />
                            <Neo.FormGroup bind={critMeta.endDate} />
                            <Neo.Button icon="search" className="form-btn" isSubmit>Search</Neo.Button>
                        </Neo.GridLayout>
                    )}
                    </Neo.Form>
                </Neo.Card>
                <Neo.Card title="Orders">
                <NeoGrid.Grid items={this.viewModel.foundOrders}>
                    {(order, orderMeta) => (
                        <NeoGrid.RowGroup expandProperty={orderMeta.isExpanded} >
                            <NeoGrid.Row>
                                <NeoGrid.Column display={orderMeta.customerName} />
                                <NeoGrid.Column display={orderMeta.orderedOn} dateProps={{formatString: "dd MMM - HH:mm"}} />
                                <NeoGrid.Column display={orderMeta.orderTotal} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} />
                            </NeoGrid.Row>
                            <NeoGrid.ChildRow>
                                <NeoGrid.Grid items={order.items}>
                                {(orderDetail, orderDetailMeta) => (
                                    <NeoGrid.Row>
                                        <NeoGrid.Column display={orderDetailMeta.product} />
                                        <NeoGrid.Column display={orderDetailMeta.vat} />
                                        <NeoGrid.Column display={orderDetailMeta.value} />
                                    </NeoGrid.Row>
                                )} 
                                </NeoGrid.Grid>
                            </NeoGrid.ChildRow>
                        </NeoGrid.RowGroup>
                    )}
                </NeoGrid.Grid>
            </Neo.Card>
            </div>
        );
    }
}