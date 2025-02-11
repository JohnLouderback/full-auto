// This file is auto-generated. Do not modify manually.
/**
 * A collection of utility functions.
 *
 */


/**
 * Returns a promise that resolves after the specified number of milliseconds.
 *
 * @param milliseconds The number of milliseconds to wait.
 * @returns A promise that resolves after the specified number of milliseconds.
 */
export function wait(milliseconds: number): Promise<void> {
    // @ts-expect-error - This function is injected into the engine dynamically.
    return __Utils.Wait(milliseconds);
}

