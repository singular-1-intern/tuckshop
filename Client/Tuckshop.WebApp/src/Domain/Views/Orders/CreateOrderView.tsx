import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import CreateOrderVM from './CreateOrderVM';
import { observer } from 'mobx-react';
import { Link } from '@singularsystems/neo-react/dist/ReactComponents/_Exports';
import { viewOrdersRoute } from '../../DomainRoutes';
import { Data, Misc } from '@singularsystems/neo-core';

class CreateOrderParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class CreateOrderView extends Views.ViewBase<CreateOrderVM, CreateOrderParams> {
   public static params = new CreateOrderParams();

    constructor(props: unknown) {
        super("Create Order", CreateOrderVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {
        return (
            <div>
                <Neo.Card>
                    {this.viewModel.newOrder && (
                        <>
                            <Neo.Form model={this.viewModel.newOrder} showSummaryModal onSubmit={() => this.viewModel.submitOrder()}>
                                {(order, orderMeta) => {
                                    const selectedCustomerId = Number(order.customerId) || 0;
                                    const selectedCustomer = this.viewModel.customers.find(c => c.customerId === selectedCustomerId);

                                    return (
                                        <>
                                            <div className="row g-3 align-items-center mb-3">
                                                {!selectedCustomer && !this.viewModel.myOrdersDisplay && (
                                                    <div className="login-screen">
                                                        <h1>Login</h1>
                                                        <div className="col-md-8">
                                                            <Neo.FormGroup bind={orderMeta.customerId} select={{ items: this.viewModel.customers, valueMember: "customerId", displayMember: "customerName" }} />
                                                        </div>
                                                    </div>
                                                )}

                                                {selectedCustomer && !this.viewModel.myOrdersDisplay && (
                                                    <>
                                                        <div className="d-flex justify-content-between">
                                                            <button
                                                                type="button"
                                                                className="btn btn-link btn-sm text-decoration-none mb-2 text-start ps-0 col-md-1"
                                                                onClick={() => {
                                                                    order.customerId = 0;
                                                                    this.viewModel.clearSelectedCustomer();
                                                                }}>
                                                                Logout
                                                            </button>
                                                            <button
                                                                type="button"
                                                                className="btn btn-link btn-sm text-decoration-none mb-2 text-start ps-0 col-md-1"
                                                                onClick={() => this.viewModel.showMyOrdersForCustomer(selectedCustomer.customerId)}>
                                                                View my orders
                                                            </button>
                                                        </div>
                                                        <div className="col-md-12 d-flex flex-direction-column flex-md-row align-items-center justify-content-center">
                                                            <div className="col-md-8">
                                                                <h1>Welcome back, <em>{selectedCustomer.customerName}</em></h1>
                                                            </div>

                                                            <div className="col-md-4 d-flex flex-column align-items-md-end align-items-start gap-2">
                                                                <div className="manage-wallet-btn">
                                                                    <Neo.Modal
                                                                        title="Wallet Action"
                                                                        bind={this.viewModel.meta.showBasicModal}
                                                                    >
                                                                        <Neo.FormGroup bind={this.viewModel.meta.walletAmount} label="Amount" />
                                                                        <div className="mt-2">
                                                                            <Neo.Button
                                                                                variant="secondary"
                                                                                icon="credit-card"
                                                                                onClick={() => this.viewModel.startPaystackCheckout(selectedCustomer.customerId)}>
                                                                                {this.viewModel.walletAction === 'deposit' ? 'Deposit' : 'Withdraw'} with Paystack
                                                                            </Neo.Button>
                                                                        </div>
                                                                    </Neo.Modal>
                                                                    <Neo.Button
                                                                        className="ms-1"
                                                                        menuAlignment="right"
                                                                        menuItems={[
                                                                            { text: "Deposit", icon: "money", onClick: () => this.viewModel.depositFunds(selectedCustomer.customerId) },
                                                                            { text: "Withdraw", icon: "money", onClick: () => this.viewModel.withdrawFunds(selectedCustomer.customerId) }
                                                                        ]}>
                                                                        <>Wallet balance: <strong>{`R ${selectedCustomer.walletBalance}`}</strong></>
                                                                    </Neo.Button>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </>
                                                )}
                                            </div>

                                            {selectedCustomer && !this.viewModel.myOrdersDisplay && (
                                                <>
                                                    <NeoGrid.Grid items={order.orderDetails}>
                                                        {(orderDetail, orderDetailMeta) => (
                                                            <NeoGrid.Row>
                                                                <NeoGrid.Column display={orderDetailMeta.productName} />
                                                                <NeoGrid.Column display={orderDetailMeta.price} />
                                                                <NeoGrid.Column display={orderDetailMeta.value} sum />
                                                                <NeoGrid.Column bind={orderDetailMeta.quantity} />
                                                            </NeoGrid.Row>
                                                        )}
                                                    </NeoGrid.Grid>
                                                    <div className="text-right mt-3">
                                                        <Neo.Button isSubmit size="lg" icon="coffee">Place Order</Neo.Button>
                                                    </div>
                                                </>
                                            )}
                                        </>
                                    );
                                }}
                            </Neo.Form>

                            {this.viewModel.myOrdersDisplay && (
                                <div>
                                    <button type="button" className="btn btn-link btn-sm text-decoration-none mb-2 text-start ps-0 col-md-1" onClick={() => this.viewModel.backToShop()}>Back to shop</button>
                                    <Neo.Card title="Criteria">
                                        <Neo.Form model={this.viewModel.criteria} onSubmit={() => this.viewModel.findOrders()}>
                                            {(crit, critMeta) => (
                                                <Neo.GridLayout md={2} lg={4}>
                                                    <Neo.FormGroup bind={critMeta.customerName} readOnly />
                                                    <Neo.Button icon="search" className="form-btn" isSubmit>Search</Neo.Button>
                                                </Neo.GridLayout>
                                            )}
                                        </Neo.Form>
                                    </Neo.Card>

                                    <Neo.Card title="Orders">
                                        <NeoGrid.Grid items={this.viewModel.foundOrders}>
                                            {(order, orderMeta) => (
                                                <NeoGrid.RowGroup expandProperty={orderMeta.isExpanded}>
                                                    <NeoGrid.Row>
                                                        <NeoGrid.Column display={orderMeta.customerName} />
                                                        <NeoGrid.Column display={orderMeta.orderedOn} dateProps={{ formatString: "dd MMM - HH:mm" }} />
                                                        <NeoGrid.Column display={orderMeta.orderTotal} numProps={{ format: Misc.NumberFormat.CurrencyDecimals }} />
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
                            )}
                        </>
                    )}

                    {!this.viewModel.newOrder && (
                        <Neo.Alert variant="success" heading="Order submitted" animateInitial className="mt-4">
                            Your order has been submitted, <Neo.Button variant="link" className="btn-link-inline" onClick={() => {
                                if (this.viewModel.selectedCustomerId > 0) {
                                    this.viewModel.showMyOrdersForCustomer(this.viewModel.selectedCustomerId);
                                }
                            }}>view your orders here</Neo.Button>,
                            {' '}or <Neo.Button variant="link" className="btn-link-inline" onClick={() => this.viewModel.setupOrder()}>create another order</Neo.Button>.
                        </Neo.Alert>
                    )}
                </Neo.Card>
            </div>
        );
    }
}