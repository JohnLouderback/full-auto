namespace GameLauncher.Utils.SendKeys;

/// <summary>
///   Parses a string of SendKeys input into a list of keystroke events. This expects a format
///   similar to the AutoHotkey SendKeys syntax. For instance:
///   <ul>
///     <li><c>"a"</c> -> press 'a' key down and up</li>
///     <li><c>"^a"</c> -> press 'Ctrl' + 'a' down and up</li>
///     <li><c>"!a"</c> -> press 'Alt' + 'a' down and up</li>
///     <li><c>"+a"</c> -> press 'Shift' + 'a' down and up</li>
///     <li><c>"#a"</c> -> press 'Win' + 'a' down and up</li>
///     <li><c>"{Enter}"</c> -> press 'Enter' key down and up</li>
///     <li><c>"{Del 4}"</c> -> press 'Delete' key 4 times</li>
///     <li><c>"{b down}"</c> -> press 'b' key down (no up event)</li>
///     <li><c>"{b up}"</c> -> release 'b' key</li>
///     <li><c>"^+{Enter}"</c> -> press 'Ctrl' + 'Shift' + 'Enter' down and up</li>
///     <li><c>"^+{Enter 2}"</c> -> press 'Ctrl' + 'Shift' + 'Enter' down and up twice</li>
///     <li><c>"^+{Enter down}"</c> -> press 'Ctrl' + 'Shift' + 'Enter' down (no up event)</li>
///   </ul>
/// </summary>
public class SendKeysParser {
  private readonly string input;
  private          int    pos;


  public SendKeysParser(string input) {
    this.input = input;
    pos        = 0;
  }


  /// <returns>
  ///   A list of <see cref="Keystroke" /> objects representing the parsed keystrokes.
  /// </returns>
  /// <inheritdoc cref="SendKeysParser" />
  public List<Keystroke> Parse() {
    var events = new List<Keystroke>();

    while (pos < input.Length) {
      var c = input[pos];

      if (IsModifier(c)) {
        // Collect one or more modifier symbols (^, +, !, #)
        var modifiers = new List<string>();
        while (pos < input.Length &&
               IsModifier(input[pos])) {
          modifiers.Add(MapModifier(input[pos]));
          pos++;
        }

        // Next token: if it starts with '{', parse braced token;
        // otherwise, parse a literal (single character).
        List<Keystroke> keyEvents;
        if (pos < input.Length &&
            input[pos] == '{') {
          keyEvents = ParseBracedToken();
        }
        else {
          keyEvents = ParseLiteralToken();
        }

        // Emit modifier key-down events, then the key events, then modifier key-up events (in reverse order).
        foreach (var mod in modifiers) {
          events.Add(new Keystroke(mod, true));
        }

        events.AddRange(keyEvents);

        modifiers.Reverse();
        events.AddRange(modifiers.Select(mod => new Keystroke(mod, false)));
      }
      else if (c == '{') {
        // Process a braced token (special key or with options).
        var tokenEvents = ParseBracedToken();
        events.AddRange(tokenEvents);
      }
      else {
        // Process a literal character.
        var tokenEvents = ParseLiteralToken();
        events.AddRange(tokenEvents);
      }
    }

    return events;
  }


  /// <summary>
  ///   Processes the contents of a braced token.
  /// </summary>
  /// <remarks>
  ///   Examples:
  ///   <list type="bullet">
  ///     <item>
  ///       <description><c>"Enter"</c> -> key press of Enter (down then up)</description>
  ///     </item>
  ///     <item>
  ///       <description><c>"Del 4"</c> -> press Delete key 4 times</description>
  ///     </item>
  ///     <item>
  ///       <description><c>"b down"</c> -> press 'b' key down (no up event)</description>
  ///     </item>
  ///     <item>
  ///       <description><c>"b up"</c> -> release 'b' key</description>
  ///     </item>
  ///   </list>
  /// </remarks>
  private static List<Keystroke> ProcessTokenContent(string content) {
    var events = new List<Keystroke>();
    // Split on whitespace.
    var parts = content.Split([' '], StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0) {
      return events;
    }

    var  keyName  = parts[0];
    bool downOnly = false, upOnly = false;
    var  repeat   = 1;

    // Process additional parts.
    for (var i = 1; i < parts.Length; i++) {
      var part = parts[i].ToLower();
      switch (part) {
        case "down":
          downOnly = true;
          break;
        case "up":
          upOnly = true;
          break;
        default: {
          if (int.TryParse(part, out var r)) {
            repeat = r;
          }

          break;
        }
      }
    }

    for (var i = 0; i < repeat; i++) {
      if (!upOnly) {
        events.Add(new Keystroke(keyName, true));
      }

      if (!downOnly) {
        events.Add(new Keystroke(keyName, false));
      }
    }

    return events;
  }


  /// <summary>
  ///   Returns true if the character is one of the modifier symbols.
  /// </summary>
  private bool IsModifier(char c) {
    return c == '^' || c == '+' || c == '!' || c == '#';
  }


  /// <summary>
  ///   Maps a modifier symbol to a key name.
  /// </summary>
  private string MapModifier(char c) {
    return c switch {
      '^' => "Ctrl",
      '+' => "Shift",
      '!' => "Alt",
      '#' => "Win",
      _   => c.ToString()
    };
  }


  /// <summary>
  ///   Parses a braced token (assumes current pos is at '{').
  /// </summary>
  private List<Keystroke> ParseBracedToken() {
    // Skip the opening brace.
    pos++;
    var start = pos;
    while (pos < input.Length &&
           input[pos] != '}') {
      pos++;
    }

    if (pos >= input.Length) {
      throw new Exception("Unclosed brace in input.");
    }

    var tokenContent = input.Substring(start, pos - start);
    pos++; // Skip the closing brace.
    return ProcessTokenContent(tokenContent);
  }


  /// <summary>
  ///   Parses a literal token: a single character not part of any special syntax.
  /// </summary>
  private List<Keystroke> ParseLiteralToken() {
    var events = new List<Keystroke>();
    // Here we take just one character as a token.
    var c = input[pos];
    pos++;
    events.Add(new Keystroke(c.ToString(), true));
    events.Add(new Keystroke(c.ToString(), false));
    return events;
  }
}
