import { List, ModalUtils, NeoModel, NotifyUtils } from '@singularsystems/neo-core';
import { Views } from '@singularsystems/neo-react';
import { NotificationDuration } from '../../../App/Models/NotificationDuration';
import { AppService, Types } from '../../IdentityTypes';
import IdentityProvider from '../../Models/IdentityProviders/IdentityProvider';
import IdentityProviderTypeLookup from '../../Models/IdentityProviders/IdentityProviderTypeLookup';
import OidcProviderConfig from '../../Models/IdentityProviders/OidcProviderConfig';
import IdentityProviderLookup from '../../Models/IdentityProviders/TenantIdentityProviderLookup';

@NeoModel
export default class IdentityProvidersVM extends Views.ViewModelBase {
    
    public identityProviderLookups = new List(IdentityProviderLookup);
    public editingProvider: IdentityProvider | null = null;

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private identityProvidersApiClient = AppService.get(Types.Identity.ApiClients.IdentityProvidersApiClient)) {

        super(taskRunner);
    }

    public identityProviderTypes = new List(IdentityProviderTypeLookup);

    public async initialise() {
        await this.loadIdentityProviders();
    }

    private async loadIdentityProviders() {
        const result = await this.taskRunner.waitFor(this.identityProvidersApiClient.getLookups());
        this.identityProviderLookups.set(result.data);

        const typesResult = await this.taskRunner.waitFor(this.identityProvidersApiClient.getIdentityProviderTypes());
        this.identityProviderTypes.set(typesResult.data);
    }

    public addProvider() {
        this.editingProvider = new IdentityProvider();
    }

    public async editProvider(provider: IdentityProviderLookup) {
        const fullProvider = await this.taskRunner.waitForData(this.identityProvidersApiClient.getIdentityProvider(provider.identityProviderId));
        this.editingProvider = new IdentityProvider()
        this.editingProvider.set(fullProvider as any);

        this.switchProviderType();
    }

    public cancelEdit() {
        this.editingProvider = null;
    }

    public async updateProvider() {
        this.taskRunner.run(async () => {
            if (this.editingProvider) {
                const isNew = this.editingProvider.isNew;
                const result = await this.identityProvidersApiClient.updateIdentityProvider(this.editingProvider.toJSObject());
                
                this.editingProvider.identityProviderId = result.data;
    
                if (isNew) {
                    const newLookup = new IdentityProviderLookup();
                    newLookup.mapFrom(this.editingProvider.toJSObject());
                    this.identityProviderLookups.push(newLookup);
                } else {
                    const updatedLookup = this.identityProviderLookups.find(c => c.identityProviderId === this.editingProvider?.identityProviderId);
                    updatedLookup?.mapFrom(this.editingProvider.toJSObject());
                }

                NotifyUtils.addSuccess("Saved Successfully", "Identity Provider saved successfully", NotificationDuration.Standard);
                this.editingProvider = null;
            }
        });
    }

    public deleteProvider(provider: IdentityProviderLookup) {
        ModalUtils.showYesNo(
            "Are you sure?",
            "Note that deleting this identity provider will disable logins for all users linked to it. Are you VERY sure you want to delete it?",
            () => { 
                this.identityProvidersApiClient.deleteIdentityProvider(provider.identityProviderId); 
                const updatedLookup = this.identityProviderLookups.find(c => c.identityProviderId === provider.identityProviderId)!;
                this.identityProviderLookups.removeWithoutTracking(updatedLookup);
            }
        );
    }

    public switchProviderType() {
        if (this.editingProvider) {
            const providerLookup = this.getIdentityProviderType();

            this.editingProvider.setProviderType(providerLookup!);

            const isOidc = providerLookup?.isOidc ?? false;

            if (isOidc) {
                if (!this.editingProvider.oidcConfig) {
                    this.editingProvider.oidcConfig = new OidcProviderConfig();
                }
            } else {
                // Need to clear this on non OIDC providers because it would cause validation errors when not filled in
                this.editingProvider.oidcConfig = null;
            }
        }

        this.generateProviderName();
    }

    private getIdentityProviderType(identityProvider: IdentityProvider | null = null) {
        if (!identityProvider) {
            identityProvider = this.editingProvider;
        }
        return this.identityProviderTypes.find(c => c.identityProviderType === identityProvider?.identityProviderType);
    }

    public generateProviderName() {
        if (this.editingProvider && this.editingProvider.isNew && this.editingProvider) {
            if (this.editingProvider.identityProviderType) {
                const namePrefix = this.getIdentityProviderType()?.namePrefix;
                const nameSuffix = this.editingProvider.editNameSuffix;
                this.editingProvider.name = `${namePrefix}-${nameSuffix}`;
            } else {
                this.editingProvider.name = "";
            }
        }
    }

    public getButtonImageUrl(identityProvider: IdentityProvider) {
        if (identityProvider.buttonImageUrl && identityProvider.buttonImageUrl.length > 0) {
            return identityProvider.buttonImageUrl;
        } else {
            return this.getIdentityProviderType(identityProvider)?.fullDefaultImageUrl;
        }
    }

    public testProvider(provider: IdentityProvider) {        
        this.taskRunner.run(async () => {
            const result = await this.identityProvidersApiClient.testProvider(provider.toJSObject());
            if (!result.data) {
                NotifyUtils.addSuccess("Test Successful", "The identity provider was successfully tested", NotificationDuration.Long);
            } else {
                NotifyUtils.addDanger("Test Failed", `The identity provider test failed: ${result.data}`, NotificationDuration.Long);
            }
        });        
    }
}