using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameLauncherTaskGenerator;

public static class JsDocGenerator {
    /// <summary>
    ///   Generates a formatted JSDoc comment from the XML documentation associated with a C# method.
    ///   Allows specifying a leading indent (to align with the method) and a maximum column width for
    ///   wrapping.
    /// </summary>
    /// <param name="method">The C# method declaration.</param>
    /// <param name="indent">Leading whitespace to prefix each line (e.g. "    " for four spaces).</param>
    /// <param name="maxColumn">The maximum column width (default 80).</param>
    /// <returns>The formatted JSDoc comment as a string.</returns>
    public static string GenerateJsDoc(
    MethodDeclarationSyntax method,
    string indent = "",
    int maxColumn = 80
  ) {
    var xmlDocTrivia = method.GetLeadingTrivia()
      .Select(t => t.GetStructure())
      .OfType<DocumentationCommentTriviaSyntax>()
      .FirstOrDefault();

    if (xmlDocTrivia == null) {
      return string.Empty;
    }

    // Remove leading "///" from each line.
    var rawXml = xmlDocTrivia.ToFullString();
    var cleanedXml = string.Join(
      "\n",
      rawXml.Split('\n')
        .Select(line => line.TrimStart())
        .Select(line => line.StartsWith("///") ? line.Substring(3).TrimStart() : line)
        .Where(line => !string.IsNullOrWhiteSpace(line))
    );

    try {
      // Wrap the XML in a root element for proper parsing.
      var doc = XDocument.Parse("<root>" + cleanedXml + "</root>");
      return ConvertDocToJsDoc(doc.Root, indent, maxColumn);
    }
    catch {
      return string.Empty;
    }
  }


    /// <summary>
    ///   Converts the parsed XML documentation (wrapped in a root element) into a formatted JSDoc comment.
    /// </summary>
    private static string ConvertDocToJsDoc(XElement root, string indent, int maxColumn) {
    var lines = new List<string>();
    lines.Add($"{indent}/**");

    // Process <summary> element.
    var summaryElement = root.Element("summary");
    if (summaryElement != null) {
      var summaryText = ProcessInlineNodes(summaryElement.Nodes()).Trim();
      if (!string.IsNullOrWhiteSpace(summaryText)) {
        lines.AddRange(FormatJsDocBlock(summaryText, indent, maxColumn));
        lines.Add($"{indent} *"); // blank line after summary
      }
    }

    // Process <param> elements.
    foreach (var param in root.Elements("param")) {
      var paramName  = param.Attribute("name")?.Value ?? "";
      var paramText  = ProcessInlineNodes(param.Nodes()).Trim();
      var tagContent = $"@param {paramName} {paramText}".Trim();
      lines.AddRange(FormatJsDocBlock(tagContent, indent, maxColumn));
    }

    // Process <returns> element.
    var returnsElement = root.Element("returns");
    if (returnsElement != null) {
      var returnsText = ProcessInlineNodes(returnsElement.Nodes()).Trim();
      if (!string.IsNullOrWhiteSpace(returnsText)) {
        var tagContent = $"@returns {returnsText}";
        lines.AddRange(FormatJsDocBlock(tagContent, indent, maxColumn));
      }
    }

    // Process <exception> elements.
    foreach (var exception in root.Elements("exception")) {
      var exceptionName = exception.Attribute("cref")?.Value ?? "";
      if (exceptionName.StartsWith("T:")) {
        exceptionName = exceptionName.Substring(2);
      }

      var exceptionText = ProcessInlineNodes(exception.Nodes()).Trim();
      var tagContent    = $"@throws {exceptionName} {exceptionText}".Trim();
      lines.AddRange(FormatJsDocBlock(tagContent, indent, maxColumn));
    }

    // Optionally process <remarks> if needed.
    var remarksElement = root.Element("remarks");
    if (remarksElement != null) {
      var remarksText = ProcessInlineNodes(remarksElement.Nodes()).Trim();
      if (!string.IsNullOrWhiteSpace(remarksText)) {
        lines.AddRange(FormatJsDocBlock(remarksText, indent, maxColumn));
      }
    }

    lines.Add($"{indent} */");
    return string.Join("\n", lines);
  }


    /// <summary>
    ///   Splits a block of text into lines that respect the maximum column width.
    ///   Each line is prefixed with the given indent and " * " marker.
    /// </summary>
    private static IEnumerable<string> FormatJsDocBlock(string block, string indent, int maxColumn) {
    // The prefix for each line (indent + " * ") takes up some columns.
    var prefixLength = indent.Length + 3;
    // Calculate the available width for text.
    var availableWidth = Math.Max(maxColumn - prefixLength, 20);

    // Split the block into paragraphs (by existing newline) and wrap each paragraph.
    var paragraphs = block.Split(new[] { "\n" }, StringSplitOptions.None);
    foreach (var paragraph in paragraphs) {
      var trimmed = paragraph.Trim();
      if (string.IsNullOrEmpty(trimmed)) {
        yield return $"{indent} *";
        continue;
      }

      foreach (var wrapped in WrapText(trimmed, availableWidth)) {
        yield return $"{indent} * {wrapped}";
      }
    }
  }


    /// <summary>
    ///   Recursively processes XML nodes (text and elements) to build a string,
    ///   converting inline documentation tags (like <paramref>, <see>, and <c>) to JSDoc syntax.
    /// </summary>
    private static string ProcessInlineNodes(IEnumerable<XNode> nodes) {
    if (nodes == null) {
      return string.Empty;
    }

    return string.Concat(nodes.Select(ProcessXmlNode));
  }


    /// <summary>
    ///   Processes an individual XML node, converting inline elements as needed.
    /// </summary>
    private static string ProcessXmlNode(XNode node) {
    if (node is XText textNode) {
      return textNode.Value;
    }

    if (node is XElement element) {
      switch (element.Name.LocalName) {
        case "paramref":
          var paramName = element.Attribute("name")?.Value;
          return !string.IsNullOrEmpty(paramName) ? $"{{@link {paramName}}}" : string.Empty;

        case "see":
          var langword = element.Attribute("langword")?.Value;
          if (!string.IsNullOrEmpty(langword)) {
            return $"`{langword}`";
          }

          var cref = element.Attribute("cref")?.Value;
          if (!string.IsNullOrEmpty(cref)) {
            if (cref.StartsWith("T:")) {
              cref = cref.Substring(2);
            }

            return $"{{@link {cref}}}";
          }

          return string.Empty;

        case "c":
          return $"`{element.Value}`";

        default:
          // Process any child nodes recursively.
          return string.Concat(element.Nodes().Select(ProcessXmlNode));
      }
    }

    return string.Empty;
  }


    /// <summary>
    ///   Wraps the provided text into lines of at most <paramref name="maxWidth" /> characters.
    ///   The routine breaks lines at whitespace boundaries and will not break tokens.
    /// </summary>
    private static IEnumerable<string> WrapText(string text, int maxWidth) {
    var words       = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    var currentLine = "";
    foreach (var word in words) {
      // If the current line is empty, start with the word.
      if (string.IsNullOrEmpty(currentLine)) {
        currentLine = word;
      }
      // Otherwise, check if adding the next word exceeds the available width.
      else if (currentLine.Length + 1 + word.Length <= maxWidth) {
        currentLine += " " + word;
      }
      else {
        yield return currentLine;
        currentLine = word;
      }
    }

    if (!string.IsNullOrEmpty(currentLine)) {
      yield return currentLine;
    }
  }
}
