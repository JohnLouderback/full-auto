// This file is auto-generated. Do not modify manually.

export class Tasks {
    /**
     * Launch the application at the specified path.
     *
     * @param path The path to the application.
     * @throws ArgumentException Thrown when {@link path} is `null` or empty.
     */
    public static async launch(path: string): Promise<void> {
      // @ts-expect-error - This function is injected into the engine dynamically.
      return  __Tasks.Launch(
          path
      )
    }

}
