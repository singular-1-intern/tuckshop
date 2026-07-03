import { Routing as NeoRouting } from '@singularsystems/neo-core';
import { Routing } from '@singularsystems/neo-react';
import { injectable } from 'inversify';
import { AppService, Types } from './AppService';
import Home from '../Views/Home';
import NotFound from '../Components/404NotFound';
import OidcLoginRedirect from '../Views/Security/OidcLoginRedirect';
import { AppConfig } from '../Models/AppConfig';
import { SecurityRoute } from '@singularsystems/neo-authorisation';
import { TemplateLayoutsView, TemplatesView, NotificationSettingsView, NotificationsView, NotificationsRoles } from '@singularsystems/neo-notifications';

import * as DomainRoutes from '../../Domain/DomainRoutes';
import * as IdentityRoutes from '../../Identity/IdentityRoutes';
import * as ReportingRoutes from '../../Reporting/ReportingRoutes';

interface ICommonAppRouteProps {
    /** TODO: Add any custom route props here, like userType. */
}

export interface IAppRoute extends NeoRouting.IRoute, ICommonAppRouteProps {

}

export interface IAppMenuItem extends NeoRouting.IMenuRoute, ICommonAppRouteProps {
    header?: boolean;
}

@injectable()
export class RouteService {

    private routeProvider: Routing.RouteProvider;

    constructor(private config = AppService.get(Types.App.Config)) {
        
        this.routeProvider = new Routing.RouteProvider(
            this.getMenuRoutes(),
            this.getPureRoutes(),
            NotFound,
        )
    }

    /**
     * Gets the routes provider
     */
    public get routes(): Routing.RouteProvider {
        return this.routeProvider;
    }

    private getMenuRoutes(): IAppMenuItem[] {
        return [
            {
                name: "Home", path: '/', component: Home, icon: "home", exact: true, allowAnonymous: true
            },
            ...ReportingRoutes.MenuRoutes,
			...DomainRoutes.MenuRoutes,
            { name: "Notifications", children: [
                { name: "Templates", path: "/templates", icon: "format_image_left", component: TemplatesView, role: NotificationsRoles.SetupTemplates },
                { name: "Template layouts", path: "/templateLayouts", icon: "layers", component: TemplateLayoutsView, role: NotificationsRoles.SetupLayouts },
                { name: "Notification settings", path: "/notification-settings", icon: "inbox_customize", component: NotificationSettingsView, role: NotificationsRoles.ConfigureSettings },
                { name: "View notifications", path: "/notifications", icon: "mail", component: NotificationsView, role: NotificationsRoles.ViewSentNotifications },
            ]},
            this.getAdministrationRoute(),
        ]
    }

    private getAdministrationRoute() {
        var adminRoute = { name: "Administration", children: [
            { ...SecurityRoute, icon: "admin_panel_settings" },
            ...IdentityRoutes.IdentityMenuRoutes,
        ]};

        if (this.config.isDevelopment) {
            adminRoute.children.push(IdentityRoutes.SecurityConfigRoute);
        }
        return adminRoute;
    }


    private getPureRoutes(): IAppRoute[] {
        const pureRoutes = [
            {
                path: AppConfig.loginRedirectRoute, component: OidcLoginRedirect, allowAnonymous: true
            },
			...DomainRoutes.PureRoutes,
            ...ReportingRoutes.PureRoutes,
        ];

        if (!this.config.isDevelopment) {
            pureRoutes.push(IdentityRoutes.SecurityConfigRoute);
        }

        return pureRoutes;
    }
}