import React from 'react';
import { ModalUtils } from '@singularsystems/neo-core';
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
			    <Neo.Card title="Create Order">
                    {this.viewModel.newOrder && 
                        <Neo.Form model={this.viewModel.newOrder} showSummaryModal onSubmit={() => this.viewModel.submitOrder()}>
                            {(order, orderMeta) => {
                                const selectedCustomer = this.viewModel.customers.find(c => c.customerId === order.customerId);

                                return <>
                                    {/* <Neo.FormGroupInline bind={orderMeta.customerName} /> */}
                                    <div className="row g-3 align-items-center mb-3">
                                        <div className="col-md-8">
                                            <Neo.FormGroup bind={orderMeta.customerId} select={{ items: this.viewModel.customers, valueMember: "customerId", displayMember: "customerName" }} />
                                        </div>
                                        <div className="col-md-4">
                                            <div className="WalletBalance text-md-end text-start fw-semibold">
                                                {selectedCustomer
                                                    ? `Wallet balance: ${selectedCustomer.walletBalance}`
                                                    : 'Wallet balance: _____'}
                                            </div>
                                            <Neo.Button
                                                className="ms-1"
                                                menuAlignment="right"
                                                menuItems={[
                                                    { text: "Deposit", icon: "money", onClick: () => ModalUtils.showMessage("Wallet Deposit", "You have successfully deposited funds.") },
                                                    { text: "Withdraw", icon: "money", onClick: () => ModalUtils.showMessage("Wallet Withdrawal", "You have successfully withdrawn funds.") }
                                                ]}>
                                                Manage Wallet
                                            </Neo.Button>
                                        </div>
                                    </div>
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