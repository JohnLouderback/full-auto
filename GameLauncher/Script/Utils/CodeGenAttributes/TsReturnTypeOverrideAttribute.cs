namespace GameLauncher.Script.Utils.CodeGenAttributes;

/// <inheritdoc />
/// <summary>
///   An attribute that is passed a string to override the TypeScript return type for a
///   method or function. This is useful for cases where the default TypeScript
///   mapping is not suitable, such as when you want to use a specific TypeScript type such as a
///   union or a custom type or when you wish to specify a JavaScript object type that is not
///   an existing C# type, such as UInt8Array, Uint16Array, etc.
/// </summary>
public class TsReturnTypeOverrideAttribute : Attribute {
  /// <summary>
  ///   Store the Type of the decorated member, if applicable. This is used to determine the
  ///   overridden type at runtime. This is useful for cases where a JavaScript object is being
  ///   converted to a C# type, but the type in question is an interface and not a concrete class,
  ///   but where this attribute is used to specify a concrete type for the TypeScript
  ///   type override. In that case, we use the type specified in this attribute to determine the
  ///   correct type to use in C#.
  /// </summary>
  public Type Type { get; }


  /// <summary>
  ///   Replaces the type of the decorated member with the specified TypeScript type using the
  ///   string representation of the type. The string will be directly inserted into the
  ///   TypeScript code, so it should be a valid TypeScript type expression.
  /// </summary>
  /// <param name="tsType">
  ///   The TypeScript type to use as a replacement for the decorated member's return type.
  /// </param>
  public TsReturnTypeOverrideAttribute(string tsType) {}


  /// <summary>
  ///   Replaces the type of the decorated member with the specified TypeScript type using the
  ///   string representation of the type. The string will be directly inserted into the
  ///   TypeScript code, so it should be a valid TypeScript type expression. This
  ///   constructor also accepts an array of dependency types that are used to ensure that the
  ///   TypeScript type can be resolved correctly. This is useful for cases where the TypeScript type
  ///   depends on other types that need to be imported or referenced in the TypeScript code.
  /// </summary>
  /// <param name="tsType">
  ///   The TypeScript type to use as a replacement for the decorated member's return type.
  /// </param>
  /// <param name="deps">
  ///   One or more dependency types that are used to ensure that the TypeScript type can be
  ///   resolved correctly.
  /// </param>
  /// <exception cref="ArgumentNullException">
  ///   Thrown when any of the dependency types are null.
  /// </exception>
  public TsReturnTypeOverrideAttribute(string tsType, params Type[] deps) {
    // Ensure none of the dependencies are null.
    if (deps.Any(dep => dep is null)) {
      throw new ArgumentNullException(nameof(deps), "Dependency types cannot be null.");
    }
  }


  /// <summary>
  ///   Replaces the type of the decorated member with the specified type using the Type object.
  /// </summary>
  /// <param name="replacementType">
  ///   The type to use as a replacement for the decorated member's return type.
  /// </param>
  public TsReturnTypeOverrideAttribute(Type replacementType) {
    Type = replacementType ??
           throw new ArgumentNullException(
             nameof(replacementType),
             "Replacement type cannot be null."
           );
  }
}
