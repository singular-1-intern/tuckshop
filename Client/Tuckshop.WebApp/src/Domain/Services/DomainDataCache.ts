import { Data } from '@singularsystems/neo-core';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import Product from '../Models/Product';

export enum LifeTime {
    Short = 30,
    Long = 240
}

@injectable()
export class DomainDataCache extends Data.CachedDataService {

    constructor(
        private productsApiClient = AppService.get(Types.Domain.ApiClients.ProductsApiClient)
    ) {
        super();
    }

    // Register cached data here:
    public products = this.registerList(Product, this.productsApiClient.get, LifeTime.Short);
}