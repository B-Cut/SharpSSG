// This is a simple static site generator powered by .NET and C#

using SharpSiteGenerator;
using SharpSiteGenerator.Html;

var settings = new SiteSettings();
IHtmlGenerator htmlGenerator = new HandlebarsHtmlGenerator(settings);

try
{
    htmlGenerator.GenerateHtml();
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    // For debug only
    Console.Error.WriteLine(ex);
}
