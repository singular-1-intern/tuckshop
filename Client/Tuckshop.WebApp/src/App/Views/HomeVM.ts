import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../Services/AppService';
import { DomainTypes } from '../../Domain/DomainTypes';
import { List } from '@singularsystems/neo-core';
import Product from '../../Domain/Models/Product';
import { type ChartOptions } from '@highcharts/react';

export default class HomeVM extends Views.ViewModelBase {

    private readonly pieColors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd', '#8c564b'];

    private readonly pieData = [
        { name: 'Toyota', value: 13529 },
        { name: 'VW', value: 6322 },
        { name: 'Suzuki', value: 4287 },
        { name: 'Nissan', value: 3167 },
        { name: 'Hyundai', value: 2980 },
        { name: 'Ford', value: 2341 }
    ];

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private productsApiClient = AppService.get(DomainTypes.ApiClients.ProductsApiClient)) {

        super(taskRunner);
        this.makeObservable();
    }

    public products = new List(Product);

    public async initialise() {
        const response = await this.taskRunner.waitFor(this.productsApiClient.get());
        this.products.set(response.data);
    }

    public get hasProducts() {
        return this.products.length > 0;
    }

    public get pieChartOptions(): ChartOptions {
        return {
            chart: { type: 'pie', backgroundColor: '#fff' },
            title: { text: 'Car Sales' },
            plotOptions: {
                pie: {
                    innerSize: '70%',
                    dataLabels: {
                        enabled: false,
                        format: '<b>{point.name}</b>: {point.percentage:.1f} %',
                        style: { fontSize: '10px' }
                    }
                }
            },
            series: [{
                type: 'pie',
                name: 'Value',
                data: this.pieData,
                colors: this.pieColors,
                showInLegend: true
            }]
        };
    }

    public get stockBarChartOptions(): ChartOptions {
        const categories: string[] = [];
        const stockData: number[] = [];

        // Neo List is model-aware, so we prepare chart arrays in the VM.
        for (const product of this.products) {
            categories.push(product.productName);
            stockData.push(product.stockCount);
        }

        return {
            chart: { type: 'bar', backgroundColor: '#fff' },
            title: { text: 'Stock Count by Product' },
            xAxis: {
                categories,
                title: { text: null },
                gridLineWidth: 1,
                lineWidth: 0
            },
            yAxis: {
                min: 0,
                title: {
                    text: 'Stock Count',
                    align: 'high'
                },
                labels: { overflow: 'justify' },
                gridLineWidth: 0
            },
            tooltip: {
                pointFormat: '<span>Stock</span>: <b>{point.y}</b>'
            },
            legend: { enabled: false },
            credits: { enabled: false },
            plotOptions: {
                bar: {
                    dataLabels: { enabled: true },
                    groupPadding: 0.1,
                    borderRadius: '50%'
                }
            },
            series: [
                {
                    type: 'bar',
                    name: 'Stock Count',
                    color: '#1f77b4',
                    data: stockData
                }
            ]
        };
    }

}