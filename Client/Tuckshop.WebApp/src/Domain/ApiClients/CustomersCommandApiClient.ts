import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';
import DepositFunds from '../Models/Customers/Commands/DepositFunds';
import Customer from '../Models/Customer';
import WithdrawFunds from '../Models/Customers/Commands/WithdrawFunds';

export interface ICustomersCommandApiClient {

    /** 
     * Deposits funds into a customer's wallet
     * @param command The deposit funds command
     * @returns The updated customer
     */
    depositFunds(command: Model.PartialPlainObject<DepositFunds>): AxiosPromise<Model.PlainTrackedObject<Customer>>;

    /** 
     * Withdraws funds from a customer's wallet
     * @param command The withdraw funds command
     * @returns The updated customer
     */
    withdrawFunds(command: Model.PartialPlainObject<WithdrawFunds>): AxiosPromise<Model.PlainTrackedObject<Customer>>;

    // Client only properties / methods
}

@injectable()
export default class CustomersCommandApiClient extends Data.ApiClientBase implements ICustomersCommandApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/customers/commands`);
    }

    public depositFunds(command: Model.PartialPlainObject<DepositFunds>): AxiosPromise<Model.PlainTrackedObject<Customer>> {
        return this.axios.post(`${this.apiPath}/wallet/deposit`, command);
    }

    public withdrawFunds(command: Model.PartialPlainObject<WithdrawFunds>): AxiosPromise<Model.PlainTrackedObject<Customer>> {
        return this.axios.post(`${this.apiPath}/wallet/withdraw`, command);
    }

    // Client only properties / methods
}