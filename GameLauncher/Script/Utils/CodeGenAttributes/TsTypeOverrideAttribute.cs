namespace GameLauncher.Script.Utils.CodeGenAttributes;

/// <inheritdoc />
/// <summary>
///   An attribute that is passed a string to override the TypeScript type for a
///   class, property, method, or parameter. This is useful for cases where the default TypeScript
///   mapping is not suitable, such as when you want to use a specific TypeScript type such as a
///   union or a custom type.
/// </summary>
public class TsTypeOverrideAttribute : Attribute {
  public TsTypeOverrideAttribute(string tsType) {}
}
