import { Security } from "@singularsystems/neo-core";

interface IAppUserClaims {
    /** Gets the user id. */
    sub: string;
}

export default class AppUser extends Security.OidcUser {

    /** Gets the users claims. */
    public get claims(): IAppUserClaims {
        return this.userData.profile;
    }
}