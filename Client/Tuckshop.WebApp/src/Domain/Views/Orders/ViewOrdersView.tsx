import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import ViewOrdersVM from './ViewOrdersVM';
import { observer } from 'mobx-react';
import { OrderStatus } from '../../Models/Orders/Enums/OrderStatus';
import { Data, Misc, ModalUtils } from '@singularsystems/neo-core';
import CancelOrder from '../../Models/Orders/Commands/CancelOrder';
import OrderLookup from '../../Models/Orders/Queries/OrderLookup';

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

    private completeOrder(order: OrderLookup) {
        ModalUtils.showYesNo("Complete order", "Are you sure you want to complete this order?", 
                () => this.viewModel.completeOrder(order));
    }

    private async cancelOrder(order: OrderLookup) {
        const cancelInfo = new CancelOrder();

        if ((await ModalUtils.showOkCancel(
            "Cancel order",
            <Neo.FormGroup bind={cancelInfo.meta.reason} label="Please enter a reason:" />, 
            cancelInfo)) === Misc.ModalResult.Yes) {

            this.viewModel.cancelOrder(order, cancelInfo.reason);
        }
    }

    public render() {
        return (
            <div>
                <Neo.Card title="Criteria">
                    <Neo.Form model={this.viewModel.criteria} onSubmit={() => this.viewModel.findOrders()}>
                    {(crit, critMeta) => (
                        <Neo.GridLayout md={2} lg={4}>
                            <Neo.FormGroup bind={critMeta.customerName} select={{ items: this.viewModel.customers, valueMember: "customerName", displayMember: "customerName", allowNulls: true }}/>
                            <Neo.FormGroup bind={critMeta.orderStatus} select={{itemSource: Data.StaticDataSource.fromEnum(OrderStatus), allowNulls: true}} />
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
                                <NeoGrid.Column display={orderMeta.orderedOn} dateProps={{formatString: "dd MMM - HH:mm"}} alignment={'center'}/>
                                <NeoGrid.Column display={orderMeta.orderTotal} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} alignment={'center'} />
                                <NeoGrid.ButtonColumn alignment={'center'}>
                                    {order.canAction &&
                                    <>
                                        <Neo.Button variant="danger" icon="times" onClick={() => this.cancelOrder(order)}>Cancel</Neo.Button>
                                        <Neo.Button variant="success" icon="check" onClick={() => this.completeOrder(order)}>Complete</Neo.Button>
                                    </>
                                    }
                                </NeoGrid.ButtonColumn>
                            </NeoGrid.Row>
                            <NeoGrid.ChildRow>
                                <NeoGrid.Grid items={order.items}>
                                {(orderDetail, orderDetailMeta) => (
                                    <NeoGrid.Row>
                                        <NeoGrid.Column display={orderDetailMeta.product} alignment={'center'} />
                                        <NeoGrid.Column display={orderDetailMeta.quantity} alignment={'center'} />
                                        <NeoGrid.Column display={orderDetailMeta.vat} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} sum alignment={'center'} />
                                        <NeoGrid.Column display={orderDetailMeta.value} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} sum alignment={'center'} />
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