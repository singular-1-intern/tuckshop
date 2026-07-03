import * as React from 'react';
import { NotifyUtils } from '@singularsystems/neo-core';
import { Neo } from '@singularsystems/neo-react';
import { Catalogue } from '@singularsystems/neo-react-services';
import { ReportViewModal } from "@singularsystems/neo-reporting";
import { observer, Observer } from 'mobx-react';
import Sidebar from '../Components/Sidebar';
import HeaderPanel from '../Components/HeaderPanel';
import Footer from '../Components/Footer';
import RouteView from '../Components/RouteView';
import { AppService, Types } from '../Services/AppService';

import '../Styles/App.scss';

export default class Layout extends React.Component {

    private routing = AppService.get(Types.Neo.Routing.GlobalRoutingState);

    public render() {
        const showLayout = window.opener == null;

        return (
            <main>
                <ContainerPanel>

                    {/* Left menu panel */}
                    {showLayout &&
                        <Sidebar showPrefixes={true} singleLevelOnCollapse={true} />}

                    {/* Right content panel */}
                    <div className="app-right-panel" id="right-panel">

                        <Observer>
                            {() => (
                                <>
                                    <Neo.Loader task={this.routing.taskRunner} className="page-loader" showSpinner={false} />
                                    <div className={"app-content-area container-fluid" + (this.routing.currentViewFullScreen ? "" : " constrain-width")}>
                                        <div id="content-panel">
                                            {showLayout &&
                                                <HeaderPanel />}

                                            <React.Suspense fallback={<div>Loading...</div>}>
                                                <RouteView />
                                            </React.Suspense>
                                        </div>

                                        {showLayout && 
                                            <Footer />}
                                    </div>
                                </>
                            )}
                        </Observer>
                    </div>

                    <Neo.ModalContainer />
                    <Neo.ToastContainer notificationStore={NotifyUtils.store} />
                    <Neo.TooltipProvider />
                    <Neo.ContextMenuContainer />
                    <Catalogue.EditModal />
                    <ReportViewModal />
                </ContainerPanel>
            </main>
        );
    }
}

@observer
class ContainerPanel extends React.Component<{ children: React.ReactNode }> {

    private appLayout = AppService.get(Types.App.Services.AppLayout);

    render() {
        const showLayout = window.opener == null;

        const layout = this.appLayout;
        let containerClassName = "app-container";
        if (layout.thinSideBar) {
            containerClassName += " thin-sidebar";
        }
        if (layout.sideBarExpanded) {
            containerClassName += " sidebar-expanded";
        }
        if (layout.thinSideBar && !layout.sideBarExpanded) {
            containerClassName += " sidebar-collapsed";
        }
        if (layout.sideBarExpanded || !layout.thinSideBar) {
            containerClassName += " full-sidebar";
        }
        if (!showLayout) {
            containerClassName += " suppress-app-layout";
        }

        return (
            <div className={containerClassName}>{this.props.children}</div>
        )
    }
}