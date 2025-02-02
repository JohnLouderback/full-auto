// This file is auto-generated. Do not modify manually.
const tryInvoke = (func, ...args): any => {
    try {
        return func(...args);
    } catch (error) {
        throw error;
    }
};

export const Tasks = {

    /**
     * Launch the application at the specified path.
     * @param path The path to the application.
     * @throws ArgumentException Thrown when "path" is null or empty.
     */
    Launch: async (path: string): Promise<void> => tryInvoke(
        // @ts-ignore
        __Tasks_Launch,
        path
    ),

};
