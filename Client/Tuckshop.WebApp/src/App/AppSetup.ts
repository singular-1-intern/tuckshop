import { AppService } from './Services/AppService';
import { AppModule } from './AppModule';
import { AppServices } from '@singularsystems/neo-core';
import { NeoReactModule } from '@singularsystems/neo-react';
import { IdentityModule } from '../Identity/IdentityModule';
import { ReportingModule } from '@singularsystems/neo-reporting';
import { AppReportingModule } from '../Reporting/ReportingModule';
import { NotificationServiceModule } from '@singularsystems/neo-notifications';
import { AuthorisationAppModule } from '@singularsystems/neo-authorisation';
import { DomainAppModule } from '../Domain/DomainModule';

const appService = AppService as AppServices.AppService;

appService.registerModule(AppServices.NeoModule);
appService.registerModule(NeoReactModule);
appService.registerModule(AuthorisationAppModule);
appService.registerModule(IdentityModule)
appService.registerModule(NotificationServiceModule);
appService.registerModule(ReportingModule);
appService.registerModule(AppReportingModule);
appService.registerModule(AppModule);
appService.registerModule(DomainAppModule);