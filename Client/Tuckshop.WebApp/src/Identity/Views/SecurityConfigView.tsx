import React from 'react';
import { observer } from 'mobx-react';
import { Neo, Views } from '@singularsystems/neo-react';

import { AppService, Types } from '../IdentityTypes';

const codeFormat: React.CSSProperties = {
    fontFamily: "monospace",
    fontSize: "9pt",
    whiteSpace: "pre"
}

@observer
export default class SecurityConfigView extends Views.ViewBase {
    
    private authenticationService = AppService.get(Types.Neo.Security.AuthenticationService);
    private authorisationService = AppService.get(Types.Neo.Security.AuthorisationService);
    
    constructor(props: unknown) {
        super("Security Configuration", Views.EmptyViewModel, props);
    }

    render() {

        const user = this.authenticationService.user;

        return (
            <div className="pt-4">
                {user &&
                <div>
                    <Neo.Card icon="fa-user-shield" title="Username" >
                        {user.userName}
                    </Neo.Card>
                    <Neo.Card icon="key" title="Claims" >
                        <p style={codeFormat}>
                            {JSON.stringify((user as any).claims, undefined, 2)}
                        </p>
                    </Neo.Card>
                    <Neo.Card icon="user_attributes" title="Authorisation Roles" >
                        <p style={codeFormat}>
                            {JSON.stringify((this.authorisationService as any).roleDictionary, undefined, 2)}
                        </p>
                    </Neo.Card>
                    <Neo.Card icon="fa-edit" title="User Data">
                        <p style={codeFormat}>
                            {JSON.stringify((user as any).userData, undefined, 2)}
                        </p>
                    </Neo.Card>
                </div>
                }
            </div>
        )
    }
}