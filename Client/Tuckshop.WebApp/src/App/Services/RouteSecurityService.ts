import { RouteSecurityService as NeoRouteSecurityService } from "@singularsystems/neo-core/dist/Routing/RouteSecurityService";
import { injectable } from "inversify";
import { AppService, Types } from "./AppService";
import { IAppMenuItem, IAppRoute } from "./RouteService";

@injectable()
export class RouteSecurityService extends NeoRouteSecurityService {

    constructor(
        authorisationService = AppService.get(Types.Neo.Security.AuthorisationService),
        private authenticationService = AppService.get(Types.App.Services.AuthenticationService)) {
        super(authorisationService);
    }

    routeAllowed(route: IAppRoute) {
        return super.routeAllowed(route);
    }

    menuItemAllowed(menuItem: IAppMenuItem) {
        return super.menuItemAllowed(menuItem)
    }
}