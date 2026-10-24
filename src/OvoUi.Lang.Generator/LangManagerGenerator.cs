using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace OvoUi.Lang.Generator;

[Generator]
public sealed class LangManagerGenerator : IIncrementalGenerator
{
    private const string AttributeName = "OvoUi.Lang.LangManagerAttribute";
    private const string ResourceAttributeName = "OvoUi.Lang.LangResourceAttribute";

    private static readonly StringComparison PathComparison = Path.DirectorySeparatorChar == '\\'
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private static readonly DiagnosticDescriptor Invalid = Descriptor("LANG001", "Invalid localization configuration",
        "{0}", DiagnosticSeverity.Error);

    private static readonly DiagnosticDescriptor Missing = Descriptor("LANG002", "Missing translation",
        "Resource key '{0}' is missing in '{1}'; parent/default fallback will be used", DiagnosticSeverity.Warning);

    private static readonly DiagnosticDescriptor Extra = Descriptor("LANG003", "Unknown translation key",
        "Resource key '{0}' in '{1}' is absent from the default resource", DiagnosticSeverity.Warning);

    private static readonly DiagnosticDescriptor Malformed = Descriptor("LANG004", "Invalid localization resource",
        "Cannot read resource '{0}': {1}", DiagnosticSeverity.Error);

    private static readonly DiagnosticDescriptor Collision = Descriptor("LANG005", "Conflicting localization member",
        "Resource key '{0}' produces conflicting member '{1}'", DiagnosticSeverity.Error);

