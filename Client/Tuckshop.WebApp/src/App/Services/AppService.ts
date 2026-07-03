import { AppServices, Misc } from '@singularsystems/neo-core';
import Types from '../AppTypes';

const AppService : AppServices.IAppService = new AppServices.AppService();

Misc.Globals.appService = AppService;

export { AppService, Types };