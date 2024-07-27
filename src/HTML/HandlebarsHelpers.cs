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
            if (context["page_settings"] is null ||
                !(context["page_settings"] as TomlTable)!.ContainsKey("header_template"))
            {
                var title = (context["site_settings"] as SiteSettings)!.Title;
                
                var def = $"<head>\n\t<title>{title}</title>\n</head>";
                output.WriteSafeString(def);
            }
            else{
                var baseString =  "{{{> " + (context["page_settings"] as TomlTable)!["header_template"] + " this}}}";
                output.WriteSafeString(baseString);
            }
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
        // TODO: Refactor to add ability to load a shell from a file
        handlebarsContext.RegisterHelper("shell", (output, options, context, arguments) =>
        {

            var head = "<!DOCTYPE html5>\n<html>\n\t{{load_header}}\n";
            var body = $"\t<body>\n\t\t{options.Template()}\n\t</body>\n";
            var footer = "</html>";

            var content = handlebarsContext.Compile(head + body + footer)(context.Value);
            
            output.WriteSafeString(content);
        });
        
    }
}