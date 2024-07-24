using HandlebarsDotNet;
using Tomlyn;
using Tomlyn.Model;

namespace SharpSiteGenerator.Html;

public static class HtmlGenerator
{
    // A reference to SiteSettings, just so I don't have to get the Instance all the time
    private static readonly SiteSettings Settings = SiteSettings.Instance;
    private static readonly List<string> ValidExtensions = [".html", "htm", ".hbs", ".handlebars"];
    
    
    private static bool _checkExtension(string filename)
    {
        return ValidExtensions.Any(filename.EndsWith);
    }

    /// <summary>
    /// Registers the contents of a folder and any of its subfolders as partials. Uses the filename as the partial name.
    /// </summary>
    /// <param name="context">Handlebars context </param>
    /// <param name="dir">Templates directory</param>
    private static void _RegisterTemplates(IHandlebars context, string dir)
    {
        // Get all files that end in one of the valid extensions
        var files = Directory.GetFiles(dir)
            .Where(_checkExtension);

        foreach (var file in files)
        {
            var partialName = file.Split(".")[0].Split(['/', '\\']).Last();
            if (partialName.Contains(' '))
            {
                Console.Error.WriteLine($"The file {file} has a whitespace and will not be registered as a partial");
                continue;
            }
            context.RegisterTemplate(partialName, File.ReadAllText(file));
        }

        foreach (var subdir in Directory.GetDirectories(dir))
        {
            _RegisterTemplates(context, subdir);
        }
    }

    private static TomlTable _getThemeSettings(string path)
    {
        if (!File.Exists(path))
        {
            // No need to panic here since the theme may be simple enough to not have a settings file
            // Just return an empty dict
            Console.WriteLine("No theme_settings.toml file found in theme folder");
            return new TomlTable();
        }

        var contents = File.ReadAllText(path);
        return Toml.ToModel(contents);

    }

    /// <summary>
    /// Compiles every file on <c>layoutDir</c> and outputs them on <c>outputDir</c>. Calls recursively on sub-folders
    /// and keeps hierarchy.
    /// </summary>
    /// <param name="context">The handlebars context</param>
    /// <param name="layoutDir">path to layouts directory</param>
    /// <param name="outputDir">path to output</param>
    /// <param name="themeSettings">Dictionary containing the site settings</param>
    private static void _CompileLayouts(IHandlebars context, string layoutDir, string outputDir, TomlTable themeSettings)
    {
        var files = Directory.GetFiles(layoutDir);
        foreach (var file in files)
        {
            var fileName = file.Split(".")[0].Split(['/', '\\']).Last();
            var outputPath = Path.Join(outputDir, fileName + ".html");

            var func = context.Compile(File.ReadAllText(file));
            var pageSettings = new TomlTable();
            if ((themeSettings["page"] as TomlTable)!.ContainsKey(fileName))
            {
                pageSettings = (themeSettings["page"] as TomlTable)![fileName] as TomlTable;
            }
            
            var content = func(new
            {
                site_settings = Settings,
                theme_settings = themeSettings["general"],
                page_settings = pageSettings
            });
            
            
            File.WriteAllText(outputPath, content);
        }

        foreach (var subdir in Directory.GetDirectories(layoutDir))
        {
            var newOutputDir = Path.Join(outputDir, subdir.Split(['/', '\\']).Last());
            if (!Directory.Exists(newOutputDir))
                Directory.CreateDirectory(newOutputDir);
            // Actual folder name
            _CompileLayouts(context, subdir, newOutputDir, themeSettings);
        }
    }
    
    /// <summary>
    /// Compiles the contents of the <c>templates</c> directory as valid HTML.
    /// </summary>
    public static void CompileHtml()
    {
        // We first ensure that the project has a theme folder, containing a layouts folder with an index file.
        // A templates folder inside themes is optional.
        var themePath = Path.Join(Settings.BasePath, Settings.ThemeDirectoryName);
        if (!Directory.Exists(themePath))
        {
            throw new DirectoryNotFoundException("Could not find the \"theme\" directory in current project");
        }

        var themeSettings = _getThemeSettings(Path.Join(themePath, Settings.ThemeSettingsFileName));
        
        
        var handlebarsContext = Handlebars.Create();
        
        var templateDir = Path.Join(themePath, Settings.TemplatesDirectoryName);
        if (Directory.Exists(templateDir))
        {
            _RegisterTemplates(handlebarsContext, templateDir);
        }

        var layoutsDir = Path.Join(themePath, "layouts");
        if (!Directory.Exists(layoutsDir))
        {
            throw new DirectoryNotFoundException("Could not find the \"layouts\" directory inside theme");
        }
        
        var indexPath = Path.Join(layoutsDir, Settings.IndexFileName);

        if (!File.Exists(indexPath))
        {
            throw new FileNotFoundException($"Could not find the index layout \"{Settings.IndexFileName}\" inside layouts");
        }
        
        //TODO: Compile to temp folder before going straight to build
        if (!Directory.Exists(Settings.OutputDir))
        {
            Directory.CreateDirectory(Settings.OutputDir);
        }
        
        
        
        // We'll add the helper functions provided by Handlebars.Net.Helpers and our own
        
        HandlebarsHelpers.Register(handlebarsContext);
        
        _CompileLayouts(handlebarsContext, layoutsDir, Settings.OutputDir, themeSettings);
    }
}