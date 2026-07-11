import * as React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import { observer } from 'mobx-react';
import { Chart } from '@highcharts/react';
import HomeVM from './HomeVM';

@observer
export default class Home extends Views.ViewBase<HomeVM> {

    constructor(props: unknown) {
        super("", HomeVM, props);
    }

    public render() {
        return (
            <div>
                <Neo.GridLayout lg={3}>

                    <Neo.Card title="Dashboard item 1">
                        
                    </Neo.Card>

                    <Neo.Card title="Dashboard item 2">
                      {this.viewModel.hasProducts ? (
                        <Chart options={this.viewModel.stockBarChartOptions} />
                      ) : (
                        <div className="text-muted">No products available.</div>
                      )}
                    </Neo.Card>

                    <Neo.Card title='Dashboard'>
                        <Chart options={this.viewModel.pieChartOptions} />
                    </Neo.Card>

                </Neo.GridLayout>
            </div>
        )
    }
}