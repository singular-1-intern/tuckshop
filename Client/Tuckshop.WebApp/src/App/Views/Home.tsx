import * as React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import { observer } from 'mobx-react';

@observer
export default class Home extends Views.ViewBase {

    constructor(props: unknown) {
        super("", Views.EmptyViewModel, props);
    }

    public render() {
        return (
            <div>
                <Neo.GridLayout lg={3}>
                    <Neo.Card title="Dashboard item 1">

                    </Neo.Card>
                    <Neo.Card title="Dashboard item 2">

                    </Neo.Card>
                    <Neo.Card title="Dashboard item 3">

                    </Neo.Card>
                </Neo.GridLayout>
            </div>
        )
    }
}