import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import DashboardVM from './DashboardVM';
import { observer } from 'mobx-react';
import { Chart, type ChartOptions } from '@highcharts/react';
// import CodeUtil from './CodeUtil';
// import * as DemoCode from './Layout';

interface PieOptions {
  showLegend?: boolean;
  isDonut?: boolean;
}

const pieData = [
  { browser: 'Toyota', value: 13529 },
  { browser: 'VW', value: 6322 },
  { browser: 'Suzuki', value: 4287 },
  { browser: 'Nissan', value: 3167 },
  { browser: 'Hyundai', value: 2980 },
  { browser: 'Ford', value: 2341 }
];

const pieColors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd', '#8c564b'];

@observer
class PieChart extends React.Component<{ options: PieOptions }> {
  public getPieChartOptions(): ChartOptions {
    const options = this.props.options;
    const showLabels = !options.showLegend;

    return {
      chart: { type: 'pie', backgroundColor: '#fff' },
      title: { text: 'Car Sales' },
      plotOptions: {
        pie: {
          innerSize: options.isDonut ? '70%' : undefined,
          dataLabels: {
            enabled: showLabels,
            format: '<b>{point.name}</b>: {point.percentage:.1f} %',
            style: { fontSize: '10px' }
          }
        }
      },
      series: [{
        type: 'pie',
        name: 'Value',
        data: pieData.map(c => ({ name: c.browser, y: c.value })),
        colors: pieColors,
        showInLegend: !!options.showLegend
      }]
    };
  }

  public render() {
    return <Chart options={this.getPieChartOptions()} />;
  }
}

class DashboardParams {}

@observer
export default class DashboardView extends Views.ViewBase<DashboardVM, DashboardParams> {
  public static params = new DashboardParams();

  constructor(props: unknown) {
    super('Dashboard', DashboardVM, props);
  }

  protected viewParamsUpdated() {}

  public render() {
    return (
      <div>
        <Neo.Card title='Dashboard'>
          <PieChart options={{ showLegend: true, isDonut: true }} />
        </Neo.Card>
                        <Neo.Card title="Shadows" data-code-key="Layout"  className="mt-3 shadow">
                    <p>
                        Bootstrap allows you to layout content using the <code>row</code> and <code>col-</code> css classes. If you need elements to flow vertically
                        this is easy to write (two column divs nested inside a row div). Creating a layout that flows horizontally is a lot more difficult.
                    </p>
                    <p>
                        The <code>Neo.GridLayout</code> component makes this easy if you are happy that each cell is of equal width.
                        Each direct child of <code>Neo.GridLayout</code> will be wrapped in a div with the bootstrap column class.
                    </p>
                    <p>
                        You can specify how many columns to display for each screen size. Resize the browser window to see this work. If you don't specify any props,
                        it will default to 2 columns above medium screen size, and 1 column below.
                    </p>
                    <p>
                        <code>Neo.GridLayout</code>s can be nested. If you need to put multiple elements in a 'cell', wrap them in a nested grid layout, or just
                        use a <code>div</code>.
                    </p>
                    <Neo.GridLayout withGaps md={2} xl={3} xxl={4} >
                        <div className="box">One</div>
                        <div className="box">Two</div>
                        <div className="box">Three</div>
                        <div className="box">Four</div>
                        <Neo.GridLayout withGaps gutterSize={1} >
                            <div className="box">Five</div>
                            <div className="box">Six</div>
                        </Neo.GridLayout>
                    </Neo.GridLayout>
                </Neo.Card>
      </div>
    );
  }
}