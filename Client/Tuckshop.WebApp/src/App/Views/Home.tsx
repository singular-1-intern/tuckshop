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
                    <Neo.Card title={
                      <div className="d-inline-flex align-items-center gap-3 mt-1">
                        <span className="material-symbols-outlined" style={{ fontSize: '2rem' }}>
                          today
                        </span>
                        <span style={{ fontSize: '2rem' }}>Today's Orders</span>
                      </div>
                    }>
                        {this.viewModel.todaysOrders.length > 0 ? (
                          <NeoGrid.Grid items={this.viewModel.todaysOrders}>
                          {(order, orderMeta) => (
                              <NeoGrid.RowGroup expandProperty={orderMeta.isExpanded} >
                                  <NeoGrid.Row>
                                      <NeoGrid.Column display={orderMeta.customerName} alignment={'left'}/>
                                      <NeoGrid.Column display={orderMeta.orderTotal} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} alignment={'center'}/>
                                      <NeoGrid.Column label="Status" alignment={'center'}>
                                        <Neo.Badge variant={this.viewModel.getOrderStatusVariant(order)} className="d-inline-flex align-items-center gap-2" isLarge>
                                          <span className="material-symbols-outlined" style={{ fontSize: '1rem', lineHeight: 1 }}>
                                            {this.viewModel.getOrderStatusSymbol(order)}
                                          </span>
                                          <span>{this.viewModel.getOrderStatusText(order)}</span>
                                        </Neo.Badge>
                                      </NeoGrid.Column>
                                  </NeoGrid.Row>
                                  <NeoGrid.ChildRow>
                                      <NeoGrid.Grid items={order.items}>
                                      {(orderDetail, orderDetailMeta) => (
                                          <NeoGrid.Row>
                                              <NeoGrid.Column display={orderDetailMeta.product} alignment={'left'}/>
                                              <NeoGrid.Column display={orderDetailMeta.quantity} alignment={'left'}/>
                                              <NeoGrid.Column display={orderDetailMeta.vat} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} alignment={'left'} sum/>
                                              <NeoGrid.Column display={orderDetailMeta.value} numProps={{format: Misc.NumberFormat.CurrencyDecimals}}alignment={'left'} sum/>
                                          </NeoGrid.Row>
                                      )} 
                                      </NeoGrid.Grid>
                                  </NeoGrid.ChildRow>
                              </NeoGrid.RowGroup>
                          )}
                            </NeoGrid.Grid>
                        ) : (
                          <div className="text-muted">No orders today.</div>
                        )}
                    </Neo.Card>

                    <Neo.Card title={
                      <div className="d-inline-flex align-items-center gap-3 mt-1">
                        <span className="material-symbols-outlined" style={{ fontSize: '2rem' }}>
                          hourglass
                        </span>
                        <span style={{ fontSize: '2rem' }}>Pending Orders</span>
                      </div>
                    }>
                      {this.viewModel.foundOrders.length > 0 ? (
                        <NeoGrid.Grid items={this.viewModel.foundOrders}>
                          {(order, orderMeta) => (
                            <NeoGrid.RowGroup expandProperty={orderMeta.isExpanded} >
                              <NeoGrid.Row>
                                <NeoGrid.Column display={orderMeta.customerName} alignment={'left'}/>
                                <NeoGrid.Column display={orderMeta.orderedOn} dateProps={{formatString: "dd/MM - HH:mm"}} alignment={'center'}/>
                                <NeoGrid.ButtonColumn alignment={'center'}>
                                  {order.canAction &&
                                  <>
                                      <Neo.Button variant="danger" icon="times" onClick={() => this.cancelOrder(order)}>
                                        <span className="material-symbols-outlined">close</span>
                                      </Neo.Button>
                                      <Neo.Button variant="success" icon="check" onClick={() => this.completeOrder(order)}>
                                        <span className="material-symbols-outlined">check</span>
                                      </Neo.Button>
                                  </>
                                  }
                                </NeoGrid.ButtonColumn>
                              </NeoGrid.Row>
                            <NeoGrid.ChildRow>
                                  <NeoGrid.Grid items={order.items}>
                                  {(orderDetail, orderDetailMeta) => (
                                      <NeoGrid.Row>
                                          <NeoGrid.Column display={orderDetailMeta.product}  alignment={'left'}/>
                                          <NeoGrid.Column display={orderDetailMeta.quantity} alignment={'center'}/>
                                          <NeoGrid.Column display={orderDetailMeta.value} numProps={{format: Misc.NumberFormat.CurrencyDecimals}} alignment={'center'} sum/>
                                      </NeoGrid.Row>
                                  )} 
                                  </NeoGrid.Grid>
                              </NeoGrid.ChildRow>
                            </NeoGrid.RowGroup> 
                          )}
                        </NeoGrid.Grid>
                      ) : (
                        <div className="text-muted">No pending orders.</div>
                      )}
                    </Neo.Card>

                    <Neo.Card title={
                      <div className="d-inline-flex align-items-center gap-3 mt-1 mx-auto">
                        <span className="material-symbols-outlined" style={{ fontSize: '2rem' }}>
                          inventory_2
                        </span>
                        <span style={{ fontSize: '2rem' }}>Inventory Status</span>
                      </div>
                    }>
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