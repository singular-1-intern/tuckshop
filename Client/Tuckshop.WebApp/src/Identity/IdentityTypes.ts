import { AppServices } from '@singularsystems/neo-core';
import { AppService, Types as AppTypes } from '../App/Services/AppService';
import { IIdentityProvidersApiClient } from './ApiClients/Custom/IdentityProvidersApiClient';
import { IUserManagementApiClient } from './ApiClients/Custom/UserManagementApiClient';

// Symbols specific to this module.
const IdentityTypes = {
    ApiClients: {
        UserManagementApiClient: new AppServices.ServiceIdentifier<IUserManagementApiClient>("Identity.ApiClients.UserManagement"),
        IdentityProvidersApiClient: new AppServices.ServiceIdentifier<IIdentityProvidersApiClient>("Identity.ApiClients.IdentityProviders"),
    },
}

// Merged symbols from app for convenience.
const Types = {
    ...AppTypes,
    Identity: IdentityTypes
}

export { AppService, Types, IdentityTypes }