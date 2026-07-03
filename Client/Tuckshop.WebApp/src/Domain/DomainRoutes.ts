import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import CatalogueView from "./Views/Catalogue/CatalogueView";
import * as CatalogueRoles from "./Models/Security/CatalogueRoles";
import ProductsView from './Views/ProductsView';
import CreateOrderView from './Views/Orders/CreateOrderView';
import ViewOrdersView from './Views/Orders/ViewOrdersView';
import CustomersView from './Views/CustomersView';

export const viewOrdersRoute = { name: "View Orders", path: '/view-orders', component: ViewOrdersView, icon: "list_alt" };

const MenuRoutes: IAppMenuItem[] = 
    [
        { 
            name: "Domain", children: 
            [
                { 
                    name: "Products", path: "/products", icon: "store", component: ProductsView 
                },
                { 
                    name: "Create Order", path: "/create-order", icon: "add_shopping_cart", component: CreateOrderView
                },
                // { 
                //     name: "View Orders", path: "/view-orders", icon: "list_alt", component: ViewOrdersView
                // }, 
                    viewOrdersRoute,
                { 
                    name: "Customers", path: "/customers", icon: "people", component: CustomersView
                },           
                { 
                    name: "Catalogue", 
                    path: "/catalogue", 
                    component: CatalogueView,
                    icon: "browse",
                    role: CatalogueRoles.CataloguePage.View,
                    routeChildren: CatalogueView.getRouteChildren()
                }
            ]
        }
    ];

const PureRoutes: IAppRoute[] = [];

export { 
    MenuRoutes, 
    PureRoutes 
}