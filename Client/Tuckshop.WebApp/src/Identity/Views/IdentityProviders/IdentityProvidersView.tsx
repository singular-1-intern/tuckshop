import React from 'react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import IdentityProvidersVM from './IdentityProvidersVM';
import { observer } from 'mobx-react';
import { EnumHelper, NotifyUtils } from '@singularsystems/neo-core';
import IdentityProvider from '../../Models/IdentityProviders/IdentityProvider';
import OidcProviderConfig from '../../Models/IdentityProviders/OidcProviderConfig';
import { IdentityProviderType } from '../../Models/IdentityProviders/IdentityProviderType';
import { IPropertyInstance } from '@singularsystems/neo-core/dist/Model';
import { NotificationDuration } from '../../../App/Models/NotificationDuration';

import '../../Styles/Identity.scss';

@observer
export default class IdentityProvidersView extends Views.ViewBase<IdentityProvidersVM> {

    constructor(props: unknown) {
        super("Identity Providers", IdentityProvidersVM, props);
    }

    public render() {
        return (
            <div className="pt-4">
                <Neo.GridLayout md={1}>
                    <div className="clients-buttons">
                        <Neo.Button className={"button-100 " + (this.viewModel.editingProvider?.isDirty ? "primary-color-pulse" : "")}
                            variant="primary" icon="save" disabled={!this.viewModel.editingProvider || !this.viewModel.editingProvider.isDirty}
                            isSubmit>Apply Changes</Neo.Button>
                    </div>
                </Neo.GridLayout>

                <Neo.Card icon="fa-user-shield" title={this.viewName}>
                    {/* Tenant Identity Providers List */}
                    <NeoGrid.Grid items={this.viewModel.identityProviderLookups}
                        addButton={{
                            text: "Add", variant: "primary", icon: "plus", isSubmit: false,
                            onClick: () => { this.viewModel.addProvider(); }
                        }}>
                        {(item, meta) => (
                            <NeoGrid.Row>
                                <NeoGrid.Column display={item.meta.displayName} label="Identity Provider" />
                                <NeoGrid.Column display={item.meta.name} />
                                <NeoGrid.Column display={item.meta.identityProviderType}
                                    select={{ items: EnumHelper.asList(IdentityProviderType), renderAsText: true }}
                                    sort={false} />
                                <NeoGrid.ButtonColumn
                                    editButton={{ onClick: () => this.viewModel.editProvider(item) }}
                                    deleteButton={{ onClick: () => this.viewModel.deleteProvider(item) }} />
                            </NeoGrid.Row>
                        )}
                    </NeoGrid.Grid>

                    {/* Edit Identity Provider Modal */}
                    <Neo.Modal size="xl"
                        title="Identity Provider"
                        bindModel={this.viewModel.meta.editingProvider}
                        formProps={{ showSummaryModal: true }}
                        acceptButton={{
                            onClick: () => { this.viewModel.updateProvider(); }
                        }}
                        onClose={() => { this.viewModel.cancelEdit(); }}>
                        {(model: IdentityProvider) => (
                            <>
                                <Neo.GridLayout lg={2}>

                                    <Neo.FormGroupInline label="Type"
                                        bind={model.meta.identityProviderType}
                                        select={{ items: this.viewModel.identityProviderTypes }}
                                        onChange={() => this.viewModel.switchProviderType()} />

                                    <Neo.FormGroupInline
                                        bind={model.meta.editNameSuffix}
                                        onChange={() => this.viewModel.generateProviderName()} />

                                    <Neo.FormGroupInline
                                        display={model.meta.name} />

                                    <Neo.FormGroupInline bind={model.meta.displayName} />

                                    <Neo.FormGroupInline
                                        label="Button Image Override Url (not required)"
                                        bind={model.meta.buttonImageUrl} />

                                    {model.identityProviderType !== IdentityProviderType.LoginCredentials &&
                                        <img src={this.viewModel.getButtonImageUrl(model)} className="identity-provider-image" alt="Button Image" />}
                                </Neo.GridLayout>

                                {model.oidcConfig &&
                                    model.identityProviderType !== IdentityProviderType.LoginCredentials &&
                                    this.renderOidcConfig(model, model.oidcConfig!)}

                            </>)}
                    </Neo.Modal>

                </Neo.Card>
            </div>
        );
    }

    private renderOidcConfig(identityProvider: IdentityProvider, oidcConfig: OidcProviderConfig) {
        return (
            <div>
                <Neo.Card icon="fa-user-check" title="OIDC Configuration" 
                        headerElements={
                        <div>
                            <Neo.Button variant="primary" icon="cog"
                                    onClick={() => this.viewModel.testProvider(this.viewModel.editingProvider!) }
                                    disabled={!(oidcConfig.authority && oidcConfig.clientId && oidcConfig.clientSecret)}
                                    tooltip={!(oidcConfig.authority && oidcConfig.clientId && oidcConfig.clientSecret) ? "Please make sure the 'Authority URL', 'Client ID' and 'Client Secret' are captured." : ""} >
                                    Verify
                            </Neo.Button>
                        </div>}>
                    <Neo.GridLayout lg={2}>
                        <div>
                            <Neo.FormGroupInline bind={oidcConfig.meta.authority} />
                            <Neo.FormGroupInline label="Client ID" bind={oidcConfig.meta.clientId} />
                            <Neo.FormGroupInline bind={oidcConfig.meta.clientSecret} input={{ type: "password" }} />
                        </div>
                        <div>
                            <Neo.FormGroupInline bind={oidcConfig.meta.nameClaimType} />
                            <Neo.FormGroupInline bind={oidcConfig.meta.roleClaimType} />
                            <Neo.FormGroupInline bind={oidcConfig.meta.scopes} />
                        </div>
                    </Neo.GridLayout>
                </Neo.Card>

                <Neo.Card icon="fa-link" title="SSO Urls">
                    <Neo.GridLayout lg={1}>
                        <Neo.FormGroupInline display={identityProvider.meta.RedirectUrl} append={this.getCopyTextButton(identityProvider.meta.RedirectUrl)} />
                        <Neo.FormGroupInline display={identityProvider.meta.LoggedOutUrl} append={this.getCopyTextButton(identityProvider.meta.LoggedOutUrl)} />
                        <Neo.FormGroupInline display={identityProvider.meta.appSsoUrl} append={this.getCopyTextButton(identityProvider.meta.appSsoUrl)} />
                    </Neo.GridLayout>
                </Neo.Card>
            </div>
        );
    }

    private getCopyTextButton(textProperty: IPropertyInstance<string>) {
        return <Neo.Button size="sm" icon="copy" tooltip="Copy to clipboard"
            onClick={() => {
                navigator.clipboard.writeText(textProperty.value);
                NotifyUtils.addSuccess("Copy", "Coped to clipboard.", NotificationDuration.Standard);
            }} />
    }
}