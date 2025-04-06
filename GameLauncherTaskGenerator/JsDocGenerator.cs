using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameLauncherTaskGenerator;

/// <summary>
///   Generates formatted JSDoc comments from the XML documentation associated with a C# syntax node.
///   Supports additional XML constructs such as lists and inheriting documentation from other members.
/// </summary>
public static class JsDocGenerator {
  /// <summary>
  ///   Generates a formatted JSDoc comment from the XML documentation associated with a C# method.
  ///   Allows specifying a leading indent, a maximum column width for wrapping, and an optional
  ///   semantic model for resolving inherited documentation.
  /// </summary>
  /// <param name="method">The C# method declaration.</param>
  /// <param name="semanticModel">
  ///   An optional semantic model used to resolve inherited documentation via <c>&lt;inheritdoc&gt;</c>.
  /// </param>
  /// <param name="indent">Leading whitespace to prefix each line (e.g. "    " for four spaces).</param>
  /// <param name="maxColumn">The maximum column width (default 80).</param>
  /// <returns>The formatted JSDoc comment as a string.</returns>
  public static string GenerateJsDoc(
    MethodDeclarationSyntax method,
    SemanticModel? semanticModel = null,
    string indent = "",
    int maxColumn = 80
  ) {
    return GenerateJsDoc((SyntaxNode)method, semanticModel, indent, maxColumn);
  }


