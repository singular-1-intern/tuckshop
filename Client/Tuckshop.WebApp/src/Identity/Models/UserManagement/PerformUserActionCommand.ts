import { NeoModel, ValueObject } from '@singularsystems/neo-core';
import { UserManagementAction } from './UserManagementAction';

@NeoModel
export default class PerformUserActionCommand extends ValueObject
{
    public userId: string = "";

    public action: UserManagementAction | null = null;

    // Client only properties / methods
}