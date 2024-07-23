namespace SharpSiteGenerator.Html;

public static class BuiltinTemplate
{
    public static readonly string Title = "<title> {{ title }} </title>";
    public static readonly string StyleSheet = "<link rel=\"stylesheet\" href=\"{{ style_path }}\">";
    public static readonly string Script = "<script src=\"{{ script_path }}\"></script>";
}