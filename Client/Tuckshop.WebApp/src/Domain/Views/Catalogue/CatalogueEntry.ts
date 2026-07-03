import { ModelBase } from '@singularsystems/neo-core';
import { Catalogue } from '@singularsystems/neo-react-services';
import { ICatalogueApiClient } from '../../ApiClients/CatalogueApiClient';
import { DomainDataCache } from '../../Services/DomainDataCache';

/** Generic catalogue component base. */
export abstract class CatalogueEntry<TModel extends ModelBase> extends Catalogue.CatalogueEntryBase<TModel, DomainDataCache, ICatalogueApiClient> {

}