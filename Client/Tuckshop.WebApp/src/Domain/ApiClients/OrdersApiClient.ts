import { Data } from '@singularsystems/neo-core';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import Order from '../Models/Orders/Order';

export interface IOrdersApiClient extends Data.IUpdateableApiClient<Order, number> {

    // Client only properties / methods
}

@injectable()
export default class OrdersApiClient extends Data.UpdateableApiClient<Order, number> implements IOrdersApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/Orders`);
    }

    // Client only properties / methods
}