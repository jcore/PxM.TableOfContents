using HtmlAgilityPack;
using PxM.TableOfContents.Extensions;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using Sitecore.PrintStudio.PublishingEngine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace PxM.TableOfContents.Renderers
{
    public class XmlDynamicContentWithTocRenderer : XmlDynamicContentRenderer
    {
        public string HTags { get; set; }
        /// <summary>
        /// Gets or sets the name of the index.
        /// </summary>
        /// <value>
        /// The name of the index.
        /// </value>
        public string IndexName { get; set; }

        /// <summary>
        /// Gets or sets the replace var.
        /// </summary>
        /// <value>
        /// The replace var.
        /// </value>
        public string ReplaceVar { get; set; }

        protected override void BeginRender(Sitecore.PrintStudio.PublishingEngine.PrintContext printContext)
        {
            if (!string.IsNullOrEmpty(this.RenderingItem[Constants.Fields.ItemReference]))
            {
                this.DataSource = this.RenderingItem[Constants.Fields.ItemReference];
            }
        }

        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            string content = this.ParseContent(printContext);
            XElement tempContentNode;
            if (content == null)
            {
                return;
            }
            var hTags = !string.IsNullOrWhiteSpace(HTags) ? HttpUtility.UrlDecode(HTags).Split(',') : new string[0];

            try
            {
                var dataItem = !string.IsNullOrWhiteSpace(this.RenderingItem[Constants.Fields.DataKey]) && printContext.Settings.Parameters.ContainsKey(this.RenderingItem[Constants.Fields.DataKey]) ? printContext.Database.GetItem((string)printContext.Settings.Parameters[this.RenderingItem[Constants.Fields.DataKey]]) : GetDataItem(printContext);
                if (dataItem != null)
                {
                    try
                    {
                        var htmlContent = dataItem[this.RenderingItem[Constants.Fields.ItemField]];

                        ParseContext context = new ParseContext(printContext.Database, printContext.Settings)
                        {
                            DefaultParagraphStyle = this.Style,
                            ParseDefinitions = GetParseDefinitionCollection(this.RenderingItem)
                        };

                        content = AddWrapping(RichTextParser.ConvertToXml(htmlContent.RemoveBadHtml(), context, printContext.Language));

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex, this);
                    }
                }
                //to prevent exception related to "&" characters in values, encode it
                tempContentNode = XElement.Parse(string.Format("<TempContentRoot>{0}</TempContentRoot>", content.Contains("CDATA") ? content : content.Replace("&", "&amp;")));
            }
            catch (Exception exc)
            {
                Log.Error("Render content parse content value", exc, this);
                return;
            }

            ProcessToc(printContext, hTags, ref tempContentNode);

            if (!tempContentNode.HasElements)
            {
                var plainTextNodes = tempContentNode.Elements().Where(t => t.NodeType == XmlNodeType.Text);
                foreach (var plainTextNode in plainTextNodes)
                {
                    //now we need to decode "&" character to provide correct value for InDesign. APS char replacement is not working with this. doing replace :(
                    plainTextNode.ReplaceWith(new XCData(content.Contains("CDATA") ? plainTextNode.ToString() : plainTextNode.ToString().Replace("&amp;", "&").Replace("&amp;", "&")));
                }                

                output.Add(tempContentNode.Nodes());
                this.RenderChildren(printContext, output);
            }
            else
            {
                XElement currentNode = output;
                if (tempContentNode.Elements().Count() == 1)
                {                    
                    currentNode = tempContentNode.Elements().First();
                }

                // first render children, then add the nodes, prevents from losing the children xml
                this.RenderChildren(printContext, currentNode);
                output.Add(tempContentNode.Nodes());
            }
        }

        private void ProcessToc(PrintContext printContext, string[] hTags, ref XElement rootElement)
        {
            foreach (var element in rootElement.Elements())
            {
                if (element.Name.LocalName == Constants.ParagraphStyle && element.Attribute(Constants.XmlAttributes.Style) != null && hTags.Contains(element.Attribute(Constants.XmlAttributes.Style).Value))
                {
                    int tocIndex = 0;
                    if (printContext.Settings.Parameters.ContainsKey(this.IndexName))
                    {
                        tocIndex = (int)printContext.Settings.Parameters[this.IndexName];
                    }

                    printContext.Settings.Parameters[this.IndexName] = tocIndex + 1;

                    var indexContent = this.RenderingItem[Constants.Fields.StaticTextFieldName];
                    indexContent = indexContent.Replace(this.ReplaceVar, tocIndex.ToString());

                    var newNode = XElement.Parse(indexContent);
                    element.Add(newNode);
                }
            }

            HtmlExtensions.RemoveEmptyParagraphStyles(rootElement);
        }

    }
}
