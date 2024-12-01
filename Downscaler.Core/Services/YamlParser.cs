using Downscaler.Core.Contracts.Models.AppState;
using Downscaler.Core.Contracts.Models.Yaml;
using Downscaler.Core.Contracts.Services;
using Downscaler.Core.Models;
using Downscaler.Core.Models.Yaml;
using Downscaler.Core.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static Downscaler.Core.Utils.WindowUtils;

namespace Downscaler.Core.Services;

public class YamlParser(IAppState AppState) : IYamlParser {
  /// <inheritdoc />
  public bool ParseYaml(string path) {
    var yamlContent = GetYamlContent(path);
    var yamlConfig  = DeserializeYaml(yamlContent);

    // Validate the YAML configuration.
    var validationErrors = ValidateYamlConfig(yamlConfig).ToList();

    // If there are errors, log them and return false.
    if (validationErrors.Count != 0) {
      foreach (var error in validationErrors) {
        Console.WriteLine(error);
      }

      return false;
    }

    var updateErrors = UpdateAppState(yamlConfig).ToList();

    // If there are errors, log them and return false.
    if (updateErrors.Count != 0) {
      foreach (var error in updateErrors) {
        Console.WriteLine(error);
      }
    }

    return yamlConfig != null;
  }


  /// <summary>
  ///   Checks that all of the given properties are greater than zero. If any are not, then an
  ///   error message is returned. If all are greater than zero, then <c>null</c> is returned.
  /// </summary>
  /// <param name="properties">
  ///   The properties to check for being greater than zero. They're taken in the form of key/value
  ///   tuples.
  /// </param>
  /// <returns>
  ///   An error message if any of the properties are not greater than zero. Otherwise, <c>null</c>.
  /// </returns>
  private string? CheckForGreaterThanZero(params (string, double?)[] properties) {
    var lessThanZeroProperties = properties.Where(p => p.Item2 <= 0).ToArray();
    var count                  = lessThanZeroProperties.Length;

    // If any of the properties are less than or equal to zero, return an error message.
    if (count > 0) {
      return $"The following properties must be numeric values greater than zero: {
        string.Join(", ", lessThanZeroProperties.Select(p => $"\"{p.Item1}\""))
      }.";
    }

