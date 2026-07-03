import { Attributes, LookupBase, NeoModel } from '@singularsystems/neo-core';

@NeoModel
export default class UserManagementActionLogLookup extends LookupBase {

    public action: string = "";

    public actionedBy: string = "";

    @Attributes.Date()
    public actionedOn: Date = new Date();

    // Client only properties / methods
}