import * as React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import { observer } from 'mobx-react';
import { Chart } from '@highcharts/react';
import HomeVM from './HomeVM';
import { Misc, ModalUtils } from '@singularsystems/neo-core';
import OrderLookup from '../../Domain/Models/Orders/Queries/OrderLookup';
import CancelOrder from '../../Domain/Models/Orders/Commands/CancelOrder';

@observer
export default class Home extends Views.ViewBase<HomeVM> {

    constructor(props: unknown) {
        super("", HomeVM, props);
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
                <Neo.GridLayout lg={3}>

                    <Neo.Card title="Pending Orders">
                      <NeoGrid.Grid items={this.viewModel.foundOrders}>
                        {(order, orderMeta) => (
                          <NeoGrid.RowGroup expandProperty={orderMeta.isExpanded} >
                            <NeoGrid.Row>
                              <NeoGrid.Column display={orderMeta.customerName} />
                              <NeoGrid.Column display={orderMeta.orderedOn} dateProps={{formatString: "dd/MM - HH:mm"}}/>
                              {/* <NeoGrid.Column display={orderMeta.orderTotal} numProps={{format: Misc.NumberFormat.CurrencyDecimals}}/> */}
                              <NeoGrid.ButtonColumn>
                                {order.canAction &&
                                <>
                                    <Neo.Button variant="danger" icon="times" onClick={() => this.cancelOrder(order)}>
                                      <span className="material-symbols-outlined">cancel</span>
                                    </Neo.Button>
                                    <Neo.Button variant="success" icon="check" onClick={() => this.completeOrder(order)}>
                                      <span className="material-symbols-outlined">task_alt</span>
                                    </Neo.Button>
                                </>
                                }
                              </NeoGrid.ButtonColumn>
                            </NeoGrid.Row>
                          <NeoGrid.ChildRow>
                                <NeoGrid.Grid items={order.items}>
                                {(orderDetail, orderDetailMeta) => (
                                    <NeoGrid.Row>
                                        <NeoGrid.Column display={orderDetailMeta.product} />
                                        <NeoGrid.Column display={orderDetailMeta.vat} sum/>
                                        <NeoGrid.Column display={orderDetailMeta.value} sum/>
                                    </NeoGrid.Row>
                                )} 
                                </NeoGrid.Grid>
                            </NeoGrid.ChildRow>
                          </NeoGrid.RowGroup> 
                        )}
                      </NeoGrid.Grid>
                    </Neo.Card>

                    <Neo.Card title="Today's Orders">
                        
                    </Neo.Card>

                    <Neo.Card title="Stock Levels">
                      {this.viewModel.hasProducts ? (
                        <Chart options={this.viewModel.stockBarChartOptions} />
                      ) : (
                        <div className="text-muted">No products available.</div>
                      )}
                    </Neo.Card>

                </Neo.GridLayout>
            </div>
        )
    }
}