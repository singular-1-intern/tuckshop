import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import CreateOrderVM from './CreateOrderVM';
import { observer } from 'mobx-react';
import { Misc } from '@singularsystems/neo-core';

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
                {this.viewModel.newOrder && (
                        <Neo.Form model={this.viewModel.newOrder} showSummaryModal onSubmit={() => this.viewModel.submitOrder()}>
                            {(order, orderMeta) => {
                                const selectedCustomerId = Number(order.customerId) || 0;
                                const selectedCustomer = this.viewModel.customers.find(c => c.customerId === selectedCustomerId);

                                return (
                                    <>
                                        <Neo.Card className="mb-2 p-10 shadow rounded-4">
                                            <div className="row g-3 align-items-center">
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

                                                        <div className="col-md-12 d-flex flex-column flex-md-row align-items-center justify-content-between gap-3">
                                                            <div className="col-md-8">
                                                                <h1 className="mb-0">Welcome back, <em>{selectedCustomer.customerName}</em></h1>
                                                            </div>

                                                            <div className="col-md-4 d-flex flex-column align-items-md-end align-items-start gap-2">
                                                                <div className="manage-wallet-btn d-flex flex-wrap gap-2 justify-content-md-end">
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
                                        </Neo.Card>

                                        {selectedCustomer && !this.viewModel.myOrdersDisplay && (
                                            <Neo.Card title="Products" className="mt-3 shadow rounded-4">
                                                <div className="row row-cols-1 row-cols-sm-2 row-cols-lg-4 row-cols-xxl-5 g-3">
                                                    {order.orderDetails.map((orderDetail, orderDetailIndex) => (
                                                        <div key={orderDetail.productId || orderDetailIndex} className="col">
                                                            <Neo.Card className="h-100 border-0 shadow rounded-4 overflow-hidden">
                                                                <div className="ratio ratio-1x1 bg-light">
                                                                    {orderDetail.imageUrl ? (
                                                                        <img
                                                                            src={orderDetail.imageUrl}
                                                                            alt={orderDetail.productName || 'Product image'}
                                                                            className="w-100 h-100"
                                                                            style={{ objectFit: 'cover' }}
                                                                        />
                                                                    ) : (
                                                                        <div className="d-flex h-100 w-100 align-items-center justify-content-center text-muted">
                                                                            No image
                                                                        </div>
                                                                    )}
                                                                </div>

                                                                <div className="card-body d-flex flex-column gap-2 p-3">
                                                                    <div className="fw-semibold fs-6 text-truncate">{orderDetail.productName}</div>

                                                                    <div className="d-flex align-items-center justify-content-between">
                                                                        <span className="text-muted">Price</span>
                                                                        <span className="fw-semibold">R {orderDetail.price.toFixed(2)}</span>
                                                                    </div>

                                                                    <div className="d-flex align-items-center justify-content-between gap-3">
                                                                        <span className="text-muted">Quantity</span>
                                                                        <div className="d-flex align-items-center gap-2">
                                                                            <Neo.Button
                                                                                variant="secondary"
                                                                                icon="far-minus"
                                                                                className="rounded-circle d-inline-flex align-items-center justify-content-center p-0"
                                                                                style={{ width: '1.75rem', height: '1.75rem', minWidth: '1.75rem', minHeight: '1.75rem', lineHeight: '1' }}
                                                                                onClick={() => this.viewModel.decrementOrderDetailQuantity(orderDetail)}
                                                                                disabled={orderDetail.quantity <= 0}
                                                                            />
                                                                            <span className="fw-semibold text-center" style={{ minWidth: '1.5rem' }}>{orderDetail.quantity}</span>
                                                                            <Neo.Button
                                                                                variant="secondary"
                                                                                icon="far-plus"
                                                                                className="rounded-circle d-inline-flex align-items-center justify-content-center p-0"
                                                                                style={{ width: '1.75rem', height: '1.75rem', minWidth: '1.75rem', minHeight: '1.75rem', lineHeight: '1' }}
                                                                                onClick={() => this.viewModel.incrementOrderDetailQuantity(orderDetail)}
                                                                            />
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </Neo.Card>
                                                        </div>
                                                    ))}
                                                </div>

                                                <div className="text-right mt-3">
                                                    <Neo.Button isSubmit size="lg" icon="coffee">Place Order</Neo.Button>
                                                </div>
                                            </Neo.Card>
                                        )}
                                    </>
                                );
                            }}
                        </Neo.Form>
                )}

                {this.viewModel.selectedCustomer && this.viewModel.myOrdersDisplay && (
                    <div className="mt-3">
                        <button type="button" className="btn btn-link btn-sm text-decoration-none mb-2 text-start ps-0 col-md-1" onClick={() => this.viewModel.backToShop()}>Back to shop</button>
                        <Neo.Card title="Your Orders">
                            <NeoGrid.Grid items={this.viewModel.foundOrders}>
                                {(order, orderMeta) => (
                                    <NeoGrid.RowGroup expandProperty={orderMeta.isExpanded}>
                                        <NeoGrid.Row>
                                            <NeoGrid.Column label='Order Reference' sort={false}>{`Order Number: #${order.orderId}`}</NeoGrid.Column>
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

                {!this.viewModel.newOrder && this.viewModel.isOrderSuccessful && !this.viewModel.myOrdersDisplay && (
                    <Neo.Alert variant="success" heading="Order submitted" animateInitial className="mt-4">
                        Your order has been submitted. <Neo.Button variant="link" className="btn-link-inline" onClick={() => this.viewModel.showSelectedCustomerOrders()}>View your orders here</Neo.Button>
                        {' '}or <Neo.Button variant="link" className="btn-link-inline" onClick={() => this.viewModel.createAnotherOrder()}>create another order</Neo.Button>.
                    </Neo.Alert>
                )}
            </div>
        );
    }
}