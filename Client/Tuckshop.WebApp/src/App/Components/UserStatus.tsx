import React from "react";
import { AppService, Types } from "../Services/AppService";
import { Neo } from "@singularsystems/neo-react";

export default class UserStatus extends React.Component {

    private authService = AppService.get(Types.App.Services.AuthenticationService);

    constructor(props: any) {
        super(props);

        this.logoutClicked = this.logoutClicked.bind(this);
    }

    private logoutClicked(e: React.MouseEvent) {
        e.preventDefault();

        this.authService.beginSignOut();
    }

    public render() {
        const user = this.authService.user;

        return (
            user && <div className="app-user-icon">
                <Neo.Icon name="user" solid />

                <div className="app-user-card">
                    <div className="card-arrow" />
                    <div className="card ">
                        <div className="card-body">
                            <h5 className="mb-0">{user.displayName}</h5>
                            <p className="card-text">{user.userName}</p>
                        </div>
                        <ul className="list-group list-group-flush">
                            {/* Uncomment if you have a profile page. */}
                            {/* <li className="list-group-item"><a href="#">Profile</a></li> */}
                            <li className="list-group-item"><a href="/" onClick={this.logoutClicked}>Logout</a></li>
                        </ul>
                    </div>
                </div>
            </div>
        )
    }
}