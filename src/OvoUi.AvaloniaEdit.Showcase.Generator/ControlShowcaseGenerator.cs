using System.Collections.Immutable;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OvoUi.AvaloniaEdit.Showcase.Generator;

[Generator]
public sealed class ControlShowcaseGenerator : IIncrementalGenerator
{
    private const string OvoXmlNamespace = "https://github.com/tiouoo/ovoui.avalonia";

    private static readonly DiagnosticDescriptor InvalidXaml = new(
        "OVOSC001",
        "ControlShowcase source could not be read",
        "Could not inspect '{0}': {1}",
        "OvoUi.ControlShowcase",
        DiagnosticSeverity.Warning,
        true);

    private static readonly DiagnosticDescriptor MissingSourceKey = new(
        "OVOSC002",
        "ControlShowcase SourceKey is missing",
        "ControlShowcase in '{0}' at line {1} needs a unique SourceKey for automatic code generation",
        "OvoUi.ControlShowcase",
        DiagnosticSeverity.Warning,
        true);

    private static readonly DiagnosticDescriptor DuplicateSourceKey = new(
        "OVOSC003",
        "ControlShowcase SourceKey is duplicated",
        "SourceKey '{0}' is used by more than one ControlShowcase",
        "OvoUi.ControlShowcase",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var parsedFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase) ||
                           file.Path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            .Select(ParseFile);

