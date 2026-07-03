import { Neo } from '@singularsystems/neo-react';
import React from 'react';

export default class NotFound extends React.Component {

    public render() {
        return (
            <Neo.Card title="404">
                <h3>Page not found</h3>
                <p className="mt-4">The requested url could not be found.</p>
            </Neo.Card>     
        )
    }
}