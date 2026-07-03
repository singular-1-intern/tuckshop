import { AppServices } from '@singularsystems/neo-core';
import { Types } from './IdentityTypes';
import UserManagementApiClient from './ApiClients/Custom/UserManagementApiClient';
import IdentityProvidersApiClient from './ApiClients/Custom/IdentityProvidersApiClient';

// Modules
export const IdentityModule = new AppServices.Module("Identity", container => {
    
    // ApiClients
    container.bind(Types.Identity.ApiClients.UserManagementApiClient).to(UserManagementApiClient).inSingletonScope();
    container.bind(Types.Identity.ApiClients.IdentityProvidersApiClient).to(IdentityProvidersApiClient).inSingletonScope();
});

export const IdentityTestModule = new AppServices.Module("Reporting", container => {
    // bind any test types here
});