    // If all of the properties are greater than zero, return null, indicating no violation of the
    // rule.
    return null;
  }


  /// <summary>
  ///   Assumes the list of properties are mutually exclusive from each other and checks whether
  ///   any more than one has a value set. If more than one is set, it returns a string error
  ///   message. If one or none are set, it returns <c>null</c>, indicating that there is no
  ///   issue.
  /// </summary>
  /// <param name="properties">
  ///   The properties to check for mutual exclusion. They're taken in the form of key/value tuples.
  /// </param>
  /// <returns></returns>
  private string? CheckForMutualExclusion(params (string, object?)[] properties) {
    var setProperties = properties.Where(p => p.Item2 != null).ToArray();
    var count         = setProperties.Length;

    // If more than one property has a value set, return an error message.
    if (count > 1) {
      return $"The following properties are set, but mutually exclusive from each other: {
        string.Join(", ", setProperties.Select(p => p.Item1))
      }.";
    }

    // If one or none of the properties are set, return null, indicating no violation of the
    // mutual exclusion rule.
    return null;
  }


  /// <summary>
  ///   Checks that all of the given properties are not zero. If any are zero, then an error message
  ///   is returned. If all are not zero, then <c>null</c> is returned.
  /// </summary>
  /// <param name="properties">
  ///   The properties to check for being not zero. They're taken in the form of key/value
  ///   tuples.
  /// </param>
  /// <returns>
  ///   An error message if any of the properties are zero. Otherwise, <c>null</c>.
  /// </returns>
  private string? CheckForNotZero(params (string, double?)[] properties) {
    var zeroProperties = properties.Where(p => p.Item2 == 0).ToArray();
    var count          = zeroProperties.Length;

    // If any of the properties are zero, return an error message.
    if (count > 0) {
      return $"The following properties must be numeric values not equal to zero: {
        string.Join(", ", zeroProperties.Select(p => $"\"{p.Item1}\""))
      }.";
    }

    // If all of the properties are not zero, return null, indicating no violation of the rule.
    return null;
  }


  /// <summary>
  ///   Checks that the given property is one of the given values. If it is not, then an error
  ///   message is returned. If it is, then <c>null</c> is returned.
  /// </summary>
  /// <param name="properties"> The properties to check against along with an array of acceptable values. </param>
  /// <returns>
  ///   An error message if the property is not one of the given values. Otherwise, <c>null</c>.
  /// </returns>
  private string? CheckForOneOfValues(params (string, object?, object?[])[] properties) {
    var invalidProperties = properties.Where(p => !p.Item3.Contains(p.Item2)).ToArray();
    var count             = invalidProperties.Length;

    // If any of the properties are not one of the given values, return an error message.
    if (count > 0) {
      return $"The following properties must be one of the following values: \n\r{
        string.Join(
          "\n\r",
          invalidProperties.Select(
            p => $"\"{p.Item1}\" must be one of: {string.Join(", ", p.Item3.Where(v => v != null))}"
          )
        )
      }.";
    }

    // If all of the properties are one of the given values, return null, indicating no violation of the rule.
    return null;
  }


  /// <summary>
  ///   Deserializes the given YAML content into a <see cref="IYamlConfig" /> instance.
  /// </summary>
  /// <param name="yamlContent"> The content of the YAML file in string form to deserialize. </param>
  /// <returns></returns>
  private IYamlConfig DeserializeYaml(string yamlContent) {
    var deserializer = new DeserializerBuilder()
      .WithNamingConvention(HyphenatedNamingConvention.Instance)
      .WithTypeMapping<IDebugConfig, DebugConfig>()
      .Build();

    return deserializer.Deserialize<YamlConfig>(yamlContent);
  }


  /// <summary>
  ///   For a given path, returns the content of the YAML file at that path as a string.
  /// </summary>
  /// <param name="path"> The path to the YAML file. </param>
  /// <returns> The content of the YAML file as a string. </returns>
  private string GetYamlContent(string path) {
    return File.ReadAllText(path);
  }


  /// <summary>
  ///   Given a valid <see cref="IYamlConfig" /> instance, updates the application state using the
  ///   values from the configuration.
  /// </summary>
  /// <param name="yamlConfig"></param>
  /// <returns>
  ///   A list of error messages, if any. If the list is empty, then the configuration is valid.
  ///   Examples of errors that can occur at this juncture include window titles that don't match
  ///   any running windows, or process names that don't match any running processes.
  /// </returns>
  private IEnumerable<string> UpdateAppState(IYamlConfig yamlConfig) {
    var errors = new List<string>();

    // If the X position is set, set it in the app state.
    if (yamlConfig.X != null) {
      AppState.InitialX = yamlConfig.X.Value;
    }

    // If the Y position is set, set it in the app state.
    if (yamlConfig.Y != null) {
      AppState.InitialY = yamlConfig.Y.Value;
    }

    // If the downscale factor is set, set it in the app state.
    if (yamlConfig.DownscaleFactor != null) {
      AppState.DownscaleFactor = yamlConfig.DownscaleFactor.Value;
    }
    // Otherwise, use the scale width and height if either are set.
    else {
      if (yamlConfig.ScaleWidth != null) {
        AppState.DownscaleWidth = (uint)yamlConfig.ScaleWidth.Value;
      }

      if (yamlConfig.ScaleHeight != null) {
        AppState.DownscaleHeight = (uint)yamlConfig.ScaleHeight.Value;
      }
    }

    // If the window title is set, search for the window by title.
    if (yamlConfig.WindowTitle != null) {
      var windowByTitle = GetWindowForWindowTitle(yamlConfig.WindowTitle, yamlConfig.ClassName);

      // If a window was found, set it in the app state.
      if (windowByTitle is not null) {
        AppState.WindowToScale = (Win32Window)windowByTitle;
      }
      // If the window was null, and a class name was provided, log an error detailing that the
      // window with the given title and class name was not found.
      else if (yamlConfig.ClassName is not null) {
        errors.Add(
          $"No window with title \"{
            yamlConfig.WindowTitle
          }\" and class name \"{
            yamlConfig.ClassName
          }\" was found."
        );
      }
      // Otherwise, if no class name was provided, log an error detailing that the window with the
      // given title was not found.
      else {
        errors.Add($"No window with title \"{yamlConfig.WindowTitle}\" was found.");
      }
    }
    // Otherwise, if the process name is set, search for the window by process name.
    else if (yamlConfig.ProcessName != null) {
      var windowByProcessName =
        GetWindowForProcessName(yamlConfig.ProcessName, yamlConfig.ClassName);

      // If a window was found, set it in the app state.
      if (windowByProcessName is not null) {
        AppState.WindowToScale = (Win32Window)windowByProcessName;
      }
      // If the window was null, and a class name was provided, log an error detailing that the
      // window with the given process name and class name was not found.
      else if (yamlConfig.ClassName is not null) {
        errors.Add(
          $"No window with process name \"{
            yamlConfig.ProcessName
          }\" and class name \"{
            yamlConfig.ClassName
          }\" was found."
        );
      }
      // Otherwise, if no class name was provided, log an error detailing that the window with the
      // given process name was not found.
      else {
        errors.Add($"No window with process name \"{yamlConfig.ProcessName}\" was found.");
      }
    }
    else {
      errors.Add("No window title or process name was provided.");
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

    // Add the debug configuration to the app state if it's set.
    if (yamlConfig.Debug is not null) {
      if (yamlConfig.Debug.Enabled is not null) {
        AppState.DebugState.Enabled = yamlConfig.Debug.Enabled.Value;
      }

      if (yamlConfig.Debug.FontScale is not null) {
        AppState.DebugState.FontScale = yamlConfig.Debug.FontScale.Value;
      }

      if (yamlConfig.Debug.ShowFps is not null) {
        AppState.DebugState.ShowFps = yamlConfig.Debug.ShowFps.Value;
      }

      if (yamlConfig.Debug.ShowMouseCoordinates is not null) {
        AppState.DebugState.ShowMouseCoordinates = yamlConfig.Debug.ShowMouseCoordinates.Value;
      }

      if (yamlConfig.Debug.FontFamily is not null) {
        AppState.DebugState.FontFamily = yamlConfig.Debug.FontFamily switch {
          "extra-small" => FontFamily.ExtraSmall,
          "small"       => FontFamily.Small,
          "normal"      => FontFamily.Normal,
          "large"       => FontFamily.Large,
          _ => throw new InvalidOperationException(
                 $"Unknown font family: {yamlConfig.Debug.FontFamily}"
               )
        };
      }
    }

    return errors;
  }


  /// <summary>
  ///   Ensures that the given <see cref="IYamlConfig" /> instance is valid in its current state.
  ///   For example, we check that properties which are mutually exclusive are not both set.
  /// </summary>
  /// <param name="yamlConfig"></param>
  /// <returns>
  ///   A list of error messages, if any. If the list is empty, then the configuration is valid.
  /// </returns>
  private IEnumerable<string> ValidateYamlConfig(IYamlConfig yamlConfig) {
    var errors = new List<string?> {
      CheckForMutualExclusion(
        ("process-name", yamlConfig.ProcessName),
        ("window-title", yamlConfig.WindowTitle)
      ),
      CheckForMutualExclusion(
        ("downscale-factor", yamlConfig.DownscaleFactor),
        ("scale-width", yamlConfig.ScaleWidth)
      ),
      CheckForMutualExclusion(
        ("downscale-factor", yamlConfig.DownscaleFactor),
        ("scale-height", yamlConfig.ScaleHeight)
      ),
      CheckForGreaterThanZero(
        ("scale-width", yamlConfig.ScaleWidth),
        ("scale-height", yamlConfig.ScaleHeight),
        ("debug.font-scale", yamlConfig.Debug?.FontScale)
      ),
      CheckForNotZero(
        ("downscale-factor", yamlConfig.DownscaleFactor),
        ("scale-width", yamlConfig.ScaleWidth),
        ("scale-height", yamlConfig.ScaleHeight)
      ),
      CheckForOneOfValues(
        ("debug.font-family", yamlConfig.Debug?.FontFamily?.ToLower(),
         [null, /* "extra-small", */ "small", "normal", "large"])
      )
    };

    // Return all non-null error messages.
    return errors.Where(e => e != null).Select(e => e!);
  }
}
