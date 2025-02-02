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

    InjectIntoEngine: async (engine: any) => tryInvoke(
        // @ts-ignore
        __Tasks_InjectIntoEngine,
        engine
    ),

    Launch: async (path: any) => tryInvoke(
        // @ts-ignore
        __Tasks_Launch,
        path
    ),
};
