import { Data } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../DomainTypes';

export interface ITestApiClient {

    /** 
     * Gets the app name.
     * @returns The app name.
     */
    getAppName(): AxiosPromise<string>;

    // Client only properties / methods
}

@injectable()
export default class TestApiClient extends Data.ApiClientBase implements ITestApiClient {

    constructor (config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/test`);
    }

    public getAppName(): AxiosPromise<string> {
        return this.axios.get(`${this.apiPath}/app-name`);
    }

    // Client only properties / methods
}