import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import DashboardVM from './DashboardVM';
import { observer } from 'mobx-react';

class DashboardParams {
    // TODO: Add parameters here in the form: public paramName = { isQuery?: boolean, required?: boolean };
}

@observer
export default class DashboardView extends Views.ViewBase<DashboardVM, DashboardParams> {
   public static params = new DashboardParams();

    constructor(props: unknown) {
        super("Dashboard", DashboardVM, props);
    }

    protected viewParamsUpdated() {

    }

    public render() {
        return (
            <div>
			    <Neo.Card title="Dashboard">
        
                </Neo.Card>
            </div>
        );
    }
}