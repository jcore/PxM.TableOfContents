using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Security;
using Sitecore.SecurityModel;

namespace PxM.TableOfContents.Extensions
{
    /// <summary>
    /// Static property and methods used for printing
    /// </summary>
    internal static class RenderingHelper
    {
        /// <summary>
        /// Replaces the variables.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="variables">The variables.</param>
        internal static void ReplaceVariables(ref string input, IDictionary<string, string> variables)
        {
            input = variables.Aggregate(input, (current, variable) => current.Replace(variable.Key, variable.Value));
        }

        /// <summary>
        /// Inners the XML.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public static string InnerText(this XElement elem)
        {
            return elem.Elements().Aggregate(string.Empty, (element, node) => element += node.Value.ToString());
        }


        public static string EscapeInvalidCharacters(string xpath)
        {
            var newPath = new List<string>();
            if (string.IsNullOrWhiteSpace(xpath) || (!xpath.Contains(" ") && !xpath.Contains("-")))
            {
                return xpath;
            }
            var pathSegments = xpath.Split('/');
            foreach (var segment in pathSegments)
            {
                if (segment.Contains(" ") || segment.Contains("-"))
                {
                    newPath.Add(string.Format("#{0}#", segment));
                }
                else
                {
                    newPath.Add(segment);
                }
            }
            return string.Join("/", newPath);
        }
    }
}
