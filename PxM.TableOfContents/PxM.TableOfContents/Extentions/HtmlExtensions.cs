using HtmlAgilityPack;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PxM.TableOfContents.Extensions
{
    public static class HtmlExtensions
    {
        public static string StripHtmlTags(this string source)
        {
            return Regex.Replace(source, @"<.*?>|&.*?;", string.Empty);
        }

        public static string RemoveImages(this string source)
        {
            return Regex.Replace(source, @"<img>(.*?)<\/img>", string.Empty);
        }

        public static string RemoveBadHtml(this string htmlContent)
        {
            try
            {
                var brPatterh = @"([\b\s]*<[\b\s]*[bB][rR][\s]*/?[\b\s]*>){2,}";

                htmlContent = htmlContent.Replace("\n", string.Empty).RemoveStyle("font-size").RemoveStyle("font-family");
                htmlContent = Regex.Replace(htmlContent, brPatterh, "<br>", RegexOptions.Multiline);
                htmlContent = htmlContent.Replace(":<br>", ":").Replace("<br><p>", "<p>");                

                return htmlContent;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex, typeof(HtmlExtensions));
            }
            return htmlContent;
        }
        
        public static string RemoveStyle(this string html, string style)
        {
            Regex regex = new Regex(style + "\\s*:.*?;?");

            return regex.Replace(html, string.Empty);
        }

        public static void RemoveEmptyParagraphStyles(XElement rootElement)
        {
            for (var i = rootElement.Elements().Count() - 1; i >= 0; i--)
            {
                var element = rootElement.Elements().ElementAt(i);
                if (element.Name.LocalName == Constants.ParagraphStyle && !element.HasElements && string.IsNullOrWhiteSpace(element.Value))
                {
                    element.Remove();
                }
            }
        }
    }
}
