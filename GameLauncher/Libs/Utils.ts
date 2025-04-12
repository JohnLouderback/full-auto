// Auto-generated. Do not edit manually.

/**
 * A collection of utility functions.
 *
 */
export function injectIntoEngine(engine: V8ScriptEngine): void {
    // @ts-expect-error - Function is injected by the engine
    return __Utils.InjectIntoEngine(engine);
}

    /**
     * Returns a promise that resolves after the specified number of milliseconds.
     *
     * @param milliseconds The number of milliseconds to wait.
     * @returns A promise that resolves after the specified number of
     * milliseconds.
     */
export function wait(milliseconds: number): Promise<void> {
    // @ts-expect-error - Function is injected by the engine
    return __Utils.Wait(milliseconds);
}