    private static DiagnosticDescriptor
        Descriptor(string id, string title, string message, DiagnosticSeverity severity) =>
        new(id, title, message, "OvoUi.Lang", severity, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var managers = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => new Manager((INamedTypeSymbol)ctx.TargetSymbol, ctx.Attributes[0],
                ctx.TargetNode.GetLocation()));
        var files = context.AdditionalTextsProvider
            .Where(static file => IsResource(file.Path))
            .Select(static (file, ct) => new ResourceFile(file.Path, file.GetText(ct)?.ToString()))
            .Collect();
        var directories = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
            options.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var path) ? path : "");
        context.RegisterSourceOutput(managers.Combine(files).Combine(directories), static (spc, input) =>
            Generate(spc, input.Left.Left, input.Left.Right, input.Right));
    }

    private static bool IsResource(string path) =>
        path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".resx", StringComparison.OrdinalIgnoreCase);

    private static void Generate(SourceProductionContext context, Manager manager, ImmutableArray<ResourceFile> files,
        string directory)
    {
        void Error(string message) => context.ReportDiagnostic(Diagnostic.Create(Invalid, manager.Location, message));
        var symbol = manager.Symbol;
        if (symbol.IsStatic || symbol.IsAbstract || symbol.IsRecord || symbol.Arity != 0 ||
            symbol.ContainingType != null ||
            symbol.BaseType?.SpecialType != SpecialType.System_Object ||
            symbol.DeclaringSyntaxReferences.Any(r =>
                r.GetSyntax() is ClassDeclarationSyntax c && !c.Modifiers.Any(SyntaxKind.PartialKeyword)) ||
            symbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared && c.Parameters.Length == 0))
        {
            Error(
                "[LangManager] requires a top-level, non-static, non-abstract, non-generic partial class without a base class or parameterless constructor.");
            return;
        }

        var resourcePath = manager.Attribute.ConstructorArguments.FirstOrDefault().Value as string;
        var autoDiscover = true;
        var defaultCulture = "";
        foreach (var argument in manager.Attribute.NamedArguments)
        {
            if (argument.Key == "AutoDiscover") autoDiscover = argument.Value.Value is true;
            if (argument.Key == "DefaultCulture") defaultCulture = argument.Value.Value as string ?? "";
        }

        try
        {
            defaultCulture = CultureInfo.GetCultureInfo(defaultCulture).Name;
        }
        catch (CultureNotFoundException)
        {
            Error("Invalid default culture: " + defaultCulture);
            return;
        }

        ResourceFile? Find(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !IsResource(path!))
            {
                Error("Resource paths must identify a JSON or RESX AdditionalFile.");
                return null;
            }

            try
            {
                var normalized = Normalize(path!);
                var matches = files.Where(f => directory.Length > 0
                    ? string.Equals(Normalize(Path.GetFullPath(f.Path)),
                        Normalize(Path.GetFullPath(Path.Combine(directory,
                            path!.Replace('\\', Path.DirectorySeparatorChar)
                                .Replace('/', Path.DirectorySeparatorChar)))), PathComparison)
                    : string.Equals(Normalize(f.Path), normalized, PathComparison) || Normalize(f.Path)
                        .EndsWith("/" + normalized.TrimStart('.', '/'), PathComparison)).ToArray();
                if (matches.Length == 1) return matches[0];
                Error(matches.Length == 0
                    ? "Resource is missing from AdditionalFiles: " + path
                    : "Ambiguous resource path; expose MSBuildProjectDirectory or use an absolute path: " + path);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException ||
                                       ex is PathTooLongException)
            {
                Error("Invalid resource path: " + path);
            }

            return null;
        }

        var baseFile = Find(resourcePath);
        if (baseFile is null) return;
        var explicitFiles = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var attribute in symbol.GetAttributes()
                     .Where(a => a.AttributeClass?.ToDisplayString() == ResourceAttributeName))
        {
            var culture = attribute.ConstructorArguments.ElementAtOrDefault(0).Value as string ?? "";
            try
            {
                culture = CultureInfo.GetCultureInfo(culture).Name;
            }
            catch (CultureNotFoundException)
            {
                Error("Invalid culture: " + culture);
                return;
            }

            if (culture.Length == 0 || explicitFiles.ContainsKey(culture))
            {
                Error("Each LangResource must specify a unique, non-empty culture: " + culture);
                return;
            }

            var file = Find(attribute.ConstructorArguments.ElementAtOrDefault(1).Value as string);
            if (file is null) return;
            explicitFiles.Add(culture, file);
        }

        var selected = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase) { [""] = baseFile };
        if (autoDiscover)
        {
            var normalizedBase = Normalize(baseFile.Path);
            var folder = normalizedBase.Substring(0, normalizedBase.LastIndexOf('/') + 1);
            var baseName = Path.GetFileNameWithoutExtension(normalizedBase);
            var extension = Path.GetExtension(normalizedBase);
            foreach (var file in files.OrderBy(f => f.Path, StringComparer.Ordinal))
            {
                var path = Normalize(file.Path);
                if (!string.Equals(path.Substring(0, path.LastIndexOf('/') + 1), folder, PathComparison) ||
                    !string.Equals(Path.GetExtension(path), extension, StringComparison.OrdinalIgnoreCase)) continue;
                var name = Path.GetFileNameWithoutExtension(path);
                if (!name.StartsWith(baseName + ".", StringComparison.OrdinalIgnoreCase)) continue;
                var culture = name.Substring(baseName.Length + 1);
                try
                {
                    culture = CultureInfo.GetCultureInfo(culture).Name;
                }
                catch (CultureNotFoundException)
                {
                    Error("Invalid culture suffix in " + file.Path);
                    return;
                }

                if (culture.Length == 0 || explicitFiles.ContainsKey(culture)) continue;
                if (selected.ContainsKey(culture))
                {
                    Error("Multiple files discovered for culture " + culture + "; use LangResource to select one.");
                    return;
                }

                selected.Add(culture, file);
            }
        }

        foreach (var pair in explicitFiles) selected[pair.Key] = pair.Value;

        var resources = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in selected)
        {
            try
            {
                resources.Add(pair.Key, Parse(pair.Value));
            }
            catch (Exception ex) when (ex is JsonException || ex is XmlException || ex is ArgumentException ||
                                       ex is InvalidOperationException)
            {
                context.ReportDiagnostic(Diagnostic.Create(Malformed, manager.Location, pair.Value.Path, ex.Message));
                return;
            }
        }

        var keys = resources[""].Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
        var members = new HashSet<string>(
        [
            "Instance", "Keys", "CreateResources", "GetString", "CurrentCulture", "CultureChanges", "GetObservable",
                "UpdateCulture", "AddResources", "GetTranslations", "ClearRuntimeResources", "Equals",
                "ReferenceEquals", "GetHashCode", "GetType", "ToString", "MemberwiseClone", "Finalize", symbol.Name
        ], StringComparer.Ordinal);
        foreach (var member in symbol.GetMembers())
            if (!member.IsImplicitlyDeclared)
                members.Add(member.Name);
        foreach (var key in keys)
        {
            var name = Identifier(key);
            if (!members.Add(name.TrimStart('@')))
            {
                context.ReportDiagnostic(Diagnostic.Create(Collision, manager.Location, key, name));
                return;
            }
        }

        foreach (var pair in resources.Where(p => p.Key.Length > 0))
        {
            foreach (var key in keys.Where(k => !pair.Value.ContainsKey(k)))
                context.ReportDiagnostic(Diagnostic.Create(Missing, manager.Location, key, pair.Key));
            foreach (var key in pair.Value.Keys.Where(k => !resources[""].ContainsKey(k)))
                context.ReportDiagnostic(Diagnostic.Create(Extra, manager.Location, key, pair.Key));
        }

        if (defaultCulture.Length > 0 && !resources.ContainsKey(defaultCulture))
            resources.Add(defaultCulture, resources[""]);
        var source = Build(manager, keys, resources, defaultCulture);
        context.AddSource(symbol.ToDisplayString().Replace("@", "") + ".Lang.g.cs",
            SourceText.From(source, Encoding.UTF8));
    }

    private static string Normalize(string path) => path.Replace('\\', '/').TrimEnd('/');

    private static Dictionary<string, string> Parse(ResourceFile file)
    {
        if (file.Content is null) throw new InvalidOperationException("The file could not be read.");
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            using var document = JsonDocument.Parse(file.Content);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("The JSON root must be an object.");
            Flatten(document.RootElement, "", result);
        }
        else
        {
            using var reader = XmlReader.Create(new StringReader(file.Content),
                new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            var document = XDocument.Load(reader);
            if (document.Root?.Name.LocalName != "root")
                throw new InvalidOperationException("The RESX root must be <root>.");
            foreach (var entry in document.Root.Elements("data"))
            {
                var type = (string?)entry.Attribute("type");
                if (entry.Attribute("mimetype") != null ||
                    (type != null && !type.StartsWith("System.String", StringComparison.Ordinal))) continue;
                var name = (string?)entry.Attribute("name");
                if (name is null || entry.Element("value") is not { } value)
                    throw new InvalidOperationException("RESX data requires a name and value.");
                result.Add(name, value.Value);
            }
        }

        return result;
    }

    private static void Flatten(JsonElement obj, string prefix, Dictionary<string, string> result)
    {
        var propertyNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in obj.EnumerateObject())
        {
            if (!propertyNames.Add(property.Name))
                throw new JsonException("Duplicate JSON property: " + property.Name);
            var key = prefix.Length == 0 ? property.Name : prefix + "_" + property.Name;
            if (property.Value.ValueKind == JsonValueKind.Object) Flatten(property.Value, key, result);
            else if (property.Value.ValueKind == JsonValueKind.String) result.Add(key, property.Value.GetString()!);
        }
    }

    private static string Identifier(string key)
    {
        var sb = new StringBuilder();
        if (key.Length == 0 || !SyntaxFacts.IsIdentifierStartCharacter(key[0])) sb.Append('_');
        foreach (var c in key) sb.Append(SyntaxFacts.IsIdentifierPartCharacter(c) ? c : '_');
        var name = sb.ToString();
        return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
    }

    private static string Literal(string value) =>
        SymbolDisplay.FormatLiteral(value, true);

    private static string Xml(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
        .Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u0085', '\n').Replace('\u2028', '\n')
        .Replace('\u2029', '\n');

    private static string Build(Manager manager, string[] keys,
        Dictionary<string, Dictionary<string, string>> resources, string defaultCulture)
    {
        var symbol = manager.Symbol;
        var typeName = "@" + symbol.Name;
        var sb = new StringBuilder("// <auto-generated/>\n#nullable enable\n");
        if (!symbol.ContainingNamespace.IsGlobalNamespace)
            sb.Append("namespace ").Append(symbol.ContainingNamespace.ToDisplayString()).AppendLine(";");
        sb.Append(symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal")
            .Append(" partial class ").Append(typeName).AppendLine(" : global::OvoUi.Lang.LangManagerBase\n{");
        sb.Append("    private ").Append(typeName).Append("() : base(CreateResources(), ")
            .Append(Literal(defaultCulture)).AppendLine(") { }");
        sb.Append("    public static ").Append(typeName).AppendLine(" Instance { get; } = new();");
        sb.AppendLine(
            "    private static global::System.Collections.Generic.IReadOnlyDictionary<string, global::System.Collections.Generic.IReadOnlyDictionary<string, string>> CreateResources() =>");
        sb.AppendLine(
            "        new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.IReadOnlyDictionary<string, string>>(global::System.StringComparer.OrdinalIgnoreCase)\n        {");
        foreach (var culture in resources.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            sb.Append("            [").Append(Literal(culture.Key)).AppendLine(
                "] = new global::System.Collections.Generic.Dictionary<string, string>(global::System.StringComparer.Ordinal)\n            {");
            foreach (var pair in culture.Value.Where(p => resources[""].ContainsKey(p.Key))
                         .OrderBy(p => p.Key, StringComparer.Ordinal))
                sb.Append("                [").Append(Literal(pair.Key)).Append("] = ").Append(Literal(pair.Value))
                    .AppendLine(",");
            sb.AppendLine("            },");
        }

        sb.AppendLine("        };");
        foreach (var key in keys)
        {
            Documentation(sb, "    ", key, resources, defaultCulture);
            sb.Append("    public global::OvoUi.Lang.LangObservableString ").Append(Identifier(key))
                .Append(" => GetString(").Append(Literal(key)).AppendLine(");");
        }

        sb.AppendLine("    public static class Keys\n    {");
        foreach (var key in keys)
        {
            Documentation(sb, "        ", key, resources, defaultCulture);
            sb.Append("        public static readonly global::OvoUi.Lang.LangKey ").Append(Identifier(key))
                .Append(" = new(").Append(Literal(key)).AppendLine(", Instance);");
        }

        return sb.AppendLine("    }\n}").ToString();
    }

    private static void Documentation(StringBuilder sb, string indent, string key,
        Dictionary<string, Dictionary<string, string>> resources, string defaultCulture)
    {
        void Line(string value)
        {
            foreach (var line in value.Split('\n')) sb.Append(indent).Append("/// ").AppendLine(line);
        }

        Line("<summary>");
        Line("<para>" + Xml(key) + "</para>");
        Line("<para>Default" + (defaultCulture.Length > 0 ? " (" + Xml(defaultCulture) + ")" : "") + ": " +
             Xml(resources[""][key]) + "</para>");
        foreach (var culture in resources.Keys.Where(c => c.Length > 0).OrderBy(c => c, StringComparer.Ordinal))
        {
            var current = CultureInfo.GetCultureInfo(culture);
            var fallback = !resources[culture].ContainsKey(key);
            while (!resources.TryGetValue(current.Name, out var values) || !values.ContainsKey(key))
                current = current.Parent;
            Line("<para>" + Xml(culture) + (fallback ? " (fallback)" : "") + ": " + Xml(resources[current.Name][key]) +
                 "</para>");
        }

        Line("</summary>");
    }

    private sealed class Manager(INamedTypeSymbol symbol, AttributeData attribute, Location location)
    {
        public INamedTypeSymbol Symbol { get; } = symbol;
        public AttributeData Attribute { get; } = attribute;
        public Location Location { get; } = location;
    }

    private sealed class ResourceFile(string path, string? content)
    {
        public string Path { get; } = path;
        public string? Content { get; } = content;
    }
}
