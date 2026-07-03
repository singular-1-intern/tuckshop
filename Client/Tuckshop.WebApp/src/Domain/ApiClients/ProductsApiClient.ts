import { Data } from '@singularsystems/neo-core';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import Product from '../Models/Product';

export interface IProductsApiClient extends Data.IUpdateableApiClient<Product, number> {

    // Client only properties / methods
}

@injectable()
export default class ProductsApiClient extends Data.UpdateableApiClient<Product, number> implements IProductsApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/Products`);
    }

    // Client only properties / methods
}