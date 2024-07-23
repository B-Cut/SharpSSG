using System.ComponentModel.DataAnnotations;
using Tomlyn;

namespace SharpSiteGenerator;

// These settings are supposed to be immutable by default, maybe transition to a record

/// <summary>
/// This class contains the global program configurations
/// </summary>
public class SiteSettings
{
    private static SiteSettings instance;

    public static SiteSettings Instance
    {
        get
        {
            if (instance is null)
            {
                instance = new SiteSettings();
            }
            return instance;
        }
        set => instance = value;
    }
    
    
    // This might change once we incorporate cli arguments
    public string BasePath { get; set; } = Directory.GetCurrentDirectory();
    public readonly string ThemeDirectoryName = "theme";
    public readonly string LayoutsDirectoryName = "layouts";
    public readonly string TemplatesDirectoryName = "templates";
    public string OutputDir { get; set; }
    
    [ Required ]
    public string Url { get; set; }

    public string IndexFileName { get; } = "index.html";
    public string Title { get; }

    /// <summary>
    /// Initializes the <c>SiteSettings</c> object containing the global program settings, it will look for the
    /// settings.toml file in the current directory
    /// </summary>
    private SiteSettings()
    {
        var configText = File.ReadAllText(Path.Join(BasePath, "settings.toml"));
        var configFile = Toml.ToModel(configText);
        
        // At first, our settings will be just globals declared in the setting.toml file, in the future we may need a
        // more complex struct, that's why I'm using TOML.

        Url = configFile["url"].ToString() ?? throw new InvalidOperationException("Could not find site url in settings.toml");
        Title = configFile["title"].ToString() ?? throw new InvalidOperationException("Could not find site title in settings.toml");
        if (configFile.TryGetValue("indexFileName", out var value2))
        {
            IndexFileName = value2.ToString();
        }

        OutputDir = configFile.TryGetValue("outputDir", out var value1) ? 
            Path.GetFullPath(value1.ToString()) : 
            Path.Join(BasePath, "build");
        
    }
}