using HandlebarsDotNet;
using Tomlyn.Model;

namespace SharpSiteGenerator.Html;

public static class HandlebarsHelpers
{
    /// <summary>
    /// Registers helpers to a handlebars handlebarsContext. Includes the ones defined
    /// in <c>HandlebarsDotNet.Helpers.HandlebarsHelpers</c>
    /// </summary>
    /// <param name="handlebarsContext"></param>
    public static void Register(IHandlebars handlebarsContext)
    {
        // Registering default helpers
        HandlebarsDotNet.Helpers.HandlebarsHelpers.Register(handlebarsContext);
        
        // Compile the header template
        handlebarsContext.RegisterHelper("load_header", (output, context, arguments) =>
        {
            if ((context["page_settings"] as TomlTable)!.ContainsKey("header_template"))
            {
                var baseString =  "{{{> " + (context["page_settings"] as TomlTable)!["header_template"] + " this}}}";
                output.WriteSafeString(baseString);
                //return handlebarsContext.Compile(baseString)(context);
            }

            var def = $"<head>\n\t<title>{(context["site_settings"] as SiteSettings)!.Title}</title>\n</head>";
            output.WriteSafeString(def);
            //return def;
        });
        
        // Get the proper link to a page in site
        handlebarsContext.RegisterHelper("reference_page", (context, arguments) =>
        {
            if (arguments.Length != 1)
            {
                throw new HandlebarsException(
                    $"The \"reference_page\" helper takes only one argument, but {arguments.Length} were provided");
            }
            return (context["site_settings"] as SiteSettings)!.Url + "/" + arguments[0] + ".html";
        });
        
        // Shell block helper
        handlebarsContext.RegisterHelper("shell", (options, context, arguments) =>
        {
            
            var head = "<!DOCTYPE html5>\n<html>\n\t{{{>" + $"{context["page_settings.header_template"]}" + "}}}\n";
            var body = $"\t<body>\n\t\t{options.Template()}\n\t</body>\n";
            var footer = "</html>";
            return  head + body + footer;
        });
        
    }
}