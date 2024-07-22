// This is a simple static site generator powered by .NET and C#

using SharpSiteGenerator;

var settings = SiteSettings.Instance;

Console.WriteLine($"URL: {settings.Url}\nHome file: {settings.HomeFileName}\nTitle: {settings.Title}");
