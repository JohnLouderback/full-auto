using System;

namespace GameLauncherTaskGenerator;

/// <summary>
///   Marks a class as an entry point for the TypeScript code generator. These classes will be
///   transformed into function exports in the generated TypeScript code.
/// </summary>
public class EntryPointExportAttribute : Attribute {}
