import { Data, Model } from "@singularsystems/neo-core";
import { AxiosPromise } from "axios";
import { injectable } from "inversify";
import { AppService, Types } from "../DomainTypes";

export interface ICatalogueApiClient {
  // Client only properties / methods
}

@injectable()
export default class CatalogueApiClient
  extends Data.ApiClientBase
  implements ICatalogueApiClient
{
  constructor(config = AppService.get(Types.App.Config)) {
    super(`${config.apiPath}/catalogue`);
  }

  // Client only properties / methods
}
