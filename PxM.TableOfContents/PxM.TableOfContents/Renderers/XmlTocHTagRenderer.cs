using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Sitecore.PrintStudio.PublishingEngine.Text;
using System.Xml;
using HtmlAgilityPack;
using PxM.TableOfContents.Extensions;

namespace PxM.TableOfContents.Renderers
{
    public class XmlTocHTagRenderer : XmlDynamicContentRenderer
    {
        public string HTags { get; set; }
        public string HParagraphStyles { get; set; }
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

        protected override void BeginRender(PrintContext printContext)
        {
            if (!string.IsNullOrEmpty(this.RenderingItem[Constants.Fields.ItemReference]))
            {
                this.DataSource = this.RenderingItem[Constants.Fields.ItemReference];
            }
        }

        protected override void RenderContent(PrintContext printContext, XElement output)
        {
            string content = this.ParseContent(printContext);
            if (content == null)
            {
                return;
            }
            var hTags = !string.IsNullOrWhiteSpace(HTags) ? HttpUtility.UrlDecode(HTags).Split(',') : new string[0];
            var hParagraphStyles = !string.IsNullOrWhiteSpace(HParagraphStyles) ? HttpUtility.UrlDecode(HParagraphStyles).Split('|') : new string[0];

            try
            {
                var dataItem = !string.IsNullOrWhiteSpace(this.RenderingItem[Constants.Fields.DataKey]) && printContext.Settings.Parameters.ContainsKey(this.RenderingItem[Constants.Fields.DataKey]) ? printContext.Database.GetItem((string)printContext.Settings.Parameters[this.RenderingItem[Constants.Fields.DataKey]]) : GetDataItem(printContext);
                if (dataItem != null)
                {
                    var htmlContent = dataItem[this.RenderingItem[Constants.Fields.ItemField]];

                    htmlContent = htmlContent.RemoveBadHtml();

                    ParseContext context = new ParseContext(printContext.Database, printContext.Settings)
                    {
                        DefaultParagraphStyle = this.Style,
                        ParseDefinitions = GetParseDefinitionCollection(this.RenderingItem)
                    };

                    var xmlcontent = RichTextParser.ConvertToXml(htmlContent, context, printContext.Language);
                    var elements = XElement.Parse(string.Format("<TempContentRoot>{0}</TempContentRoot>", xmlcontent.Contains("CDATA") ? xmlcontent : xmlcontent.Replace("&", "&amp;")));

                    foreach (var element in elements.Elements())
                    {
                        if (element.Name.LocalName == Constants.ParagraphStyle && element.Attribute(Constants.XmlAttributes.Style) != null && hTags.Contains(element.Attribute(Constants.XmlAttributes.Style).Value))
                        {
                            int tocIndex = 0;
                            if (printContext.Settings.Parameters != null && printContext.Settings.Parameters.ContainsKey(this.IndexName))
                            {
                                tocIndex = (int)printContext.Settings.Parameters[this.IndexName];
                            }

                            printContext.Settings.Parameters[this.IndexName] = tocIndex + 1;

                            var indexContent = this.RenderingItem[Constants.Fields.StaticTextFieldName];
                            indexContent = indexContent.Replace(this.ReplaceVar, tocIndex.ToString());

                            var hTag = hTags.FirstOrDefault(i => i == element.Attribute(Constants.XmlAttributes.Style).Value);
                            var styleIndex = hTags.ToList().FindIndex(i => i == hTag);

                            if (element.HasElements)
                            {
                                var cData = element.DescendantNodes().Where(n => n.NodeType == XmlNodeType.CDATA);
                                element.ReplaceNodes(cData);
                            }

                            var paragraphString = string.Format(@"<ParagraphStyle Style=""{0}"">{1}</ParagraphStyle>", hParagraphStyles.Count() > styleIndex ? hParagraphStyles.ElementAt(styleIndex) : string.Empty, indexContent);

                            var paragraph = XElement.Parse(paragraphString);
                            if (!element.IsEmpty)
                            {
                                paragraph.AddFirst(element.Nodes());
                            }
                            output.Add(paragraph);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Log.Error("Render content parse content value", exc, this);
                return;
            }            
        }
    }
}
