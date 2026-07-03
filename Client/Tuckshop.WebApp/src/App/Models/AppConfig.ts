import { AppServices, Security } from '@singularsystems/neo-core'
import { UserManagerSettings } from 'oidc-client-ts';
import { injectable } from 'inversify';
import { IAuthorisationConfig } from '@singularsystems/neo-authorisation';
import { IReportingConfig } from '@singularsystems/neo-reporting';
import { INotificationServiceConfig } from '@singularsystems/neo-notifications';
import { NotificationDuration } from './NotificationDuration';

@injectable()
export class AppConfig extends AppServices.ConfigModel {

    public static readonly silentSignInRoute = "/OidcSilentLoginRedirect";
    public static readonly loginRedirectRoute = "/OidcLoginRedirect";

    public userManagerSettings = {} as UserManagerSettings;

    // These settings are loaded from public\config.json
    public apiPath: string = "";
    public apiBaseUrl: string = "";
    private identityServer = { client_id: "", url: "" };
    private authorisationServer = { apiPath: "" };
    public environment: string = "";

    public get authorisationConfig(): IAuthorisationConfig {
        return {
            authorisationServerApiPath: this.authorisationServer.apiPath,
            useUserSearch: false
        }
    }


    public get identityConfig() {
        return {
            basePath: this.identityServer.url,
            identityApiPath: `${this.identityServer.url}/api`
        }
    }

    public get notificationServerConfig(): INotificationServiceConfig {
        return {
            basePath: this.apiBaseUrl,
            apiPath: this.apiPath,
            popupHideTime: NotificationDuration.Standard,
            appId: 1,
            allowBodyHtml: true,
        }
    }

    public get reportingConfig(): IReportingConfig {
        return {
            basePath: this.apiBaseUrl,
            apiPath: this.apiPath,
            notificationDuration: NotificationDuration.Standard,
            showPIIDownloadWarning: true,
        }
    }
    
    /**
     * Transforms property values loaded from config.json
     */
    public initialise() {
        AppConfig.setHostName(this, "apiPath");
        AppConfig.setHostName(this, "apiBaseUrl");
        AppConfig.setHostName(this.identityServer, "url");
        AppConfig.setHostName(this.authorisationServer, "apiPath");

        this.loadUserManagerSettings();
    }

    private loadUserManagerSettings() {

        const appUrl = `${window.location.origin}${this.baseUrl}`;
        
        this.userManagerSettings = {
            client_id: this.identityServer.client_id,
            redirect_uri: `${appUrl}${AppConfig.loginRedirectRoute}`,
            response_type: 'code',
            scope: "openid profile Authorisation IdentityServerApi Tuckshop.Domain",
            authority: this.identityServer.url,
            silent_redirect_uri: `${appUrl}${AppConfig.silentSignInRoute}`,
            monitorSession: false,
            // Note: remove this line if you want to get this metadata from the discovery document of the identity server.
			// This will result in an additional api call.
            metadata: Security.OidcAuthService.createIdentityServerMetadata(this.identityServer.url)
        }
    }

    private static setHostName<T>(config: T, key: keyof T) {
        const value = config[key];
        if (typeof value === "string") {
            (config[key] as unknown as string) = value.replace("{domain}", window.location.hostname);
        } else {
            throw `Config error: ${String(key)} is not a string property.`;
        }
    }
}