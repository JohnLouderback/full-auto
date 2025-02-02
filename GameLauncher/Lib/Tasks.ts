// This file is auto-generated. Do not modify manually.

const cleanStackTrace = (stack: string): string => {
  const lines = stack.split('\\n');
  const filtered = lines.filter(
    line => !line.includes('at tryInvoke') && !line.includes('at V8ScriptEngine')
  );
  return filtered.join('\\n');
};

// @ts-ignore
const tryInvoke = async (func, ...args): any => {
  try {
    const returnValue = func(...args);
    if (returnValue instanceof Promise) {
      // @ts-ignore
      return (await returnValue);
    }
    return returnValue;
  } catch (error) {
    error.stack = 'bob';
    throw error;
  }
};

export class Tasks {
    /**
     * Launch the application at the specified path.
     *
     * @param path The path to the application.
     * @throws ArgumentException Thrown when {@link path} is `null` or empty.
     */
    public static async Launch(path: string): Promise<void> {
      return tryInvoke(
          // @ts-ignore
          __Tasks_Launch,
          path
      )
    }

}
