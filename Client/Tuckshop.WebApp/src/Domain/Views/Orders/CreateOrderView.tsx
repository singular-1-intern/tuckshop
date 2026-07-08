import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import CreateOrderVM from './CreateOrderVM';
import { observer } from 'mobx-react';
import { Link } from '@singularsystems/neo-react/dist/ReactComponents/_Exports';
import { viewOrdersRoute } from '../../DomainRoutes';

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
                    {this.viewModel.newOrder && 
                        <Neo.Form model={this.viewModel.newOrder} showSummaryModal onSubmit={() => this.viewModel.submitOrder()}>
                            {(order, orderMeta) => {
                                const selectedCustomerId = Number(order.customerId) || 0;
                                let selectedCustomer = this.viewModel.customers.find(c => c.customerId === selectedCustomerId);

                                return <>
                                    <div className="row g-3 align-items-center mb-3">
                                        {!selectedCustomer &&
                                            (
                                                <div className="login-screen">
                                                    <h1>Login</h1>
                                                    <div className="col-md-8">
                                                        <Neo.FormGroup bind={orderMeta.customerId} select={{ items: this.viewModel.customers, valueMember: "customerId", displayMember: "customerName" }} />
                                                    </div>
                                                </div>
                                            )
                                        }
                                       
                                        {selectedCustomer && (
                                            <>
                                            <button type="button" className="btn btn-link btn-sm text-decoration-none mb-2 text-start ps-0 col-md-1" onClick={() => order.customerId = 0}>Logout</button>
                                            <div className="col-md-12 d-flex flex-direction-column flex-md-row align-items-center justify-content-center">
                                                <div className="col-md-8">
                                                    <h1>Welcome back, <em>{selectedCustomer.customerName}</em></h1>
                                                </div>
                                               
                                                <div className="col-md-4 d-flex flex-column align-items-md-end align-items-start gap-2">
                                                    {/* <div className="WalletBalance text-md-end text-start fw-semibold">
                                                        {`Wallet balance: ${selectedCustomer.walletBalance}`}
                                                    </div> */}
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
                                    {selectedCustomer && (
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
                                                                       
                                </>;
                            }}
                        </Neo.Form>}
                        {!this.viewModel.newOrder && 
                            <Neo.Alert variant="success" heading="Order submitted" animateInitial className="mt-4">
                                Your order has been submitted, <Link to={viewOrdersRoute.path}>view your orders here</Link>, 
                                or <Neo.Button variant="link" className="btn-link-inline" onClick={() => this.viewModel.setupOrder()}>create another order</Neo.Button>.
                            </Neo.Alert>}
                </Neo.Card>
            </div>
        );
    }
}