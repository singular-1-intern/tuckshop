import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';

import SecurityConfigView from './Views/SecurityConfigView';
import UserManagementView from './Views/UserManagement/UserManagementView';

import * as Roles from './Models/Security/Roles';
import IdentityProvidersView from './Views/IdentityProviders/IdentityProvidersView';

const UserManagementRoute: IAppMenuItem =
    { name: "User Management", path: "/user-management", icon: "deployed_code_account", component: UserManagementView, role: Roles.UserManagement.Access }

const IdentityProvidersRoute: IAppMenuItem =
    { name: "Identity Providers", path: "/identity-providers", icon: "security_key", component: IdentityProvidersView, role: Roles.IdentityProviders.Setup }

const SecurityConfigRoute: IAppMenuItem =
    { name: "Security Config (Dev)", path: '/security/config', icon: "token", component: SecurityConfigView }

const IdentityMenuRoutes: IAppMenuItem[] =
    [
        UserManagementRoute,
        IdentityProvidersRoute,
    ];

const PureRoutes: IAppRoute[] =
    [
    ];

export {
    UserManagementRoute,
    IdentityProvidersRoute,
    SecurityConfigRoute,
    IdentityMenuRoutes,
    PureRoutes
} 