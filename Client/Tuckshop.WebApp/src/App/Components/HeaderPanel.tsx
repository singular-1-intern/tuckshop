import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import BreadCrumb from './BreadCrumb';
import UserStatus from './UserStatus';
import { observer } from 'mobx-react';
import { AppService, Types } from '../Services/AppService';
import { ScreenSize } from '../Services/AppLayout';

@observer
export default class HeaderPanel extends React.Component {

    private appLayout = AppService.get(Types.App.Services.AppLayout);

    componentDidMount() {
        this.appLayout.setup();
    }

    public render() {

        const layout = this.appLayout;
        const globalTask = Views.ViewBase.currentView ? Views.ViewBase.currentView.taskRunner : undefined;

        let headerClassName = "app-header-panel";

        return (
            <>
                {globalTask && globalTask.isBusy &&
                    <Neo.ProgressBar className="page-progress" progressState={globalTask.progressState} variant={globalTask.options.variant} />}
                    
                <div className={headerClassName} id="header-panel">
                    <div className="app-header">
                        {layout.currentScreenSize <= ScreenSize.Small &&
                            <div id="menu-anchor" className="app-hamburger-container" onClick={layout.menuToggle}>
                                <div className="app-hamburger">
                                    <Neo.Icon name='menu' />
                                </div>
                            </div>
                        }
                        <div className="app-breadcrumb">
                            <BreadCrumb rootItem={{ label: "Tuckshop", link: "/" }} /> {" "}
                        </div>

                        <UserStatus />
                    </div>
                </div>
            </>
        )
    }
}