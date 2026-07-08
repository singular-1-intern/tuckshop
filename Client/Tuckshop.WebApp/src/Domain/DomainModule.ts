import { AppServices } from '@singularsystems/neo-core';
import { DomainTypes } from './DomainTypes';
import { DomainDataCache } from './Services/DomainDataCache';
import { NeoServicesTypes } from "@singularsystems/neo-react-services"
import CatalogueEditService from './Services/CatalogueEditService';
import CatalogueApiClient from "./ApiClients/CatalogueApiClient";
import ProductsApiClient from './ApiClients/ProductsApiClient';
import OrdersCommandApiClient from './ApiClients/OrdersCommandApiClient';
import OrdersQueryApiClient from './ApiClients/OrdersQueryApiClient';
import CustomersApiClient from './ApiClients/CustomersApiClient';
import CustomersCommandApiClient from './ApiClients/CustomersCommandApiClient';

export const DomainAppModule = new AppServices.Module("Domain", container => {

    // Api Clients
    container.bind(DomainTypes.ApiClients.Catalogue).to(CatalogueApiClient).inSingletonScope();
    container.bind(DomainTypes.ApiClients.ProductsApiClient).to(ProductsApiClient).inSingletonScope();
    container.bind(DomainTypes.ApiClients.OrdersCommandApiClient).to(OrdersCommandApiClient).inSingletonScope();
    container.bind(DomainTypes.ApiClients.OrdersQueryApiClient).to(OrdersQueryApiClient).inSingletonScope();
    container.bind(DomainTypes.ApiClients.CustomersApiClient).to(CustomersApiClient).inSingletonScope();
    container.bind(DomainTypes.ApiClients.CustomersCommandApiClient).to(CustomersCommandApiClient).inSingletonScope();
    
    // Services
    container.bind(DomainTypes.Services.DataCache).to(DomainDataCache).inSingletonScope();
    container.bind(NeoServicesTypes.Catalogue.CatalogueEditService).to(CatalogueEditService).inSingletonScope();
});

export const DomainTestModule = new AppServices.Module("Domain", container => {
    // bind test types
});