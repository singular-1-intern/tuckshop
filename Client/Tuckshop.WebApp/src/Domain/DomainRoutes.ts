import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
// import CatalogueView from "./Views/Catalogue/CatalogueView";
// import * as CatalogueRoles from "./Models/Security/CatalogueRoles";
import ProductsView from './Views/ProductsView';
import CreateOrderView from './Views/Orders/CreateOrderView';
import ViewOrdersView from './Views/Orders/ViewOrdersView';
import CustomersView from './Views/CustomersView';
import DashboardView from './Views/DashboardView';
import ViewMyOrdersView from './Views/Orders/ViewMyOrdersView';

export const viewOrdersRoute = { name: "View Orders", path: '/view-orders', component: ViewOrdersView, icon: "list_alt" };
export const viewMyOrdersRoute = { name: "View My Orders", path: '/view-my-orders', component: ViewMyOrdersView, icon: "list_alt" };

const MenuRoutes: IAppMenuItem[] = 
    [
        { 
            name: "Domain", children: 
            [
                { 
                    name: "Tuckshop", path: "/tuckshop", icon: "store", component: CreateOrderView
                },
                { 
                    name: "Dashboard", path: "/dashboard", icon: "dashboard", component: DashboardView
                },
                { 
                    name: "Products", path: "/products", icon: "add_shopping_cart", component: ProductsView 
                }, 
                    viewOrdersRoute,
                { 
                    name: "Customers", path: "/customers", icon: "people", component: CustomersView
                },
                    viewMyOrdersRoute,
                // { 
                //     name: "Catalogue", 
                //     path: "/catalogue", 
                //     component: CatalogueView,
                //     icon: "browse",
                //     role: CatalogueRoles.CataloguePage.View,
                //     routeChildren: CatalogueView.getRouteChildren()
                // }
            ]
        }
    ];

const PureRoutes: IAppRoute[] = [];

export { 
    MenuRoutes, 
    PureRoutes 
}