// This file is auto-generated. Do not modify manually.
const tryInvoke = async (func, ...args) => {
    try {
        const returnValue = func(...args);
        if (returnValue instanceof Promise) {
            await returnValue;
        }
    } catch (error) {
        throw error;
    }
};

export const Tasks = {
/**
 * Launch the application at the specified path.
 * @param path The path to the application.

 */
    Launch: async (path: any) => tryInvoke(
        // @ts-ignore
        __Tasks_Launch,
        path
    ),
    InjectIntoEngine: async (engine: any) => tryInvoke(
        // @ts-ignore
        __Tasks_InjectIntoEngine,
        engine
    ),
};
