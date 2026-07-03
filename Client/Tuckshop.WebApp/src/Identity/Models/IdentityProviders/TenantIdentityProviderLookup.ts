import { LookupBase, NeoModel } from '@singularsystems/neo-core';
import { IdentityProviderType } from './IdentityProviderType';

@NeoModel
export default class IdentityProviderLookup extends LookupBase {

    public identityProviderId: number = 0;

    public nameSuffix: string = "";

    public name: string = "";

    public displayName: string = "";

    public identityProviderType: IdentityProviderType | null = null;

    public buttonImageUrl: string = "";

    // Client only properties / methods

    public get isExternalProvider(): boolean {
        return this.identityProviderType !== IdentityProviderType.LoginCredentials;
    }
}