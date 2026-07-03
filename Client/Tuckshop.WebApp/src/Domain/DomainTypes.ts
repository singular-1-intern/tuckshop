import { AppServices } from '@singularsystems/neo-core';
import { AppService, Types as AppTypes } from '../App/Services/AppService';
import { DomainExportedTypes } from './DomainExportedTypes';
import { DomainDataCache } from './Services/DomainDataCache';
import { ICatalogueApiClient } from "./ApiClients/CatalogueApiClient";
import { IProductsApiClient } from './ApiClients/ProductsApiClient';
import { IOrdersCommandApiClient } from './ApiClients/OrdersCommandApiClient';
import { IOrdersQueryApiClient } from './ApiClients/OrdersQueryApiClient';
import { ICustomersApiClient } from './ApiClients/CustomersApiClient';

// Symbols specific to this module.
const DomainTypes = {
    ApiClients: {
        Catalogue: new AppServices.ServiceIdentifier<ICatalogueApiClient>("Domain.ApiClients.Catalogue"),
        ProductsApiClient: new AppServices.ServiceIdentifier<IProductsApiClient>("Domain.ApiClients.ProductsApiClient"),
        OrdersCommandApiClient: new AppServices.ServiceIdentifier<IOrdersCommandApiClient>("Domain.ApiClients.OrdersCommandApiClient"),
        OrdersQueryApiClient: new AppServices.ServiceIdentifier<IOrdersQueryApiClient>("Domain.ApiClients.OrdersQueryApiClient"),
        CustomersApiClient: new AppServices.ServiceIdentifier<ICustomersApiClient>("Domain.ApiClients.CustomersApiClient")
    },
    Services: {
        ...DomainExportedTypes.Services,
        DataCache: new AppServices.ServiceIdentifier<DomainDataCache>("Domain.Services.DataCache"),
    }
}

// Merged symbols from app for convenience.
const Types = {
    ...AppTypes,
    Domain: DomainTypes
}

export { AppService, Types, DomainTypes }