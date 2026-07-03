import { AppServices } from '@singularsystems/neo-core';
import { NeoReactTypes } from '@singularsystems/neo-react';
import { AppConfig } from './Models/AppConfig';
import { RouteService } from './Services/RouteService';
import { IAppLayout } from './Services/AppLayout';
import { AuthenticationService } from './Services/AuthenticationService';
import { NotificationServiceTypes } from '@singularsystems/neo-notifications';
import { ReportingTypes } from '@singularsystems/neo-reporting';
import { DomainExportedTypes } from '../Domain/DomainExportedTypes';

const Types = {
    App: {
        Services: {
            AuthenticationService: AppServices.NeoTypes.Security.AuthenticationService.asType<AuthenticationService>(),
            AppLayout: new AppServices.ServiceIdentifier<IAppLayout>("Services.AppLayout"),
            RouteService: new AppServices.ServiceIdentifier<RouteService>("Services.RouteService"),
        },
        Config: AppServices.NeoTypes.Config.ConfigModel.asType<AppConfig>(),
    },
    Neo: NeoReactTypes,
    Notifications: NotificationServiceTypes,
    Reporting: ReportingTypes,
	Domain: DomainExportedTypes,
};

export default Types;