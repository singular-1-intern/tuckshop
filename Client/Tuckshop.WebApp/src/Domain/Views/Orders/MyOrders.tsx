import React from 'react';
import { observer } from 'mobx-react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import { Data, Misc } from '@singularsystems/neo-core';
import { OrderStatus } from '../../Models/Orders/Enums/OrderStatus';
import ViewOrdersVM from './ViewOrdersVM';

interface IMyOrdersProps {
    
}

@observer
export default class MyOrders extends Views.ViewBase<ViewOrdersVM, {}> {

    constructor(props: IMyOrdersProps) {
        super("My Orders", ViewOrdersVM, props);
    }

    public render() {
        return (
            <div>
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
            </div>
        );
    }
}