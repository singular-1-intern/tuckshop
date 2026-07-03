import { Data } from '@singularsystems/neo-core';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import Customer from '../Models/Customer';

export interface ICustomersApiClient extends Data.IUpdateableApiClient<Customer, number> {

    // Client only properties / methods
}

@injectable()
export default class CustomersApiClient extends Data.UpdateableApiClient<Customer, number> implements ICustomersApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/Customers`);
    }

    // Client only properties / methods
}