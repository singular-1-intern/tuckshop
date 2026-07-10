import * as React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import { observer } from 'mobx-react';
import { Chart, type ChartOptions } from '@highcharts/react';

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

@observer
export default class Home extends Views.ViewBase {

    constructor(props: unknown) {
        super("", Views.EmptyViewModel, props);
    }

    public render() {
        return (
            <div>
                <Neo.GridLayout lg={3}>

                    <Neo.Card title="Dashboard item 1">
                        
                    </Neo.Card>

                    <Neo.Card title="Dashboard item 2">
                    </Neo.Card>

                    <Neo.Card title='Dashboard'>
                        <PieChart options={{ showLegend: true, isDonut: true }} />
                    </Neo.Card>

                </Neo.GridLayout>
            </div>
        )
    }
}