namespace MuseDashModTools.Tasks;

[PublicAPI]
public sealed class GenerateModToolsStyles : Task
{
    private const string AvaloniaNamespaceUrl = "https://github.com/avaloniaui";
    private const string SemiNamespaceUrl = "https://irihi.tech/semi";
    private const string XamlNamespaceUrl = "http://schemas.microsoft.com/winfx/2006/xaml";

    private const string OutputFileName = "ModToolsStyles.axaml";

    private readonly Dictionary<string, CategoryInfo> _categories = new()
    {
        ["ExtendedControls"] = new CategoryInfo("Extended Controls"),
        ["TemplatedControls"] = new CategoryInfo("Templated Controls"),
        ["Styles"] = new CategoryInfo("Control Styles")
    };

    private readonly XNamespace AvaloniaNamespace = AvaloniaNamespaceUrl;
    private readonly XNamespace SemiNamespace = SemiNamespaceUrl;
    private readonly XNamespace XamlNamespace = XamlNamespaceUrl;

    [Required]
    public ITaskItem[] AvaloniaResourceFiles { get; set; } = null!;

    public override bool Execute()
    {
        try
        {
            var styles = BuildBaseStyles();
            ProcessResourceFiles();
            AppendCategoryStyles(styles);
            SaveStylesToFile(styles);
            Log.LogMessage(MessageImportance.High, $"{OutputFileName} generated successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }

    private XElement BuildBaseStyles()
    {
        var styles = new XElement(AvaloniaNamespace + "Styles",
            new XAttribute("xmlns", AvaloniaNamespaceUrl),
            new XAttribute(XNamespace.Xmlns + "x", XamlNamespaceUrl),
            new XAttribute(XNamespace.Xmlns + "semi", SemiNamespaceUrl));

        styles.Add(new XElement(SemiNamespace + "SemiTheme"));
        styles.Add(BuildResourcesSection());

        return styles;
    }

    private XElement BuildResourcesSection() =>
        new(AvaloniaNamespace + "Styles.Resources",
            new XElement(AvaloniaNamespace + "ResourceDictionary",
                new XElement(AvaloniaNamespace + "ResourceDictionary.ThemeDictionaries",
                    CreateThemeInclude("Light"),
                    CreateThemeInclude("Dark")),
                new XElement(AvaloniaNamespace + "ResourceDictionary.MergedDictionaries",
                    CreateResourceInclude("Shared"))));

    private XElement CreateThemeInclude(string themeName) =>
        new(AvaloniaNamespace + "ResourceInclude",
            new XAttribute(XamlNamespace + "Key", themeName),
            new XAttribute("Source", $"/Resources/{themeName}Resource.axaml"));

    private XElement CreateResourceInclude(string resourceName) =>
        new(AvaloniaNamespace + "ResourceInclude",
            new XAttribute("Source", $"/Resources/{resourceName}Resource.axaml"));

    private void ProcessResourceFiles()
    {
        foreach (var file in AvaloniaResourceFiles)
        {
            var directory = Path.GetDirectoryName(file.ItemSpec);
            var fileName = file.GetMetadata("Filename");
            var relativePath = $"/{directory}/{fileName}.axaml";

            if (_categories.TryGetValue(directory, out var category))
            {
                category.Sources.Add(relativePath);
            }
        }
    }

    private void AppendCategoryStyles(XElement styles)
    {
        foreach (var (_, category) in _categories)
        {
            if (category.Sources is [])
            {
                continue;
            }

            styles.Add(new XComment($"  {category.Comment}  "));
            category.Sources.ForEach(source =>
                styles.Add(CreateStyleInclude(source)));
        }
    }

    private XElement CreateStyleInclude(string source) =>
        new(AvaloniaNamespace + "StyleInclude",
            new XAttribute("Source", source));

    private void SaveStylesToFile(XElement styles)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            NewLineChars = Environment.NewLine,
            NewLineHandling = NewLineHandling.Replace,
            NewLineOnAttributes = true,
            OmitXmlDeclaration = true
        };

        using var writer = XmlWriter.Create(OutputFileName, settings);
        styles.Save(writer);
    }

    private sealed class CategoryInfo(string comment)
    {
        public string Comment { get; } = comment;
        public List<string> Sources { get; } = [];
    }
}