using System.ComponentModel.DataAnnotations;
using Tomlyn;

namespace SharpSiteGenerator;

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
    
    public string BasePath { get; set; }
    [ Required ]
    public string Url { get; set; }

    public string HomeFileName { get; set; } = "home.html";
    public string Title { get; set; }

    /// <summary>
    /// Initializes the <c>SiteSettings</c> object containing the global program settings, it will look for the
    /// settings.toml file in the current directory
    /// </summary>
    private SiteSettings()
    {
        BasePath = Directory.GetCurrentDirectory();
        string config_text = File.ReadAllText(Path.Join(BasePath, "settings.toml"));
        var config_file = Toml.ToModel(config_text);
        
        // At first, our settings will be just globals declared in the setting.toml file, in the future we may need a
        // more complex struct, that's why I'm using TOML.

        Url = config_file["url"].ToString();
        Title = config_file["title"].ToString();
        if (config_file.ContainsKey("homeFileName"))
        {
            HomeFileName = config_file["homeFileName"].ToString();
        }
    }
}