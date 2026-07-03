import React from 'react';
import { Routing } from '@singularsystems/neo-react';
import { AppService, Types } from '../Services/AppService';
import OidcLoginRedirect from "../Views/Security/OidcLoginRedirect";

class RouteView extends Routing.RouteView {

    constructor(props: any) {
        super(props, AppService.get(Types.App.Services.RouteService).routes);

        this.getForbiddenComponent = (route) => <h2>Forbidden</h2>;
        this.getSigningInComponent = (route) => OidcLoginRedirect.renderUnauthenticated();
    }
}

export default RouteView;