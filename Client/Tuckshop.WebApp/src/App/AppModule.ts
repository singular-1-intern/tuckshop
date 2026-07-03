import React from 'react';
import { AppServices, Misc, NumberUtils, Numeric } from '@singularsystems/neo-core';
import { Icons, Neo } from '@singularsystems/neo-react';
import Types from './AppTypes';
import Axios from 'axios';
import { AuthorisationTypes, Settings as AuthSettings } from '@singularsystems/neo-authorisation';
import { ReportingTypes } from '@singularsystems/neo-reporting';
import { NotificationServiceTypes } from '@singularsystems/neo-notifications';
import { AppConfig } from './Models/AppConfig';
import { AuthenticationService } from './Services/AuthenticationService';
import { RouteService } from './Services/RouteService';
import { RouteSecurityService } from './Services/RouteSecurityService';
import AppLayout from './Services/AppLayout';
import AsyncSelect from 'react-select/async'
import Select from 'react-select';
import Decimal from "decimal.js-light";

const AppModule = new AppServices.Module("App", container => {

    // Config
    container.bind(Types.App.Config).to(AppConfig).inSingletonScope();
    container.bindConfig(AuthorisationTypes.ConfigModel, (c: AppConfig) => c.authorisationConfig);
    container.bindConfig(NotificationServiceTypes.ConfigModel, (c: AppConfig) => c.notificationServerConfig);
    container.bindConfig(ReportingTypes.ConfigModel, (c: AppConfig) => c.reportingConfig);

    // Security
    container.bind(Types.Neo.Security.AuthenticationService).to(AuthenticationService).inSingletonScope();

    // Api clients
    container.bind(Types.Neo.Axios).toConstantValue(Axios);

    // Services
    container.bind(Types.App.Services.AppLayout).to(AppLayout).inSingletonScope();
    container.bind(Types.App.Services.RouteService).to(RouteService).inSingletonScope();
    container.bind(Types.Neo.Routing.RouteSecurityService).to(RouteSecurityService).inSingletonScope();

    // Components
    container.bind(Types.Neo.Core.DecimalService).toConstantValue(new Numeric.DecimalService(Decimal, Decimal.ROUND_HALF_UP, Decimal.ROUND_DOWN));
    container.bind(Types.Neo.Components.IconFactory).toConstantValue(new Icons.MaterialSymbolsFontFactory({ fontAwesomeMappings }));
    container.bind(Types.Neo.Components.AsyncSelect).toConstantValue(AsyncSelect);
    container.bind(Types.Neo.Components.ReactSelect).toConstantValue(Select);
    /*
    The Neo Slider and Color components  are omitted here to reduce bundle size. Include if your project will use these components.
    import Slider from 'rc-slider';
    import 'rc-slider/assets/index.css';
    container.bind(Types.Neo.Components.Slider).toConstantValue(Slider);
    container.bind(Types.Neo.Components.Range).toConstantValue(Range);
    
    import ColorPicker from "./Components/ColorPicker";
    container.bind(Types.Neo.Components.ColorPicker).toConstantValue(ColorPicker);
    */

    NumberUtils.allowAsDecimalSeparator = ",";

    Misc.Settings.bootstrap.version = 5.3;
    Misc.Settings.fixedAppHeaderElement = () => document.querySelector(".app-header-panel");
    Misc.Settings.grid.useStickyHeaders = true;
    Misc.Settings.grid.editButton.icon = "edit_square";
    Misc.Settings.grid.borderType = "standard";
    Misc.Settings.grid.buttonColumnButtons.inline = true;
    Misc.Settings.list.removeItemBehavior = Misc.RemoveItemBehavior.MarkDeleted;
    Misc.Settings.fileManager.browseButton = { variant: "primary", isOutline: true };
    Misc.Settings.form.validationModalCloseButton = false;
    Misc.Settings.icons.warningIcon = React.createElement(Neo.Icon, { name: "error", solid: true });
    Misc.Settings.icons.errorIcon = React.createElement(Neo.Icon, { name: "error", solid: true });
    Misc.Settings.icons.infoIcon = React.createElement(Neo.Icon, { name: "error", solid: true });
    Misc.Settings.alerts.iconPosition = "left";
    Misc.Settings.icons.shouldSuppress = context => {
        // Suppress icons in cards and tabs and non-inline buttons
        if (context.icon == Misc.Settings.icons.backIcon) return false;
        if (context.component === "button" && context.props.inline) return false;
        return true;
    }
    
    AuthSettings.roles.showInlineDescriptions = true;
});

const fontAwesomeMappings: [string, string][] = [
    ["file-pdf", "download"]
];

const AppTestModule = new AppServices.Module("App", container => {

});

export { AppModule, AppTestModule };