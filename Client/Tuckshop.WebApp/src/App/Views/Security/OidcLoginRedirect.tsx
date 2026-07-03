import * as React from "react";
import { AppService, Types } from '../../Services/AppService';
import { Neo } from "@singularsystems/neo-react";

class OidcLoginRedirect extends React.Component {

    public async componentDidMount() {

        const result = await AppService.get(Types.Neo.Security.AuthenticationService).tryCompleteSignIn();
        if (result.success) {
            const navigation = AppService.get(Types.Neo.Routing.NavigationHelper);
            
            if (result.redirectPath) {
                navigation.navigateInternal(result.redirectPath, true);
            } else {
                navigation.navigateInternal("/", true);
            }
        }
    }

    public render() {
        return OidcLoginRedirect.renderUnauthenticated();
    }

    public static renderUnauthenticated() {
        const authenticationService = AppService.get(Types.App.Services.AuthenticationService);

        return (
            <Neo.Card>
                {authenticationService.isSigningOut ? "Signing out..." : "Signing in..."} 
            </Neo.Card>
        )
    }
}

export default OidcLoginRedirect;