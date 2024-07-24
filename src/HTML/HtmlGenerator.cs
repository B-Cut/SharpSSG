using System.Dynamic;
using HandlebarsDotNet;
using Tomlyn;

namespace SharpSiteGenerator.Html;

public static class HtmlGenerator
{
    // A reference to SiteSettings, just so I don't have to get the Instance all the time
    private static SiteSettings _settings = SiteSettings.Instance;
    private static readonly List<string> ValidExtensions = [".html", "htm", ".hbs", ".handlebars"];
    
    
    private static bool _checkExtension(string filename)
    {
        return ValidExtensions.Any(filename.EndsWith);
    }
    
    /// <summary>
    /// Registers the contents of a folder and any of its subfolders as partials. Uses the filename as the partial name.
    /// </summary>
    /// <param name="dir">Templates directory</param>
    private static void _RegisterTemplates(string dir)
    {
        // Get all files that end in one of the valid extensions
        var files = Directory.GetFiles(dir)
            .Where(_checkExtension);

        foreach (var file in files)
        {
            var partialName = file.Split(".")[0];
            if (partialName.Contains(' '))
            {
                Console.Error.WriteLine($"The file {file} has a whitespace and will not be registered as a partial");
                continue;
            }
            Handlebars.RegisterTemplate(partialName, file);
        }

        foreach (var subdir in Directory.GetDirectories(dir))
        {
            _RegisterTemplates(subdir);
        }
    }

    private static Dictionary<string, object> _getThemeSettings(string path)
    {
        if (!File.Exists(path))
        {
            // No need to panic here since the theme may be simple enough to not have a settings file
            // Just return an empty dict
            Console.WriteLine("No theme_settings.toml file found in theme folder");
            return new Dictionary<string, object>();
        }

        var contents = File.ReadAllText(path);
        return Toml.ToModel(contents).ToDictionary();

    }

    /// <summary>
    /// Compiles every file on <c>layoutDir</c> and outputs them on <c>outputDir</c>. Calls recursively on sub-folders
    /// and keeps hierarchy.
    /// </summary>
    /// <param name="layoutDir"></param>
    /// <param name="outputDir"></param>
    /// <param name="themeSettings"></param>
    private static void _CompileLayouts(string layoutDir, string outputDir, IDictionary<string, object> themeSettings)
    {
        var files = Directory.GetFiles(layoutDir);
        foreach (var file in files)
        {
            var fileName = file.Split(".")[0].Split(['/', '\\']).Last() + ".html";
            var outputPath = Path.Join(outputDir, fileName);
            
            var content = Handlebars.Compile(File.ReadAllText(file))(new
            {
                site_settings = _settings,
                theme_settings = themeSettings
            });
            
            
            File.WriteAllText(outputPath, content);
        }

        foreach (var subdir in Directory.GetDirectories(layoutDir))
        {
            var newOutputDir = Path.Join(outputDir, subdir.Split(['/', '\\']).Last());
            if (!Directory.Exists(newOutputDir))
                Directory.CreateDirectory(newOutputDir);
            // Actual folder name
            _CompileLayouts(subdir, newOutputDir, themeSettings);
        }
    }
    
    /// <summary>
    /// Compiles the contents of the <c>templates</c> directory as valid HTML.
    /// </summary>
    public static void CompileHtml()
    {
        // We first ensure that the project has a theme folder, containing a layouts folder with an index file.
        // A templates folder inside themes is optional.
        var themePath = Path.Join(_settings.BasePath, _settings.ThemeDirectoryName);
        if (!Directory.Exists(themePath))
        {
            throw new DirectoryNotFoundException("Could not find the \"theme\" directory in current project");
        }

        var themeSettings = _getThemeSettings(Path.Join(themePath, _settings.ThemeSettingsFileName));
        
        var templateDir = Path.Join(themePath, _settings.TemplatesDirectoryName);
        if (Directory.Exists(templateDir))
        {
            _RegisterTemplates(templateDir);
        }

        var layoutsDir = Path.Join(themePath, "layouts");
        if (!Directory.Exists(layoutsDir))
        {
            throw new DirectoryNotFoundException("Could not find the \"layouts\" directory inside theme");
        }
        
        var indexPath = Path.Join(layoutsDir, _settings.IndexFileName);

        if (!File.Exists(indexPath))
        {
            throw new FileNotFoundException($"Could not find the index layout \"{_settings.IndexFileName}\" inside layouts");
        }
        
        //TODO: Compile to temp folder before going straight to build
        if (!Directory.Exists(_settings.OutputDir))
        {
            Directory.CreateDirectory(_settings.OutputDir);
        }
        
        _CompileLayouts(layoutsDir, _settings.OutputDir, themeSettings);
    }
}