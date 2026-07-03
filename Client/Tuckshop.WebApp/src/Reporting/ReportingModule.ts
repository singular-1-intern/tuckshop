import { AppServices } from '@singularsystems/neo-core';
import { ReportingTypes } from '@singularsystems/neo-reporting';
import ReportingService from './Services/ReportingService';

// Modules
export const AppReportingModule = new AppServices.Module("Reporting", container => {
    
    // Services
    container.bind(ReportingTypes.Services.ReportingService).to(ReportingService).inSingletonScope();
});

export const ReportingTestModule = new AppServices.Module("Reporting", container => {
    // bind any test types here
});