import React from 'react';
import { Neo } from '@singularsystems/neo-react';
import { Audit, Catalogue } from '@singularsystems/neo-react-services';
import { observer } from 'mobx-react';
import { catalogueRoutes } from "./CatalogueRoutes";

@observer
export default class CatalogueView extends Catalogue.CatalogueViewBase {

    constructor(props: unknown) {
        super("Catalogue", props, catalogueRoutes);
    }

    public static getRouteChildren() {
        return this.getViewRoutes(catalogueRoutes);
    }

    protected auditApiClient(): Audit.IAuditApiClient | null {
        // TODO: Add and return audit client.
        return null;
    }

    protected renderMenu() {
        return (
            <Neo.GridLayout lg={3} arrangeVertically>
                <div>
                    {this.tryRenderSection(catalogueRoutes.general, children => 
                        <Neo.Card title="General">{children}</Neo.Card>)}
                </div>
            </Neo.GridLayout>
        )
    }
}