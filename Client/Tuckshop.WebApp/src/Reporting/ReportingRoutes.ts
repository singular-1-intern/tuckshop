import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import ReportingView from './ReportingView';

import * as Roles from './Models/Security/Roles';

const MenuRoutes: IAppMenuItem[] = 
    [
        { name: "Reporting", path: "/reporting", icon: "print", component: ReportingView, role: Roles.General.View }
    ];
    
const PureRoutes: IAppRoute[] = [];

export { 
    MenuRoutes,
    PureRoutes
} 