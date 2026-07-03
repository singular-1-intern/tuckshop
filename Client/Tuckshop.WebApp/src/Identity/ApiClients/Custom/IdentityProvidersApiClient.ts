import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../../IdentityTypes';
import IdentityProvider from '../../Models/IdentityProviders/IdentityProvider';
import IdentityProviderTypeLookup from '../../Models/IdentityProviders/IdentityProviderTypeLookup';
import IdentityProviderLookup from '../../Models/IdentityProviders/TenantIdentityProviderLookup';

export interface IIdentityProvidersApiClient {

    /**
     * Gets the Identity Providers
     * @returns A list of TenantIdentityProviderLookup
     */
    getLookups(): AxiosPromise<Array<Model.PlainTrackedObject<IdentityProviderLookup>>>;

    /**
     * Gets the Identity Provider for the provided id.
     * @returns A list of TenantIdentityProviderLookup
     */
     getIdentityProvider(identityProviderId: number): AxiosPromise<Model.PlainTrackedObject<IdentityProvider>>;

    /**
     * Updates the Identity Provider.
     * @param identityProvider id
     */
    updateIdentityProvider(identityProvider: Model.PartialPlainObject<IdentityProvider>): AxiosPromise<number>;

    /**
     * 
     * @param identityProviderId 
     */
    deleteIdentityProvider(identityProviderId: number): AxiosPromise;

    /** 
     * Gets the Identity Provider types.
     * @returns A list of IdentityProvider
     */
    getIdentityProviderTypes(): AxiosPromise<Array<Model.PlainObject<IdentityProviderTypeLookup>>>;

    /**
     * Tests the provider settings
     * @returns A string with the error. Empty string indicates success
     */
    testProvider(provider: Model.PartialPlainObject<IdentityProvider>): AxiosPromise<string>;

    // Client only properties / methods
}

@injectable()
export default class IdentityProvidersApiClient extends Data.ApiClientBase implements IIdentityProvidersApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.identityConfig.identityApiPath}/identity-providers`);
    }

    public getLookups(): AxiosPromise<Array<Model.PlainTrackedObject<IdentityProviderLookup>>> {
        return this.axios.get(`${this.apiPath}`);
    }

    public getIdentityProvider(identityProviderId: number): AxiosPromise<Model.PlainTrackedObject<IdentityProvider>> {
        return this.axios.get(`${this.apiPath}/${identityProviderId}`);
    }

    public updateIdentityProvider(identityProvider: Model.PartialPlainObject<IdentityProvider>): AxiosPromise<number> {
        return this.axios.post(`${this.apiPath}`, identityProvider);
    }

    public deleteIdentityProvider(identityProviderId: number): AxiosPromise {
        return this.axios.delete(`${this.apiPath}/${identityProviderId}`);
    }

    public getIdentityProviderTypes(): AxiosPromise<Array<Model.PlainObject<IdentityProviderTypeLookup>>> {
        return this.axios.get(`${this.apiPath}/types`);
    }

    public testProvider(provider: Model.PartialPlainObject<IdentityProvider>): AxiosPromise<any> {
        return this.axios.post(`${this.apiPath}/test-provider`, provider);
    }

    // Client only properties / methods
}