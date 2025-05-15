import { injectable, inject } from '@theia/core/shared/inversify';
import { Command, CommandContribution, CommandRegistry, MenuContribution, MenuModelRegistry, MessageService } from '@theia/core/lib/common';
import { CommonMenus } from '@theia/core/lib/browser';

export const ScriptEdCommand: Command = {
    id: 'ScriptEd.command',
    label: 'Say Hello'
};

@injectable()
export class ScriptEdCommandContribution implements CommandContribution {
    
    @inject(MessageService)
    protected readonly messageService!: MessageService;

    registerCommands(registry: CommandRegistry): void {
        registry.registerCommand(ScriptEdCommand, {
            execute: () => this.messageService.info('Hello World!')
        });
    }
}

@injectable()
export class ScriptEdMenuContribution implements MenuContribution {

    registerMenus(menus: MenuModelRegistry): void {
        menus.registerMenuAction(CommonMenus.EDIT_FIND, {
            commandId: ScriptEdCommand.id,
            label: ScriptEdCommand.label
        });
    }
}
