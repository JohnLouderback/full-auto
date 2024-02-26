using System.Reflection;
using CommandLine;
using CommandLine.Text;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Contracts.Services;
using DownscalerV3.Core.Models;
using DownscalerV3.Core.Utils;
using TypeInfo = CommandLine.TypeInfo;
using static DownscalerV3.Core.Utils.WindowUtils;

namespace DownscalerV3.Core.Services;

/// <summary>
///   Indicates an error occurred in validating the command line arguments after parsing was
///   successful.
/// </summary>
internal class ValidationError : NamedError {
  internal ValidationError(NameInfo nameInfo)
    : base(ErrorType.BadFormatConversionError, nameInfo) {}
}

public class Options {
  [Value(
    0,
    MetaName = "app",
    Required = true,
    HelpText =
      "Title of the window or name of the process to mirror. Titles are case-sensitive, and process names are not. Process names are discerned by the presence of the \".exe\" extension."
  )]
  public string App { get; set; }

  [Value(
    1,
    MetaName = "class",
    Required = false,
    HelpText =
      "Class name of the window to mirror. You can use a tool like \"Spy++\", \"Window Detective\", or similar to find the class name of a window. Class names are case-sensitive."
  )]
  public string? ClassName { get; set; }

  [Option(
    'f',
    "factor",
    Required = true,
    SetName = "factor",
    HelpText =
      "The factor by which to downscale the window. Mutually exclusive with the --scale-width and --scale-height options."
  )]
  public double? DownscaleFactor { get; set; }

  [Option(
    'W',
    "scale-width",
    Required = true,
    SetName = "dimensions",
    HelpText =
      "The width to which to scale the window. Requires setting the height as well. Mutually exclusive with the --scale-factor option."
  )]
  public uint? DownscaleWidth { get; set; }

  [Option(
    'H',
    "scale-height",
    Required = true,
    SetName = "dimensions",
    HelpText =
      "The height to which to scale the window. Requires setting the width as well. Mutually exclusive with the --scale-factor option."
  )]
  public uint? DownscaleHeight { get; set; }
}

public class ArgsParser(IAppState appState) : IArgsParser {
  private readonly IAppState AppState = appState;


