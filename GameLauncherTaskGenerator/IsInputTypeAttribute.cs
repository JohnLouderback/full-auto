using System;

namespace GameLauncherTaskGenerator;

/// <summary>
///   Marks a class as an input type for the TypeScript generator. This means that the class is
///   intended to be used as an input to a method or property that is exposed to TypeScript. This
///   has special implications for the TypeScript generator, such making nullable properties
///   marked as optional in TypeScript.
/// </summary>
public class IsInputTypeAttribute : Attribute {}