        context.RegisterSourceOutput(parsedFiles.Collect(), EmitSources);
    }

    private static ParsedFile ParseFile(AdditionalText file, CancellationToken cancellationToken)
    {
        var text = file.GetText(cancellationToken);
        if (text is null)
            return new ParsedFile(file.Path, [], "The file has no text content.");

        try
        {
            var document = XDocument.Parse(
                text.ToString(),
                LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var samples = document
                .Descendants()
                .Where(IsControlShowcase)
                .Select(element => ParseSample(element, file.Path))
                .ToImmutableArray();
            return new ParsedFile(file.Path, samples, null);
        }
        catch (XmlException exception)
        {
            return new ParsedFile(file.Path, [], exception.Message);
        }
    }

    private static Sample ParseSample(XElement element, string path)
    {
        var sourceKey = element.Attribute("SourceKey")?.Value?.Trim();
        var hasExplicitCode = element.Attribute("Code") is not null ||
                              element.Elements().Any(child =>
                                  child.Name.LocalName.Equals("ControlShowcase.Code", StringComparison.Ordinal));
        var lineInfo = (IXmlLineInfo)element;
        var source = FormatContent(element);
        return new Sample(sourceKey, source, path, lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0, hasExplicitCode);
    }

    private static bool IsControlShowcase(XElement element)
    {
        if (!element.Name.LocalName.Equals("ControlShowcase", StringComparison.Ordinal))
            return false;

        var xmlNamespace = element.Name.NamespaceName;
        return xmlNamespace.Equals(OvoXmlNamespace, StringComparison.Ordinal) ||
               xmlNamespace.StartsWith("clr-namespace:OvoUi.AvaloniaEdit.Showcase.Controls", StringComparison.Ordinal) ||
               xmlNamespace.StartsWith("using:OvoUi.AvaloniaEdit.Showcase.Controls", StringComparison.Ordinal);
    }

    private static string FormatContent(XElement element)
    {
        var namespaces = GetNamespacesInScope(element);
        var defaultNamespace = element.GetDefaultNamespace();
        var root = new XElement(
            XName.Get("__control_showcase_root", defaultNamespace.NamespaceName),
            namespaces,
            element.Nodes().Where(node => !IsControlShowcaseProperty(node)));
        var formatted = root.ToString();
        var contentStart = formatted.IndexOf('>') + 1;
        var contentEnd = formatted.LastIndexOf("</", StringComparison.Ordinal);
        if (contentStart <= 0 || contentEnd < contentStart)
            return string.Empty;

        return NormalizeIndentation(formatted.Substring(contentStart, contentEnd - contentStart));
    }

    private static bool IsControlShowcaseProperty(XNode node) =>
        node is XElement child && child.Name.LocalName.StartsWith("ControlShowcase.", StringComparison.Ordinal);

    private static IEnumerable<XAttribute> GetNamespacesInScope(XElement element)
    {
        var seenPrefixes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var current in element.AncestorsAndSelf())
        {
            foreach (var attribute in current.Attributes().Where(attribute => attribute.IsNamespaceDeclaration))
            {
                var prefix = attribute.Name.LocalName == "xmlns" ? string.Empty : attribute.Name.LocalName;
                if (seenPrefixes.Add(prefix))
                    yield return new XAttribute(attribute);
            }
        }
    }

    private static string NormalizeIndentation(string source)
    {
        var lines = source.Replace("\r\n", "\n").Split('\n').ToList();
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
            lines.RemoveAt(0);
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[lines.Count - 1]))
            lines.RemoveAt(lines.Count - 1);

        var indentation = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.TakeWhile(char.IsWhiteSpace).Count())
            .DefaultIfEmpty(0)
            .Min();

        return string.Join(
            "\n",
            lines.Select(line => line.Length >= indentation ? line.Substring(indentation).TrimEnd() : string.Empty));
    }

    private static void EmitSources(SourceProductionContext context, ImmutableArray<ParsedFile> files)
    {
        foreach (var file in files.Where(file => file.Error is not null))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidXaml, Location.None, file.Path, file.Error));
        }

        var samples = files.SelectMany(file => file.Samples).ToImmutableArray();
        foreach (var sample in samples.Where(sample => string.IsNullOrWhiteSpace(sample.SourceKey) && !sample.HasExplicitCode))
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSourceKey, Location.None, sample.Path, sample.Line));
        }

        var keyedSamples = samples
            .Where(sample => !string.IsNullOrWhiteSpace(sample.SourceKey))
            .ToImmutableArray();
        var duplicates = keyedSamples
            .GroupBy(sample => sample.SourceKey!, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .ToImmutableArray();
        foreach (var duplicate in duplicates)
        {
            context.ReportDiagnostic(Diagnostic.Create(DuplicateSourceKey, Location.None, duplicate.Key));
        }

        var duplicateKeys = new HashSet<string>(
            duplicates.Select(group => group.Key),
            StringComparer.Ordinal);
        var validSamples = keyedSamples.Where(sample => !duplicateKeys.Contains(sample.SourceKey!));
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("using System.Runtime.CompilerServices;");
        source.AppendLine();
        source.AppendLine("namespace OvoUi.AvaloniaEdit.Showcase.Generated;");
        source.AppendLine();
        source.AppendLine("internal static class ControlShowcaseGeneratedSources");
        source.AppendLine("{");
        source.AppendLine("    [ModuleInitializer]");
        source.AppendLine("    internal static void Register()");
        source.AppendLine("    {");
        foreach (var sample in validSamples)
        {
            source.Append("        global::OvoUi.AvaloniaEdit.Showcase.Controls.ShowcaseSourceRegistry.Register(");
            source.Append(ToLiteral(sample.SourceKey!));
            source.Append(", ");
            source.Append(ToLiteral(sample.Source));
            source.AppendLine(");");
        }

        source.AppendLine("    }");
        source.AppendLine("}");
        context.AddSource("ControlShowcaseGeneratedSources.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static string ToLiteral(string value)
    {
        var result = new StringBuilder(value.Length + 2);
        result.Append('"');
        foreach (var character in value)
        {
            switch (character)
            {
                case '\\': result.Append("\\\\"); break;
                case '"': result.Append("\\\""); break;
                case '\r': result.Append("\\r"); break;
                case '\n': result.Append("\\n"); break;
                case '\t': result.Append("\\t"); break;
                default:
                    if (character < ' ' || character > '~')
                        result.Append("\\u").Append(((int)character).ToString("x4"));
                    else
                        result.Append(character);
                    break;
            }
        }

        return result.Append('"').ToString();
    }

    private sealed class ParsedFile
    {
        public ParsedFile(string path, ImmutableArray<Sample> samples, string? error)
        {
            Path = path;
            Samples = samples;
            Error = error;
        }

        public string Path { get; }
        public ImmutableArray<Sample> Samples { get; }
        public string? Error { get; }
    }

    private sealed class Sample
    {
        public Sample(string? sourceKey, string source, string path, int line, bool hasExplicitCode)
        {
            SourceKey = sourceKey;
            Source = source;
            Path = path;
            Line = line;
            HasExplicitCode = hasExplicitCode;
        }

        public string? SourceKey { get; }
        public string Source { get; }
        public string Path { get; }
        public int Line { get; }
        public bool HasExplicitCode { get; }
    }
}