  /// <inheritdoc />
  public bool ParseArgs(string[] args) {
    // First, hack off the first argument, which is the name of the executable.
    args = args[1..];

    var parser = new Parser(
      settings => {
        settings.CaseInsensitiveEnumValues = true;
        settings.CaseSensitive             = true;
        settings.HelpWriter                = null;
      }
    );

    // Parse the arguments.
    var result    = parser.ParseArguments<Options>(args);
    var succeeded = true;

    result.WithParsed(
        options => {
          var validationErrors = new List<string>();

          if (options.DownscaleFactor is > default(double) or < default(double)) {
            AppState.DownscaleFactor = (double)options.DownscaleFactor;
          }
          else if (options is { DownscaleWidth: > default(int), DownscaleHeight: > default(int) }) {
            AppState.DownscaleWidth  = (uint)options.DownscaleWidth;
            AppState.DownscaleHeight = (uint)options.DownscaleHeight;
          }
          else {
            // If factor is null, then we know width and height we set instead and still did not
            // pass validation.
            if (options.DownscaleFactor is null) {
              validationErrors.Add(
                "--scale-width and --scale-height must be a positive integer greater than 0."
              );
            }
            else {
              // Because factor and dimensions are mutually exclusive, if we get here, then the
              // factor is not null, and the dimensions are null, meaning the factor is invalid.
              validationErrors.Add(
                "--factor must not be equal to 0."
              );
            }
          }

          switch (IsStringTitleOrProcessName(options.App)) {
            case WindowSearchType.Title: {
              var windowByTitle = GetWindowForWindowTitle(options.App, options.ClassName);
              if (windowByTitle is not null) {
                AppState.WindowToScale = (Win32Window)windowByTitle;
              }
              else if (options.ClassName is not null) {
                validationErrors.Add(
                  $"No window with title \"{
                    options.App
                  }\" and class name \"{
                    options.ClassName
                  }\" was found."
                );
              }
              else {
                validationErrors.Add($"No window with title \"{options.App}\" was found.");
              }

              break;
            }
            case WindowSearchType.ProcessName: {
              var windowByProcessName = GetWindowForProcessName(options.App, options.ClassName);
              if (windowByProcessName is not null) {
                AppState.WindowToScale = (Win32Window)windowByProcessName;
              }
              else if (options.ClassName is not null) {
                validationErrors.Add(
                  $"No window with process name \"{
                    options.App
                  }\" and class name \"{
                    options.ClassName
                  }\" was found."
                );
              }
              else {
                validationErrors.Add($"No window with process name \"{options.App}\" was found.");
              }

              break;
            }
            case null:
              validationErrors.Add(
                $"No window with title \"{
                  options.App
                }\" or process name \"{
                  options.App
                }\" was found."
              );
              break;
          }

          var foundSourceWindow = AppState.WindowToScale;

          Console.WriteLine(
            $"""
            Found source window:
              Title: {
                foundSourceWindow.Title
              }
              Class: {
                foundSourceWindow.ClassName
              }
              Process: {
                foundSourceWindow.ProcessName
              }
              X: {
                foundSourceWindow.GetX()
              }px
              Y: {
                foundSourceWindow.GetY()
              }px
              Width: {
                foundSourceWindow.GetWidth()
              }px
              Height: {
                foundSourceWindow.GetHeight()
              }px
            """
          );

          if (validationErrors.Count > 0) {
            // We use reflection to instantiate the internal NotParsed<T> class, so we can pass it to
            // the DisplayHelp method to ensure it calls its onError callback. It's a dirty hack, but
            // it enables us to re-use the error handling logic of the CommandLineParser library.
            var notParsed = GenerateNotParsed([]);
            succeeded = false;
            DisplayHelp(notParsed, [], validationErrors);
          }
        }
      )
      .WithNotParsed(errs => DisplayHelp(result, errs));

    // If we were not able to parse the arguments, throw an exception.
    if (result.Tag == ParserResultType.NotParsed) {
      succeeded = false;
      return succeeded;
    }

    return succeeded;
  }


  /// <summary>
  ///   Displays the help text for the given parser result and errors.
  /// </summary>
  /// <param name="result"> The result of having parsed the command line arguments. </param>
  /// <param name="errs"> The errors that occurred during parsing, if any. </param>
  /// <param name="validationErrs">
  ///   The validation errors that occurred post-parsing, if any. Validation errors are errors that
  ///   occur after the arguments have been parsed, such as a value being out of range.
  /// </param>
  /// <typeparam name="T"> The type of the options that were parsed. </typeparam>
  private static void DisplayHelp<T>(
    ParserResult<T> result,
    IEnumerable<Error> errs,
    IEnumerable<string>? validationErrs = null
  ) {
    var helpText = HelpText.AutoBuild(
      result,
      helpText => {
        helpText.OptionComparison = (option1, option2) => {
          // If both options are values, compare them by their long names.
          if (option1.IsValue &&
              option2.IsValue) {
            // Unless one of them is required and the other is not, in which case the required one
            // comes first.
            if (option1.Required &&
                !option2.Required) {
              return -1;
            }

            if (!option1.Required &&
                option2.Required) {
              return 1;
            }

            return string.Compare(
              option1.LongName,
              option2.LongName,
              StringComparison.OrdinalIgnoreCase
            );
          }

          // Else, if option1 is a value, sort it first.
          if (option1.IsValue) {
            return -1;
          }

          // Else, if option2 is a value, sort it first.
          if (option2.IsValue) {
            return 1;
          }

          // If neither option is a value, default to the built-in comparison.
          return HelpText.RequiredThenAlphaComparison(option1, option2);
        };
        helpText.AddDashesToOption = true;
        // Enumerate the validation errors.
        var enumeratedValidationErrors = validationErrs as string[] ?? validationErrs?.ToArray();

        // If there are no validation errors, then return the default parsing errors handler.
        // Otherwise return the help text with the validation errors added to it.
        return validationErrs is null || enumeratedValidationErrors?.Length == 0
                 ? HelpText.DefaultParsingErrorsHandler(result, helpText)
                 : helpText
                   .AddPreOptionsLine(
                     Environment.NewLine + helpText.SentenceBuilder.ErrorsHeadingText()
                   )
                   .AddPreOptionsLines(enumeratedValidationErrors);
      },
      e => e
    );

    // If the help was requested, then print it to the console.
    if (errs.Any(err => err.Tag == ErrorType.HelpRequestedError)) {
      Console.WriteLine(helpText);
      return;
    }

    // Otherwise, this was invoked due to a genuine error - print it to the error stream.
    Console.Error.WriteLine(helpText);
  }


  /// <summary>
  ///   Generates a NotParsed&lt;Options&gt; instance with the given errors.
  /// </summary>
  /// <param name="errors"> The errors that occurred during parsing. </param>
  /// <returns> A NotParsed&lt;Options&gt; instance with the given errors. </returns>
  /// <exception cref="InvalidOperationException"> </exception>
  private NotParsed<Options> GenerateNotParsed(IEnumerable<Error> errors) {
    var notParsedType = typeof(NotParsed<>);
    var typeInfoCreate = typeof(TypeInfo).GetMethod(
      "Create",
      BindingFlags.Static | BindingFlags.NonPublic,
      null,
      [typeof(Type)],
      null
    );
    var notParsed = notParsedType
      .MakeGenericType(typeof(Options))
      .GetConstructor(
        BindingFlags.NonPublic | BindingFlags.Instance,
        null,
        [typeof(TypeInfo), typeof(IEnumerable<Error>)],
        null
      )
      ?.Invoke(
        [
          typeInfoCreate?.Invoke(
            null,
            [typeof(Options)]
          ),
          new[] {
            new ValidationError(NameInfo.EmptyName)
          }
        ]
      );

    if (notParsed is null) {
      throw new InvalidOperationException("Could not create an instance of NotParsed<Options>.");
    }

    return notParsed as NotParsed<Options>;
  }
}
