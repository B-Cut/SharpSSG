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
        
        handlebarsContext.RegisterHelper("load_header", (context, arguments) =>
        {
            var baseString =  "{{{> " + (context["page_settings"] as TomlTable)!["header_template"] + "}}}";
            return handlebarsContext.Compile(baseString)(context);
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