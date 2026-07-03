import { Attributes, LookupBase, NeoModel } from '@singularsystems/neo-core';

@NeoModel
export default class UserInviteLookup extends LookupBase {

    public userInviteId: number = 0;

    public emailAddress: string = "";

    @Attributes.Nullable()
    public addToGroupId: number | null = null;

    @Attributes.Date()
    public createdOn: Date = new Date();

    public hasRegistered: boolean = false;

    // Client only properties / methods

    public get status() {
        if (this.hasRegistered) {
            return "Registered";
        } else {
            return "Pending";
        }
    }
}