  /// <summary>
  ///   Generates a formatted JSDoc comment from the XML documentation associated with any C# syntax
  ///   node. This works for classes, properties, methods, etc. It allows specifying a leading
  ///   indent, a maximum column width, and an optional semantic model for resolving inherited
  ///   documentation.
  /// </summary>
  /// <param name="node">The C# syntax node.</param>
  /// <param name="semanticModel">
  ///   An optional semantic model used to resolve inherited documentation via <c>&lt;inheritdoc&gt;</c>.
  /// </param>
  /// <param name="indent">Leading whitespace to prefix each line (e.g. "    " for four spaces).</param>
  /// <param name="maxColumn">The maximum column width (default 80).</param>
  /// <returns>The formatted JSDoc comment as a string.</returns>
  public static string GenerateJsDoc(
    SyntaxNode node,
    SemanticModel? semanticModel = null,
    string indent = "",
    int maxColumn = 80
  ) {
    var xmlDocTrivia = node.GetLeadingTrivia()
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
      return ConvertDocToJsDoc(node, doc.Root, semanticModel, indent, maxColumn);
    }
    catch {
      return string.Empty;
    }
  }


  /// <summary>
  ///   Converts the parsed XML documentation (wrapped in a root element) into a formatted JSDoc comment.
  ///   This method processes common tags like <c>summary</c>, <c>param</c>, <c>returns</c>,
  ///   <c>exception</c>,
  ///   and <c>remarks</c>, as well as inline tags including <c>inheritdoc</c> and <c>list</c>.
  /// </summary>
  /// <param name="node">The original syntax node being documented.</param>
  /// <param name="root">The root XElement containing the wrapped XML documentation.</param>
  /// <param name="semanticModel">
  ///   An optional semantic model used to resolve inherited documentation via <c>&lt;inheritdoc&gt;</c>.
  /// </param>
  /// <param name="indent">Leading whitespace to prefix each line.</param>
  /// <param name="maxColumn">The maximum column width.</param>
  /// <returns>The formatted JSDoc comment as a string.</returns>
  private static string ConvertDocToJsDoc(
    SyntaxNode node,
    XElement root,
    SemanticModel semanticModel,
    string indent,
    int maxColumn
  ) {
    var lines = new List<string>();
    lines.Add($"{indent}/**");

    var isMethod = false;
    // Dictionary of parameter names and their syntax nodes.
    Dictionary<string, ParameterSyntax> parameters = null;
    if (node is MethodDeclarationSyntax method) {
      isMethod = true;
      // Create a dictionary of parameter names to their syntax nodes.
      parameters = method.ParameterList.Parameters
        .ToDictionary(p => p.Identifier.Text, p => p);
    }

    // If there's an <inheritdoc> element, process it first and merge it with the current XML.
    var inheritDocElement = root.Element("inheritdoc");
    if (inheritDocElement != null) {
      // Process inherited documentation.
      var inheritedXml = ResolveInheritdoc(inheritDocElement, node, semanticModel);
      if (!string.IsNullOrWhiteSpace(inheritedXml)) {
        var doc    = XDocument.Parse("<root>" + inheritedXml + "</root>");
        var merged = InheritDoc(doc.Root, root);
        // Replace the root with the merged documentation.
        root = merged;
      }
    }

    // Process <summary> element.
    var summaryElement = root.Element("summary");
    if (summaryElement != null) {
      var summaryText = ProcessInlineNodes(summaryElement.Nodes(), node, semanticModel).Trim();
      if (!string.IsNullOrWhiteSpace(summaryText)) {
        lines.AddRange(FormatJsDocBlock(summaryText, indent, maxColumn));
        lines.Add($"{indent} *"); // blank line after summary
      }
    }

    // Process <param> elements.
    foreach (var param in root.Elements("param")) {
      var paramName = param.Attribute("name")?.Value ?? "";
      if (isMethod &&
          parameters != null &&
          parameters.TryGetValue(paramName, out var paramNode)) {
        if (paramNode.Default != null) {
          paramName = $"[{paramName}={paramNode.Default.Value.GetText()}]";
        }
      }

      var paramText  = ProcessInlineNodes(param.Nodes(), node, semanticModel).Trim();
      var tagContent = $"@param {paramName} {paramText}".Trim();
      lines.AddRange(FormatJsDocBlock(tagContent, indent, maxColumn));
    }

    // Process <returns> element.
    var returnsElement = root.Element("returns");
    if (returnsElement != null) {
      var returnsText = ProcessInlineNodes(returnsElement.Nodes(), node, semanticModel).Trim();
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

      var exceptionText = ProcessInlineNodes(exception.Nodes(), node, semanticModel).Trim();
      var tagContent    = $"@throws {exceptionName} {exceptionText}".Trim();
      lines.AddRange(FormatJsDocBlock(tagContent, indent, maxColumn));
    }

    // Process <remarks> element.
    var remarksElement = root.Element("remarks");
    if (remarksElement != null) {
      var remarksText = ProcessInlineNodes(remarksElement.Nodes(), node, semanticModel).Trim();
      if (!string.IsNullOrWhiteSpace(remarksText)) {
        lines.AddRange(FormatJsDocBlock(remarksText, indent, maxColumn));
      }
    }

    lines.Add($"{indent} */");
    return string.Join("\n", lines);
  }


  // Helper method to search for a member matching a given cref in a type hierarchy.
  private static ISymbol? FindMemberInTypeHierarchy(INamedTypeSymbol? type, string cref) {
    while (type != null) {
      foreach (var member in type.GetMembers()) {
        // Match exact full name (preferred)
        if (member.ToDisplayString() == cref) {
          return member;
        }

        // Try to match just by name, as a fallback
        if (member.Name == cref) {
          return member;
        }

        // Try to match against signature if method
        if (member is IMethodSymbol method) {
          var signature = $"{
            method.Name
          }({
            string.Join(", ", method.Parameters.Select(p => p.Type.ToDisplayString()))
          })";
          if (cref == signature) {
            return method;
          }
        }
      }

      type = type.BaseType;
    }

    return null;
  }


  /// <summary>
  ///   Splits a block of text into lines that respect the maximum column width.
  ///   Each line is prefixed with the given indent and " * " marker.
  /// </summary>
  /// <param name="block">The text block to format.</param>
  /// <param name="indent">Leading whitespace for each line.</param>
  /// <param name="maxColumn">Maximum column width.</param>
  /// <returns>An enumerable of formatted lines.</returns>
  private static IEnumerable<string> FormatJsDocBlock(string block, string indent, int maxColumn) {
    // The prefix for each line (indent + " * ") takes up some columns.
    var prefixLength = indent.Length + 3;
    // Calculate available width for text.
    var availableWidth = Math.Max(maxColumn - prefixLength, 20);
    // Split the block into paragraphs (by newline) and wrap each paragraph.
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
  ///   Parses inherited documentation from the <c>inheritdoc</c> element and layers in overridden
  ///   XMLDoc elements.
  /// </summary>
  /// <returns> The merged XML documentation. </returns>
  private static XElement InheritDoc(
    XElement inheritedDocumentation,
    XElement overridenDocumentation
  ) {
    // If the inherited documentation is empty, return the overridden documentation.
    if (inheritedDocumentation == null ||
        !inheritedDocumentation.HasElements ||
        !(inheritedDocumentation.Element("member") is XElement member &&
          member.HasElements)) {
      return overridenDocumentation;
    }

    // If the overridden documentation is empty, return the inherited documentation.
    if (overridenDocumentation == null ||
        !overridenDocumentation.HasElements) {
      return inheritedDocumentation;
    }

    // Merge the two XML elements.
    var merged = new XElement("root");
    merged.Add(inheritedDocumentation.Element("member")!.Elements());

    // Merge the overridden documentation elements, except the <inheritdoc> element. Ensure
    // existing elements are removed before adding new ones.
    foreach (var element in overridenDocumentation.Elements()) {
      if (element.Name.LocalName == "inheritdoc") {
        continue;
      }

      // Remove existing elements with the same name.
      merged.Elements(element.Name).Remove();
      // Add the new element.
      merged.Add(element);
    }

    return merged;
  }


  /// <summary>
  ///   Recursively processes XML nodes (both text and elements) to build a string.
  ///   Converts inline documentation tags like <c>paramref</c>, <c>see</c>, <c>c</c>, <c>list</c>, and
  ///   <c>inheritdoc</c>
  ///   into corresponding JSDoc syntax.
  /// </summary>
  /// <param name="nodes">The collection of XML nodes.</param>
  /// <param name="documentedNode">The original syntax node being documented.</param>
  /// <param name="semanticModel">An optional semantic model for resolving inherited docs.</param>
  /// <returns>The concatenated string representing the processed inline nodes.</returns>
  private static string ProcessInlineNodes(
    IEnumerable<XNode> nodes,
    SyntaxNode documentedNode,
    SemanticModel semanticModel
  ) {
    if (nodes == null) {
      return string.Empty;
    }

    return string.Concat(nodes.Select(n => ProcessXmlNode(n, documentedNode, semanticModel)));
  }


  /// <summary>
  ///   Processes a <c>list</c> XML element, converting it into a text block with line breaks.
  ///   Supports bullet and numbered lists, and also processes an optional list header.
  /// </summary>
  /// <param name="listElement">The XML element representing the list.</param>
  /// <param name="documentedNode">The syntax node being documented.</param>
  /// <param name="semanticModel">A semantic model used for processing inline nodes, if needed.</param>
  /// <returns>A string representing the formatted list.</returns>
  private static string ProcessListElement(
    XElement listElement,
    SyntaxNode documentedNode,
    SemanticModel semanticModel
  ) {
    // Determine the list type (default is "bullet").
    var listType    = listElement.Attribute("type")?.Value?.ToLowerInvariant() ?? "bullet";
    var resultLines = new List<string>();
    // Optionally process a list header.
    var listHeader = listElement.Element("listheader");
    if (listHeader != null) {
      var headerText = ProcessInlineNodes(listHeader.Nodes(), documentedNode, semanticModel).Trim();
      if (!string.IsNullOrEmpty(headerText)) {
        resultLines.Add(headerText);
      }
    }

    var items = listElement.Elements("item").ToList();
    if (listType == "number") {
      var count = 1;
      foreach (var item in items) {
        var itemText = ProcessInlineNodes(item.Nodes(), documentedNode, semanticModel).Trim();
        resultLines.Add($"{count}. {itemText}");
        count++;
      }
    }
    else {
      // Default bullet list.
      foreach (var item in items) {
        var itemText = ProcessInlineNodes(item.Nodes(), documentedNode, semanticModel).Trim();
        resultLines.Add("- " + itemText);
      }
    }

    // Join list lines with a newline.
    return string.Join("\n", resultLines);
  }


  /// <summary>
  ///   Processes an individual XML node, converting inline elements as needed.
  ///   Supports inline elements such as <c>paramref</c>, <c>see</c>, <c>c</c>, <c>list</c>, and
  ///   <c>inheritdoc</c>.
  /// </summary>
  /// <param name="node">The XML node to process.</param>
  /// <param name="documentedNode">The syntax node for which documentation is being generated.</param>
  /// <param name="semanticModel">An optional semantic model for resolving inherited docs.</param>
  /// <returns>The string representation of the XML node.</returns>
  private static string ProcessXmlNode(
    XNode node,
    SyntaxNode documentedNode,
    SemanticModel semanticModel
  ) {
    if (node is XText textNode) {
      return textNode.Value;
    }

    if (node is XElement element) {
      switch (element.Name.LocalName) {
        case "paramref": {
          var paramName = element.Attribute("name")?.Value;
          return !string.IsNullOrEmpty(paramName) ? $"{{@link {paramName}}}" : string.Empty;
        }
        case "see": {
          var langword = element.Attribute("langword")?.Value;
          if (!string.IsNullOrEmpty(langword)) {
            return $"`{langword}`";
          }

          var cref = element.Attribute("cref")?.Value;
          if (!string.IsNullOrEmpty(cref)) {
            if (cref.StartsWith("T:") ||
                cref.StartsWith("M:") ||
                cref.StartsWith("P:") ||
                cref.StartsWith("F:") ||
                cref.StartsWith("E:")) {
              cref = cref.Substring(2);
            }

            // Try to adjust JS casing (camelCase for members)
            if (documentedNode != null &&
                semanticModel != null) {
              // Try to resolve the symbol
              var symbol = semanticModel.LookupSymbols(
                  documentedNode.SpanStart,
                  // Search for the identifier at the end of the cref. For example, if cref is
                  // "GameLauncher.Script.Objects.Monitor.ChangeDisplayModeResult.Undo",
                  // we want to find "Undo".
                  name: cref.Split('.').LastOrDefault()
                )
                .FirstOrDefault();
              if (symbol != null) {
                // Only change casing if it's not a type
                if (symbol.Kind != SymbolKind.NamedType &&
                    symbol.Kind != SymbolKind.Namespace) {
                  // Lowercase the member name
                  cref = char.ToLowerInvariant(cref[0]) + cref.Substring(1);
                }
              }
              else if (cref.Contains('.')) {
                // If it's a qualified name like ChangeDisplayModeResult.Undo
                var parts = cref.Split('.');
                if (parts.Length == 2) {
                  var typeSymbol = semanticModel
                    .LookupSymbols(documentedNode.SpanStart, name: parts[0])
                    .OfType<INamedTypeSymbol>()
                    .FirstOrDefault();

                  if (typeSymbol != null &&
                      typeSymbol.GetMembers(parts[1]).Any()) {
                    // Adjust member part
                    parts[1] = char.ToLowerInvariant(parts[1][0]) + parts[1].Substring(1);
                    cref     = string.Join(".", parts);
                  }
                }
              }
            }

            return $"{{@link {cref}}}";
          }

          return string.Empty;
        }
        case "c": {
          return $"`{element.Value.Trim()}`";
        }
        case "list": {
          // Process list elements (supports bullet and numbered lists).
          return ProcessListElement(element, documentedNode, semanticModel);
        }
        default: {
          // For any other element, process its child nodes recursively.
          return string.Concat(
            element.Nodes().Select(n => ProcessXmlNode(n, documentedNode, semanticModel))
          );
        }
      }
    }

    return string.Empty;
  }


  /// <summary>
  ///   Attempts to resolve inherited documentation for an <c>inheritdoc</c> element.
  ///   It first checks for a cref attribute to locate the target member. If none is provided or lookup
  ///   fails,
  ///   it falls back to retrieving documentation from the base or implemented member of the current
  ///   symbol.
  /// </summary>
  /// <param name="inheritdocElement">The XML element representing the <c>inheritdoc</c> tag.</param>
  /// <param name="documentedNode">The syntax node being documented.</param>
  /// <param name="semanticModel">
  ///   A semantic model used to resolve the symbol for inherited
  ///   documentation.
  /// </param>
  /// <returns>The inherited documentation text, or an empty string if none can be resolved.</returns>
  private static string ResolveInheritdoc(
    XElement inheritdocElement,
    SyntaxNode documentedNode,
    SemanticModel semanticModel
  ) {
    if (semanticModel == null) {
      // If no semantic model is provided, we cannot resolve inherited documentation.
      return string.Empty;
    }

    // Get the cref attribute if specified.
    var cref   = inheritdocElement.Attribute("cref")?.Value;
    var symbol = semanticModel.GetDeclaredSymbol(documentedNode);
    if (symbol == null) {
      return string.Empty;
    }

    var xml = string.Empty;
    if (!string.IsNullOrEmpty(cref)) {
      // Remove known prefixes.
      if (cref.StartsWith("T:") ||
          cref.StartsWith("M:") ||
          cref.StartsWith("P:") ||
          cref.StartsWith("F:") ||
          cref.StartsWith("E:")) {
        cref = cref.Substring(2);
      }

      // Attempt to locate a member within the containing type or its base types that matches the cref.
      var containingType = symbol.ContainingType;
      var target         = FindMemberInTypeHierarchy(containingType, cref);
      if (target != null) {
        xml = target.GetDocumentationCommentXml();
      }
    }
    else {
      // If no cref is provided, try to inherit from an overridden or explicitly implemented member.
      if (symbol is IMethodSymbol methodSymbol) {
        if (methodSymbol.OverriddenMethod != null) {
          // Traverse up the inheritance chain.
          var baseMethod = methodSymbol.OverriddenMethod;
          while (baseMethod != null &&
                 string.IsNullOrWhiteSpace(baseMethod.GetDocumentationCommentXml())) {
            baseMethod = baseMethod.OverriddenMethod;
          }

          if (baseMethod != null) {
            xml = baseMethod.GetDocumentationCommentXml();
          }
        }
        else if (methodSymbol.ExplicitInterfaceImplementations.Length > 0) {
          var target = methodSymbol.ExplicitInterfaceImplementations.First();
          xml = target.GetDocumentationCommentXml();
        }
      }
    }

    return xml ?? string.Empty;
  }


  /// <summary>
  ///   Wraps the provided text into lines of at most <paramref name="maxWidth" /> characters.
  ///   The routine breaks lines at whitespace boundaries and will not break tokens.
  /// </summary>
  /// <param name="text">The text to wrap.</param>
  /// <param name="maxWidth">Maximum allowed width for each line.</param>
  /// <returns>An enumerable of text lines that do not exceed the specified width.</returns>
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
