import { injectable } from "inversify";
import { AppService, Types } from "../DomainTypes";
import { Catalogue } from "@singularsystems/neo-react-services"
import { DomainDataCache } from "./DomainDataCache";
import { ICatalogueApiClient } from "../ApiClients/CatalogueApiClient";

@injectable()
export default class CatalogueEditService extends Catalogue.CatalogueEditService<DomainDataCache, ICatalogueApiClient> {

    constructor() {
        super(
            AppService.get(Types.Domain.Services.DataCache),
            AppService.get(Types.Domain.ApiClients.Catalogue)
        )

        this.makeObservable();
    }

    protected defaultEditRole() { 
        return null;
    }
}