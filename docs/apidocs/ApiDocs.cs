#:package System.Reflection.MetadataLoadContext@10.0.10
#:property EnableTrimAnalyzer=false
#:property EnableAotAnalyzer=false
#:property EnableSingleFileAnalyzer=false

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

return ApiDocs.Run(args);

/// <summary>
/// Turns the three Arlecchino assemblies and the XML documentation they ship with into the markdown
/// behind the API section of the site.
/// </summary>
internal sealed class ApiDocs
{
    private static readonly string[] Projects = ["Arlecchino.Core", "Arlecchino", "Arlecchino.Testing"];
    private static readonly string[] Modifiers = ["static", "abstract", "virtual", "override", "sealed", "readonly"];

    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private readonly string _output;
    private readonly Dictionary<string, XElement> _docs;
    private readonly Dictionary<string, string> _shipped;
    private readonly List<(Assembly Assembly, Type Type)> _types;
    private readonly Dictionary<string, string> _pages;
    private readonly Dictionary<string, string> _anchors;

    private ApiDocs(
        string output,
        Dictionary<string, XElement> docs,
        Dictionary<string, string> shipped,
        List<(Assembly Assembly, Type Type)> types)
    {
        _output = output;
        _docs = docs;
        _shipped = shipped;
        _types = types;
        _pages = types.ToDictionary(entry => entry.Type.FullName!, entry => Page(entry.Type), StringComparer.Ordinal);
        _anchors = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (_, type) in types)
        {
            foreach (var member in Sections(type).SelectMany(section => section.Members))
            {
                _anchors[Id(member)] = Anchor(Name(member));
            }
        }
    }

    public static int Run(string[] args)
    {
        var repo = Path.GetFullPath(Argument(args, "--repo") ?? "../Arlecchino");
        var output = Path.GetFullPath(Argument(args, "--out") ?? "docs/api");
        var configuration = Argument(args, "--configuration") ?? "Release";
        var framework = Argument(args, "--framework") ?? "net10.0";

        var binaries = new List<string>();
        var documentation = new List<string>();
        var probe = new List<string>();

        foreach (var project in Projects)
        {
            var directory = Path.Combine(repo, "src", project, "bin", configuration, framework);
            var assembly = Path.Combine(directory, project + ".dll");

            if (!File.Exists(assembly))
            {
                Console.Error.WriteLine($"{assembly} is missing. Build the framework first:");
                Console.Error.WriteLine($"  dotnet build {Path.Combine(repo, "Arlecchino.slnx")} -c {configuration}");
                return 1;
            }

            binaries.Add(assembly);
            documentation.Add(Path.Combine(directory, project + ".xml"));
            probe.AddRange(Directory.GetFiles(directory, "*.dll"));
            probe.AddRange(Dependencies(Path.Combine(directory, project + ".deps.json")));
        }

        probe.AddRange(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));

        using var context = new MetadataLoadContext(new PathAssemblyResolver(probe.Distinct()));

        var types = binaries
            .Select(context.LoadFromAssemblyPath)
            .SelectMany(assembly => assembly.GetExportedTypes().Select(type => (Assembly: assembly, Type: type)))
            .Where(entry => !Generated(entry.Type))
            .OrderBy(entry => entry.Type.Namespace, StringComparer.Ordinal)
            .ThenBy(entry => entry.Type.Name, StringComparer.Ordinal)
            .ToList();

        var generator = new ApiDocs(output, Documentation(documentation), Shipped(repo), types);
        generator.Write();

        return 0;
    }

    private static string? Argument(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    /// <summary>
    /// A library build leaves its package references in the NuGet cache rather than beside the
    /// assembly, so the dependency file is what says where to find them. Without them the reader
    /// cannot resolve a parameter typed as one.
    /// </summary>
    private static IEnumerable<string> Dependencies(string deps)
    {
        if (!File.Exists(deps))
        {
            yield break;
        }

        var packages = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
                       ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        using var document = JsonDocument.Parse(File.ReadAllText(deps));

        if (!document.RootElement.TryGetProperty("libraries", out var libraries) ||
            !document.RootElement.TryGetProperty("targets", out var targets))
        {
            yield break;
        }

        foreach (var target in targets.EnumerateObject())
        {
            foreach (var library in target.Value.EnumerateObject())
            {
                if (!library.Value.TryGetProperty("runtime", out var runtime) ||
                    !libraries.TryGetProperty(library.Name, out var declaration) ||
                    declaration.GetProperty("type").GetString() != "package")
                {
                    continue;
                }

                var folder = declaration.TryGetProperty("path", out var path)
                    ? path.GetString()
                    : library.Name.ToLowerInvariant().Replace('/', Path.DirectorySeparatorChar);

                foreach (var asset in runtime.EnumerateObject())
                {
                    var file = Path.Combine(packages, folder ?? "", asset.Name.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(file))
                    {
                        yield return file;
                    }
                }
            }
        }
    }

    // ------------------------------------------------------------------ inputs

    private static Dictionary<string, XElement> Documentation(IEnumerable<string> files)
    {
        var members = new Dictionary<string, XElement>(StringComparer.Ordinal);

        foreach (var file in files.Where(File.Exists))
        {
            foreach (var member in XDocument.Load(file).Descendants("member"))
            {
                var name = member.Attribute("name")?.Value;
                if (name is not null)
                {
                    members[name] = member;
                }
            }
        }

        return members;
    }

    /// <summary>
    /// The public API baseline carries the exact signature the analyzer holds the package to, nullable
    /// annotations included, which is more than metadata alone can say. Keyed by the same signature
    /// with the annotations stripped, it decorates what reflection produced.
    /// </summary>
    private static Dictionary<string, string> Shipped(string repo)
    {
        var declarations = new Dictionary<string, string>(StringComparer.Ordinal);
        var ambiguous = new HashSet<string>(StringComparer.Ordinal);

        foreach (var project in Projects)
        {
            var file = Path.Combine(repo, "src", project, "PublicAPI.Shipped.txt");
            if (!File.Exists(file))
            {
                continue;
            }

            foreach (var line in File.ReadLines(file))
            {
                var declaration = line.Trim();
                if (declaration.Length == 0 || declaration.StartsWith('#'))
                {
                    continue;
                }

                if (!declarations.TryAdd(Key(declaration), declaration))
                {
                    ambiguous.Add(Key(declaration));
                }
            }
        }

        foreach (var key in ambiguous)
        {
            declarations.Remove(key);
        }

        return declarations;
    }

    /// <summary>
    /// "static Arlecchino.Rendering.Theme.Header.get -&gt; Arlecchino.Rendering.TermColor!" becomes
    /// "Arlecchino.Rendering.Theme.Header.get-&gt;Arlecchino.Rendering.TermColor", which is also what
    /// the same member looks like once reflection has written it out.
    /// </summary>
    private static string Key(string declaration)
    {
        var text = declaration;

        bool trimmed;
        do
        {
            trimmed = false;
            foreach (var modifier in Modifiers)
            {
                if (text.StartsWith(modifier + " ", StringComparison.Ordinal))
                {
                    text = text[(modifier.Length + 1)..];
                    trimmed = true;
                }
            }
        }
        while (trimmed);

        text = Regex.Replace(text, @"\s*=\s*[^,)]+(?=[,)])", "");
        text = text.Replace("!", "").Replace("?", "");
        text = Regex.Replace(text, @"\b(this|params|ref|out|in|scoped)\s+", "");
        text = Regex.Replace(text, @"([\w>\]\)])\s+\w+(?=\s*[,)])", "$1");
        text = Regex.Replace(text, @"\s+", "");

        return text;
    }

    private static bool Generated(MemberInfo member) =>
        member.CustomAttributes.Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");

    // ------------------------------------------------------------------ pages

    private static string Page(Type type) => $"{Slug(type.Namespace!)}/{PageFile(type)}";

    private static string Slug(string ns) => ns.ToLowerInvariant();

    private static string PageFile(Type type) => type.Name.Replace('`', '-');

    private void Write()
    {
        if (Directory.Exists(_output))
        {
            Directory.Delete(_output, recursive: true);
        }

        Directory.CreateDirectory(_output);

        var namespaces = _types
            .GroupBy(entry => entry.Type.Namespace!)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToList();

        WriteRoot(namespaces);

        var position = 1;
        foreach (var group in namespaces)
        {
            WriteNamespace(group, position++);
        }

        Console.WriteLine($"{_types.Count} types across {namespaces.Count} namespaces written to {_output}");
    }

    private void WriteRoot(List<IGrouping<string, (Assembly Assembly, Type Type)>> namespaces)
    {
        var page = new StringBuilder();

        page.AppendLine("---");
        page.AppendLine("title: API reference");
        page.AppendLine("sidebar_label: API reference");
        page.AppendLine("sidebar_position: 0");
        page.AppendLine("description: Every public type in the Arlecchino packages, generated from the assemblies and the XML documentation they ship with.");
        page.AppendLine("---");
        page.AppendLine();
        page.AppendLine("# API reference");
        page.AppendLine();
        page.AppendLine("Every public and protected member of the three packages, generated from the assemblies and");
        page.AppendLine("the XML documentation they ship with. The written pages are the place to start; this is the");
        page.AppendLine("place to look a member up.");
        page.AppendLine();
        page.AppendLine("| Namespace | Assembly | Types |");
        page.AppendLine("|---|---|---|");

        foreach (var group in namespaces)
        {
            var assembly = group.First().Assembly.GetName().Name;
            page.AppendLine($"| [{group.Key}]({Slug(group.Key)}/index.md) | `{assembly}` | {group.Count()} |");
        }

        File.WriteAllText(Path.Combine(_output, "index.md"), page.ToString());
    }

    private void WriteNamespace(IGrouping<string, (Assembly Assembly, Type Type)> group, int position)
    {
        var directory = Path.Combine(_output, Slug(group.Key));
        Directory.CreateDirectory(directory);

        File.WriteAllText(
            Path.Combine(directory, "_category_.json"),
            $$"""
              {
                "label": "{{group.Key}}",
                "position": {{position}},
                "link": {"type": "doc", "id": "api/{{Slug(group.Key)}}/index"}
              }

              """);

        var page = new StringBuilder();

        page.AppendLine("---");
        page.AppendLine($"title: {group.Key}");
        page.AppendLine($"sidebar_label: {group.Key}");
        page.AppendLine("sidebar_position: 0");
        page.AppendLine("---");
        page.AppendLine();
        page.AppendLine($"# {group.Key}");
        page.AppendLine();

        foreach (var kind in new[] { "Classes", "Structs", "Interfaces", "Enums", "Delegates" })
        {
            var members = group.Where(entry => Kind(entry.Type) == kind).ToList();
            if (members.Count == 0)
            {
                continue;
            }

            page.AppendLine($"## {kind}");
            page.AppendLine();
            page.AppendLine("| Type | Summary |");
            page.AppendLine("|---|---|");

            foreach (var entry in members)
            {
                page.AppendLine($"| [`{Cell(Name(entry.Type))}`]({PageFile(entry.Type)}.md) | {Cell(Summary(entry.Type))} |");
            }

            page.AppendLine();
        }

        File.WriteAllText(Path.Combine(directory, "index.md"), page.ToString());

        foreach (var entry in group)
        {
            WriteType(entry.Assembly, entry.Type, directory);
        }
    }

    private static string Kind(Type type) =>
        type.IsEnum ? "Enums"
        : Delegate(type) ? "Delegates"
        : type.IsInterface ? "Interfaces"
        : type.IsValueType ? "Structs"
        : "Classes";

    private static string Noun(Type type) =>
        type.IsEnum ? "enum"
        : Delegate(type) ? "delegate"
        : type.IsInterface ? "interface"
        : type.IsValueType ? "struct"
        : "class";

    private static bool Delegate(Type type) => type.BaseType?.FullName == "System.MulticastDelegate";

    private void WriteType(Assembly assembly, Type type, string directory)
    {
        var page = new StringBuilder();
        var name = Name(type);
        var title = name.Replace("<", "&lt;").Replace(">", "&gt;");

        page.AppendLine("---");
        page.AppendLine($"title: {title}");
        page.AppendLine($"sidebar_label: {title}");
        page.AppendLine("---");
        page.AppendLine();
        page.AppendLine($"# {title} {Noun(type)}");
        page.AppendLine();
        page.AppendLine($"**Namespace:** `{type.Namespace}` &middot; **Assembly:** `{assembly.GetName().Name}`");
        page.AppendLine();

        var summary = Summary(type);
        if (summary.Length > 0)
        {
            page.AppendLine(summary);
            page.AppendLine();
        }

        page.AppendLine("```csharp");
        page.AppendLine(Declaration(type));
        page.AppendLine("```");
        page.AppendLine();

        var lineage = Lineage(type);
        if (lineage.Length > 0)
        {
            page.AppendLine(lineage);
            page.AppendLine();
        }

        Prose(page, type, "remarks", "Remarks");

        if (type.IsEnum)
        {
            WriteEnum(page, type);
        }
        else if (Delegate(type))
        {
            WriteDelegate(page, type);
        }
        else
        {
            WriteMembers(page, type);
        }

        Prose(page, type, "example", "Example");

        File.WriteAllText(Path.Combine(directory, PageFile(type) + ".md"), page.ToString());
    }

    private void Prose(StringBuilder page, MemberInfo member, string section, string heading)
    {
        var text = Section(member, section);
        if (text.Length == 0)
        {
            return;
        }

        page.AppendLine($"## {heading}");
        page.AppendLine();
        page.AppendLine(text);
        page.AppendLine();
    }

    private void WriteEnum(StringBuilder page, Type type)
    {
        page.AppendLine("## Fields");
        page.AppendLine();
        page.AppendLine("| Name | Value | Summary |");
        page.AppendLine("|---|---:|---|");

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var value = Convert.ToInt64(field.GetRawConstantValue(), CultureInfo.InvariantCulture);
            page.AppendLine($"| `{field.Name}` | `{value}` | {Cell(Summary(field))} |");
        }

        page.AppendLine();
    }

    private void WriteDelegate(StringBuilder page, Type type)
    {
        if (type.GetMethod("Invoke") is not { } invoke)
        {
            return;
        }

        var parameters = invoke.GetParameters().Select(p => $"{Short(Qualified(p.ParameterType))} {p.Name}");

        page.AppendLine("## Signature");
        page.AppendLine();
        page.AppendLine("```csharp");
        page.AppendLine($"{Short(Qualified(invoke.ReturnType))} Invoke({string.Join(", ", parameters)})");
        page.AppendLine("```");
        page.AppendLine();
    }

    private static (string Heading, List<MemberInfo> Members)[] Sections(Type type)
    {
        if (type.IsEnum || Delegate(type))
        {
            return [];
        }

        var constructors = type.GetConstructors(Declared).Where(Shown).OrderBy(c => c.GetParameters().Length).ToList();
        var fields = type.GetFields(Declared).Where(Shown).Where(f => !Generated(f)).OrderBy(f => f.Name, StringComparer.Ordinal).ToList();
        var properties = type.GetProperties(Declared).Where(Shown).Where(p => !Synthesized(p)).OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
        var all = type.GetMethods(Declared).Where(Shown).Where(m => !Synthesized(m)).ToList();
        var methods = all.Where(m => !m.IsSpecialName).OrderBy(m => m.Name, StringComparer.Ordinal).ToList();
        var operators = all.Where(m => m.IsSpecialName && m.Name.StartsWith("op_", StringComparison.Ordinal)).ToList();
        var events = type.GetEvents(Declared).Where(e => e.AddMethod is not null && Shown(e.AddMethod)).OrderBy(e => e.Name, StringComparer.Ordinal).ToList();

        return
        [
            ("Constructors", [.. constructors]),
            ("Fields", [.. fields]),
            ("Properties", [.. properties]),
            ("Methods", [.. methods]),
            ("Operators", [.. operators]),
            ("Events", [.. events]),
        ];
    }

    private void WriteMembers(StringBuilder page, Type type)
    {
        var sections = Sections(type);

        foreach (var (heading, members) in sections)
        {
            Summarise(page, heading, members);
        }

        foreach (var (heading, members) in sections)
        {
            Detail(page, heading, members);
        }
    }

    /// <summary>
    /// A record brings its own equality, printing and cloning. None of it is written by hand, none of
    /// it carries documentation, and a page of it says nothing a reader did not already know from the
    /// word "record" in the declaration. <c>Deconstruct</c> stays, since that one is used.
    /// </summary>
    private static bool Synthesized(MemberInfo member) =>
        Generated(member) && member.Name is "Equals" or "GetHashCode" or "ToString" or "PrintMembers"
            or "op_Equality" or "op_Inequality" or "<Clone>$" or "EqualityContract";

    private static bool Shown(MethodBase method) => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;

    private static bool Shown(FieldInfo field) => field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly;

    private static bool Shown(PropertyInfo property) =>
        (property.GetMethod is not null && Shown(property.GetMethod)) ||
        (property.SetMethod is not null && Shown(property.SetMethod));

    private void Summarise(StringBuilder page, string heading, List<MemberInfo> members)
    {
        if (members.Count == 0)
        {
            return;
        }

        page.AppendLine($"## {heading}");
        page.AppendLine();
        page.AppendLine("| Member | Summary |");
        page.AppendLine("|---|---|");

        foreach (var member in members)
        {
            page.AppendLine($"| [`{Cell(Name(member))}`](#{Anchor(Name(member))}) | {Cell(Summary(member))} |");
        }

        page.AppendLine();
    }

    private void Detail(StringBuilder page, string heading, List<MemberInfo> members)
    {
        if (members.Count == 0)
        {
            return;
        }

        page.AppendLine($"## {heading} in detail");
        page.AppendLine();

        foreach (var member in members)
        {
            page.AppendLine($"### `{Name(member)}` {{#{Anchor(Name(member))}}}");
            page.AppendLine();

            if (member.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "System.ObsoleteAttribute") is { } obsolete)
            {
                var message = obsolete.ConstructorArguments.Count > 0 ? obsolete.ConstructorArguments[0].Value as string : null;
                page.AppendLine($":::warning[Obsolete]\n\n{message ?? "This member is obsolete."}\n\n:::");
                page.AppendLine();
            }

            page.AppendLine("```csharp");
            page.AppendLine(Declaration(member));
            page.AppendLine("```");
            page.AppendLine();

            var summary = Summary(member);
            if (summary.Length > 0)
            {
                page.AppendLine(summary);
                page.AppendLine();
            }

            WriteParameters(page, member);
            WriteResult(page, member);
            WriteExceptions(page, member);

            var remarks = Section(member, "remarks");
            if (remarks.Length > 0)
            {
                page.AppendLine(remarks);
                page.AppendLine();
            }
        }
    }

    private void WriteParameters(StringBuilder page, MemberInfo member)
    {
        var parameters = member switch
        {
            MethodBase method => method.GetParameters(),
            PropertyInfo property => property.GetIndexParameters(),
            _ => [],
        };

        if (parameters.Length == 0)
        {
            return;
        }

        var documentation = Documented(member);

        page.AppendLine("**Parameters**");
        page.AppendLine();
        page.AppendLine("| Name | Type | Description |");
        page.AppendLine("|---|---|---|");

        foreach (var parameter in parameters)
        {
            var description = documentation?
                .Elements("param")
                .FirstOrDefault(e => e.Attribute("name")?.Value == parameter.Name);

            page.AppendLine($"| `{parameter.Name}` | {Cell(Link(parameter.ParameterType))} | {Cell(Markdown(description))} |");
        }

        page.AppendLine();
    }

    private void WriteResult(StringBuilder page, MemberInfo member)
    {
        var result = member switch
        {
            MethodInfo method when method.ReturnType.FullName != "System.Void" => Link(method.ReturnType),
            PropertyInfo property => Link(property.PropertyType),
            FieldInfo field => Link(field.FieldType),
            EventInfo @event when @event.EventHandlerType is not null => Link(@event.EventHandlerType),
            _ => null,
        };

        if (result is null)
        {
            return;
        }

        var prose = Section(member, member is MethodInfo ? "returns" : "value");
        var label = member is MethodInfo ? "**Returns**" : "**Type**";

        page.AppendLine(prose.Length > 0 ? $"{label} {result} — {prose}" : $"{label} {result}");
        page.AppendLine();
    }

    private void WriteExceptions(StringBuilder page, MemberInfo member)
    {
        var exceptions = Documented(member)?.Elements("exception").ToList();

        if (exceptions is null || exceptions.Count == 0)
        {
            return;
        }

        page.AppendLine("**Exceptions**");
        page.AppendLine();
        page.AppendLine("| Type | Thrown when |");
        page.AppendLine("|---|---|");

        foreach (var exception in exceptions)
        {
            page.AppendLine($"| {Cell(Reference(exception.Attribute("cref")?.Value ?? ""))} | {Cell(Markdown(exception))} |");
        }

        page.AppendLine();
    }

    // ------------------------------------------------------------------ signatures

    private string Declaration(Type type)
    {
        var builder = new StringBuilder();

        if (type.CustomAttributes.Any(a => a.AttributeType.FullName == "System.FlagsAttribute"))
        {
            builder.AppendLine("[Flags]");
        }

        builder.Append("public ");

        if (type.IsEnum)
        {
            var underlying = Keyword(type.GetEnumUnderlyingType().FullName!);
            return builder.Append("enum ").Append(Name(type)).Append(underlying == "int" ? "" : " : " + underlying).ToString();
        }

        if (Delegate(type))
        {
            builder.Append("delegate ");
        }
        else if (type.IsInterface)
        {
            builder.Append("interface ");
        }
        else if (type.IsValueType)
        {
            var immutable = type.CustomAttributes.Any(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute");
            builder.Append(immutable ? "readonly struct " : "struct ");
        }
        else
        {
            if (type is { IsAbstract: true, IsSealed: true })
            {
                builder.Append("static ");
            }
            else if (type.IsAbstract)
            {
                builder.Append("abstract ");
            }
            else if (type.IsSealed)
            {
                builder.Append("sealed ");
            }

            builder.Append("class ");
        }

        builder.Append(Name(type));

        var bases = Bases(type).Select(b => Short(Qualified(b))).ToList();
        if (bases.Count > 0)
        {
            builder.Append(" : ").Append(string.Join(", ", bases));
        }

        return builder.ToString();
    }

    private static IEnumerable<Type> Bases(Type type)
    {
        if (type.BaseType is { FullName: not "System.Object" and not "System.ValueType" and not "System.Enum" and not "System.MulticastDelegate" } baseType)
        {
            yield return baseType;
        }

        foreach (var contract in type.GetInterfaces())
        {
            yield return contract;
        }
    }

    private string Lineage(Type type)
    {
        if (type.IsEnum)
        {
            return "";
        }

        var parts = new List<string>();

        if (type.BaseType is { FullName: not "System.Object" and not "System.ValueType" and not "System.Enum" and not "System.MulticastDelegate" } baseType)
        {
            parts.Add($"**Inherits from** {Link(baseType)}");
        }

        var contracts = type.GetInterfaces().ToList();
        if (contracts.Count > 0)
        {
            parts.Add($"**Implements** {string.Join(", ", contracts.Select(Link))}");
        }

        var derived = _types
            .Select(entry => entry.Type)
            .Where(candidate => candidate.FullName != type.FullName)
            .Where(candidate => candidate.BaseType?.FullName == type.FullName ||
                                (type.IsInterface && candidate.GetInterfaces().Any(i => i.FullName == type.FullName)))
            .ToList();

        if (derived.Count is > 0 and <= 24)
        {
            parts.Add($"**{(type.IsInterface ? "Implemented by" : "Derived types")}** {string.Join(", ", derived.Select(Link))}");
        }

        return string.Join("  \n", parts);
    }

    private string Declaration(MemberInfo member)
    {
        if (member is PropertyInfo property)
        {
            return Property(property);
        }

        var exact = Exact(Reflected(member));

        return member is ConstructorInfo
            ? Rewrite(exact, constructor: true)
            : Rewrite(exact, constructor: false);
    }

    private string Exact(string reflected) => _shipped.GetValueOrDefault(Key(reflected), reflected);

    /// <summary>
    /// A property is two lines in the baseline, one per accessor, and one declaration on the page.
    /// </summary>
    private string Property(PropertyInfo property)
    {
        var owner = Qualified(property.DeclaringType!);
        var type = Qualified(property.PropertyType);
        var getter = property.GetMethod is not null && Shown(property.GetMethod);
        var setter = property.SetMethod is not null && Shown(property.SetMethod);
        var initial = property.SetMethod is not null &&
                      property.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                          .Any(m => m.FullName == "System.Runtime.CompilerServices.IsExternalInit");

        var declaration = getter
            ? Exact($"{Modifier(property.GetMethod!)}{owner}.{property.Name}.get -> {type}")
            : Exact($"{Modifier(property.SetMethod!)}{owner}.{property.Name}.set -> {type}");

        var arrow = declaration.LastIndexOf(" -> ", StringComparison.Ordinal);
        var exact = arrow < 0 ? type : declaration[(arrow + 4)..];

        var modifiers = new StringBuilder();
        foreach (var modifier in Modifiers)
        {
            if (declaration.StartsWith(modifier + " ", StringComparison.Ordinal))
            {
                modifiers.Append(modifier).Append(' ');
            }
        }

        var accessors = (getter, setter, initial) switch
        {
            (true, true, true) => "{ get; init; }",
            (true, true, false) => "{ get; set; }",
            (true, false, _) => "{ get; }",
            (false, true, true) => "{ init; }",
            _ => "{ set; }",
        };

        var index = property.GetIndexParameters();
        var name = index.Length == 0
            ? property.Name
            : $"this[{string.Join(", ", index.Select(p => $"{Qualified(p.ParameterType)} {p.Name}"))}]";

        return Short($"public {modifiers}{exact} {name} {accessors}");
    }

    /// <summary>
    /// The baseline writes a member as "Type.Member(args) -&gt; Result"; C# writes the result first and
    /// the declaring type not at all.
    /// </summary>
    private static string Rewrite(string declaration, bool constructor)
    {
        var arrow = declaration.LastIndexOf(" -> ", StringComparison.Ordinal);
        var left = arrow < 0 ? declaration : declaration[..arrow];
        var result = arrow < 0 ? null : declaration[(arrow + 4)..];

        var modifiers = new List<string>();

        bool trimmed;
        do
        {
            trimmed = false;
            foreach (var modifier in Modifiers)
            {
                if (left.StartsWith(modifier + " ", StringComparison.Ordinal))
                {
                    modifiers.Add(modifier);
                    left = left[(modifier.Length + 1)..];
                    trimmed = true;
                }
            }
        }
        while (trimmed);

        var accessor = left.EndsWith(".get", StringComparison.Ordinal) ? "get"
            : left.EndsWith(".set", StringComparison.Ordinal) ? "set"
            : left.EndsWith(".init", StringComparison.Ordinal) ? "init"
            : null;

        if (accessor is not null)
        {
            left = left[..^(accessor.Length + 1)];
        }

        var parenthesis = left.IndexOf('(');
        var name = parenthesis < 0 ? left : left[..parenthesis];
        var arguments = parenthesis < 0 ? "" : left[parenthesis..];

        var generic = name.IndexOf('<');
        var dot = name.LastIndexOf('.', generic < 0 ? name.Length - 1 : generic);
        if (dot >= 0)
        {
            name = name[(dot + 1)..];
        }

        var builder = new StringBuilder("public ");

        foreach (var modifier in modifiers)
        {
            builder.Append(modifier).Append(' ');
        }

        if (result is not null && !constructor)
        {
            builder.Append(result).Append(' ');
        }

        builder.Append(name).Append(arguments);

        if (accessor is not null)
        {
            builder.Append(" { ").Append(accessor).Append("; }");
        }
        else if (parenthesis >= 0)
        {
            builder.Append(';');
        }
        else
        {
            builder.Append(" { get; }");
        }

        return Short(builder.ToString());
    }

    /// <summary>
    /// Namespace-qualified names are noise on a page that already says which namespace it is in, and
    /// the project writes them out that way everywhere else too.
    /// </summary>
    private static string Short(string text) =>
        Regex.Replace(text.Replace("!", ""), @"\b(?:[A-Za-z_]\w*\.)+([A-Za-z_]\w*)", "$1");

    private static string Reflected(MemberInfo member) => member switch
    {
        ConstructorInfo constructor =>
            $"{Qualified(constructor.DeclaringType!)}.{constructor.DeclaringType!.Name.Split('`')[0]}({Arguments(constructor.GetParameters())}) -> void",
        MethodInfo method =>
            $"{Modifier(method)}{Qualified(method.DeclaringType!)}.{method.Name}{Generics(method)}({Arguments(method.GetParameters())}) -> {Qualified(method.ReturnType)}",
        PropertyInfo property =>
            $"{Qualified(property.DeclaringType!)}.{property.Name}.get -> {Qualified(property.PropertyType)}",
        FieldInfo field =>
            $"{(field.IsStatic ? "static " : "")}{Qualified(field.DeclaringType!)}.{field.Name} -> {Qualified(field.FieldType)}",
        EventInfo @event =>
            $"{Qualified(@event.DeclaringType!)}.{@event.Name} -> {Qualified(@event.EventHandlerType!)}",
        _ => member.Name,
    };

    private static string Modifier(MethodInfo method) =>
        method.IsStatic ? "static "
        : method.IsAbstract ? "abstract "
        : method is { IsVirtual: true, IsFinal: false } ? "virtual "
        : "";

    private static string Generics(MethodInfo method) =>
        method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(a => a.Name)) + ">"
            : "";

    private static string Arguments(ParameterInfo[] parameters) =>
        string.Join(", ", parameters.Select(p =>
            $"{(p.IsOut ? "out " : p.ParameterType.IsByRef ? "ref " : "")}{Qualified(p.ParameterType)} {p.Name}"));

    private static string Qualified(Type type)
    {
        if (type.IsByRef)
        {
            return Qualified(type.GetElementType()!);
        }

        if (type.IsArray)
        {
            return Qualified(type.GetElementType()!) + "[]";
        }

        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsConstructedGenericType)
        {
            var definition = type.GetGenericTypeDefinition().FullName!.Split('`')[0];
            return $"{definition}<{string.Join(", ", type.GetGenericArguments().Select(Qualified))}>";
        }

        return Keyword(type.FullName ?? type.Name);
    }

    private static string Keyword(string name) => name switch
    {
        "System.Void" => "void",
        "System.Boolean" => "bool",
        "System.Byte" => "byte",
        "System.SByte" => "sbyte",
        "System.Char" => "char",
        "System.Decimal" => "decimal",
        "System.Double" => "double",
        "System.Single" => "float",
        "System.Int32" => "int",
        "System.UInt32" => "uint",
        "System.Int64" => "long",
        "System.UInt64" => "ulong",
        "System.Int16" => "short",
        "System.UInt16" => "ushort",
        "System.Object" => "object",
        "System.String" => "string",
        _ => name,
    };

    private static string Name(Type type)
    {
        if (type.IsByRef)
        {
            return Name(type.GetElementType()!);
        }

        if (type.IsArray)
        {
            return Name(type.GetElementType()!) + "[]";
        }

        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        var bare = type.Name.Split('`')[0];

        if (!type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var keyword = Keyword(type.FullName ?? bare);
            return keyword.Contains('.') ? bare : keyword;
        }

        return $"{bare}<{string.Join(", ", type.GetGenericArguments().Select(Name))}>";
    }

    private static string Name(MemberInfo member) => member switch
    {
        ConstructorInfo constructor =>
            $"{constructor.DeclaringType!.Name.Split('`')[0]}({string.Join(", ", constructor.GetParameters().Select(Name))})",
        MethodInfo method =>
            $"{Operator(method.Name)}{Generics(method)}({string.Join(", ", method.GetParameters().Select(Name))})",
        _ => member.Name,
    };

    private static string Name(ParameterInfo parameter) =>
        (parameter.IsOut ? "out " : parameter.ParameterType.IsByRef ? "ref " : "") + Name(parameter.ParameterType);

    private static string Operator(string name) =>
        name.StartsWith("op_", StringComparison.Ordinal) ? "operator " + name[3..] : name;

    private string Link(Type type)
    {
        if (type.IsByRef)
        {
            return Link(type.GetElementType()!);
        }

        if (type.IsArray)
        {
            return Link(type.GetElementType()!) + "\\[\\]";
        }

        if (type.IsConstructedGenericType)
        {
            var arguments = string.Join(", ", type.GetGenericArguments().Select(Link));
            return $"{Link(type.GetGenericTypeDefinition())}&lt;{arguments}&gt;";
        }

        var display = type.Name.Split('`')[0];

        return type.FullName is not null && _pages.TryGetValue(type.FullName, out var page)
            ? $"[`{display}`](../{page}.md)"
            : $"`{Name(type)}`";
    }

    // ------------------------------------------------------------------ documentation

    private XElement? Documented(MemberInfo member) => _docs.GetValueOrDefault(Id(member));

    private string Summary(MemberInfo member) => Markdown(Documented(member)?.Element("summary"));

    private string Section(MemberInfo member, string name) => Markdown(Documented(member)?.Element(name));

    private static string Id(MemberInfo member) => member switch
    {
        Type type => "T:" + Id(type),
        ConstructorInfo constructor => "M:" + Id(constructor.DeclaringType!) + ".#ctor" + Parameters(constructor.GetParameters()),
        MethodInfo method => "M:" + Id(method.DeclaringType!) + "." + method.Name +
                             (method.IsGenericMethodDefinition ? "``" + method.GetGenericArguments().Length : "") +
                             Parameters(method.GetParameters()),
        PropertyInfo property => "P:" + Id(property.DeclaringType!) + "." + property.Name + Parameters(property.GetIndexParameters()),
        FieldInfo field => "F:" + Id(field.DeclaringType!) + "." + field.Name,
        EventInfo @event => "E:" + Id(@event.DeclaringType!) + "." + @event.Name,
        _ => "",
    };

    private static string Id(Type type) =>
        type.IsNested ? Id(type.DeclaringType!) + "." + type.Name : type.FullName ?? type.Name;

    private static string Parameters(ParameterInfo[] parameters) =>
        parameters.Length == 0 ? "" : "(" + string.Join(",", parameters.Select(p => Id(p.ParameterType, true))) + ")";

    private static string Id(Type type, bool inSignature)
    {
        _ = inSignature;

        if (type.IsByRef)
        {
            return Id(type.GetElementType()!, true) + "@";
        }

        if (type.IsArray)
        {
            return Id(type.GetElementType()!, true) + "[]";
        }

        if (type.IsGenericParameter)
        {
            return (type.DeclaringMethod is null ? "`" : "``") + type.GenericParameterPosition;
        }

        if (type.IsConstructedGenericType)
        {
            var definition = type.GetGenericTypeDefinition().FullName!.Split('`')[0];
            return $"{definition}{{{string.Join(",", type.GetGenericArguments().Select(a => Id(a, true)))}}}";
        }

        return type.FullName ?? type.Name;
    }

    private string Markdown(XElement? element)
    {
        if (element is null)
        {
            return "";
        }

        var builder = new StringBuilder();
        Render(element, builder);

        return Regex.Replace(builder.ToString().Trim(), @"[ \t]*\r?\n[ \t]*", "\n");
    }

    private void Render(XNode node, StringBuilder builder)
    {
        switch (node)
        {
            case XText text:
                builder.Append(Regex.Replace(text.Value, @"\s+", " "));
                break;

            case XElement element when element.Name == "see" || element.Name == "seealso":
                builder.Append(Reference(element.Attribute("cref")?.Value ?? element.Attribute("langword")?.Value ?? ""));
                break;

            case XElement element when element.Name == "paramref" || element.Name == "typeparamref":
                builder.Append('`').Append(element.Attribute("name")?.Value).Append('`');
                break;

            case XElement element when element.Name == "c":
                builder.Append('`').Append(element.Value.Trim()).Append('`');
                break;

            case XElement element when element.Name == "code":
                builder.Append("\n\n```csharp\n").Append(Dedent(element.Value)).Append("\n```\n\n");
                break;

            case XElement element when element.Name == "para":
                builder.Append("\n\n");
                foreach (var child in element.Nodes())
                {
                    Render(child, builder);
                }

                builder.Append("\n\n");
                break;

            case XElement element when element.Name == "list":
                builder.Append('\n');
                foreach (var item in element.Elements("item"))
                {
                    builder.Append("\n- ");
                    foreach (var child in (item.Element("description") ?? item).Nodes())
                    {
                        Render(child, builder);
                    }
                }

                builder.Append("\n\n");
                break;

            case XElement element:
                foreach (var child in element.Nodes())
                {
                    Render(child, builder);
                }

                break;
        }
    }

    private static string Dedent(string text)
    {
        var lines = text.Trim('\r', '\n').Split('\n').Select(line => line.TrimEnd()).ToList();
        var indent = lines
            .Where(line => line.Trim().Length > 0)
            .Select(line => line.Length - line.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        return string.Join("\n", lines.Select(line => line.Length >= indent ? line[indent..] : line));
    }

    private string Reference(string cref)
    {
        if (cref.Length < 2 || cref[1] != ':')
        {
            return cref.Length == 0 ? "" : $"`{cref}`";
        }

        var target = cref[2..];

        if (cref[0] == 'T')
        {
            var name = target.Split('.')[^1].Split('`')[0];
            return _pages.TryGetValue(target, out var page) ? $"[`{name}`](../{page}.md)" : $"`{name}`";
        }

        var arguments = target.IndexOf('(');
        var bare = arguments < 0 ? target : target[..arguments];
        var generic = bare.IndexOf("``", StringComparison.Ordinal);
        var dot = (generic < 0 ? bare : bare[..generic]).LastIndexOf('.');

        if (dot < 0)
        {
            return $"`{bare}`";
        }

        var owner = bare[..dot];
        var member = (generic < 0 ? bare : bare[..generic])[(dot + 1)..];
        var label = $"{owner.Split('.')[^1].Split('`')[0]}.{member}";

        if (!_pages.TryGetValue(owner, out var ownerPage))
        {
            return $"`{member}`";
        }

        return _anchors.TryGetValue(cref, out var anchor)
            ? $"[`{label}`](../{ownerPage}.md#{anchor})"
            : $"[`{label}`](../{ownerPage}.md)";
    }

    private static string Anchor(string name)
    {
        var slug = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return slug.Length == 0 ? "member" : slug;
    }

    /// <summary>A table cell holds neither a pipe nor a line break, and every cell here is generated.</summary>
    private static string Cell(string text) => text.Replace("|", "\\|").Replace("\n", " ").Trim();
}
