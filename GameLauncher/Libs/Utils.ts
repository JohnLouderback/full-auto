// Auto-generated. Do not edit manually.

/**
 * A collection of utility functions.
 *
 */
    /**
     * Returns a promise that never resolves. This is useful to await at the end
     * of a script to keep the script running indefinitely. The script will only
     * exit when forced.
     *
     * @returns A promise that never resolves.
     */
export function forever(): Promise<void> {
    // @ts-expect-error - Function is injected by the engine
    return __Utils.Forever();
}

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

