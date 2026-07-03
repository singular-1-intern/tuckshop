import { Rules } from '@singularsystems/neo-core';

export default class CancelOrder {

    public orderId: number = 0;

    @Rules.Required()
    public reason: string = "";

    // Client only properties / methods
}