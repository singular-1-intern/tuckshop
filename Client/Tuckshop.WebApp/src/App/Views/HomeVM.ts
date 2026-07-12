import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../Services/AppService';
import { DomainTypes } from '../../Domain/DomainTypes';
import { List } from '@singularsystems/neo-core';
import Product from '../../Domain/Models/Product';
import OrderLookupCriteria from '../../Domain/Models/Orders/Queries/OrderLookupCriteria';
import OrderLookup from '../../Domain/Models/Orders/Queries/OrderLookup';
import { type ChartOptions } from '@highcharts/react';
import { OrderStatus } from '../../Domain/Models/Orders/Enums/OrderStatus';

export default class HomeVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        private productsApiClient = AppService.get(DomainTypes.ApiClients.ProductsApiClient),
        private ordersQueryApiClient = AppService.get(DomainTypes.ApiClients.OrdersQueryApiClient),
        private ordersCommandApiClient = AppService.get(DomainTypes.ApiClients.OrdersCommandApiClient),    ) {

        super(taskRunner);
        this.makeObservable();
    }

    public products = new List(Product);
    public criteria = new OrderLookupCriteria();
    public foundOrders = new List(OrderLookup);
    public todaysOrders = new List(OrderLookup);

    public async initialise() {
        const response = await this.taskRunner.waitFor(this.productsApiClient.get());
        this.products.set(response.data);

        await this.findPendingOrders();
        await this.findTodaysOrders();
    }

    public async findPendingOrders() {
        this.criteria.orderStatus = OrderStatus.Pending;

        const response = await this.taskRunner.waitFor(this.ordersQueryApiClient.getOrderLookupAsync(this.criteria.toQueryObject()));
        this.foundOrders.set(response.data);
    }

    public async findTodaysOrders() {
        const todayCriteria = new OrderLookupCriteria();
        const now = new Date();

        const startOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate());
        const endOfDay = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1);

        todayCriteria.startDate = startOfDay;
        todayCriteria.endDate = endOfDay;

        const response = await this.taskRunner.waitFor(this.ordersQueryApiClient.getOrderLookupAsync(todayCriteria.toQueryObject()));

        this.todaysOrders.set(response.data);
    }

    public getOrderStatusText(order: OrderLookup) {
    switch (order.orderStatus) {
        case OrderStatus.Pending: 
            return "Pending";
        case OrderStatus.Completed: return "Completed";
        case OrderStatus.Cancelled: return "Cancelled";
        default: return "Unknown";
    }
}

    public getOrderStatusSymbol(order: OrderLookup) {
        switch (order.orderStatus) {
            case OrderStatus.Completed:
                return "task_alt";
            case OrderStatus.Cancelled:
                return "cancel";
            case OrderStatus.Pending:
                return "hourglass";
            default:
                return "help";
        }
    }

    public getOrderStatusVariant(order: OrderLookup) {
        switch (order.orderStatus) {
            case OrderStatus.Completed:
                return "success";
            case OrderStatus.Cancelled:
                return "danger";
            case OrderStatus.Pending:
                return "warning";
            default:
                return "secondary";
        }
    }

    public completeOrder(order: OrderLookup) {
        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.completeOrder({ orderId: order.orderId });
            order.completedOn = new Date();
        })
    }

    public cancelOrder(order: OrderLookup, reason: string) {
        this.taskRunner.run(async () => {
            await this.ordersCommandApiClient.cancelOrder({ orderId: order.orderId, reason });
            order.cancelledOn = new Date();
            order.cancelledReason = reason;
        });
    }

    public get hasProducts() {
        return this.products.length > 0;
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
            title: { text: undefined },
            xAxis: {
                categories,
                title: { text: null, style: { color: '#000000',} },
                labels: { overflow: 'justify', style: { color: '#000000' } },
                gridLineWidth: 0,
                lineWidth: 0
            },
            yAxis: {
                min: 0,
                title: {
                    text: 'Stock Count',
                    align: 'high'
                },
                labels: { overflow: 'justify', style: { color: '#000000', fontWeight: '900' } },
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
                    borderRadius: '50%',
                    borderWidth: 0,
                    borderColor: 'transparent'
                }
            },
            series: [
                {
                    type: 'bar',
                    name: 'Stock Count',
                    color: '#adb5bd',
                    data: stockData
                }
            ]
        };
    }

}