import { ModelBase, NeoModel } from '@singularsystems/neo-core';
import { AppService, Types } from '../../IdentityTypes';
import { IdentityProviderType } from './IdentityProviderType';

@NeoModel
export default class IdentityProviderTypeLookup extends ModelBase {

    public identityProviderType: IdentityProviderType | null = null;

    public identityProviderTypeName: string = "";

    public namePrefix: string = "";

    public callbackPath: string = "";

    public signedOutCallbackPath: string = "";

    public isOidc: boolean = false;

    public defaultImageUrl: string = "";

    // Client only properties / methods

    public get fullDefaultImageUrl() {
        return `${AppService.get(Types.App.Config).identityConfig.basePath}/${this.defaultImageUrl}`;
    }
}