import { ModelBase, NeoModel, Rules } from '@singularsystems/neo-core';

@NeoModel
export default class CancelOrder extends ModelBase {

    public orderId: number = 0;

    @Rules.Required()
    public reason: string = "";

    // Client only properties / methods
}