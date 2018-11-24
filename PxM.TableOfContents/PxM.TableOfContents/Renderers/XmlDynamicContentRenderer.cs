using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PxM.TableOfContents.Extensions;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.PrintStudio.PublishingEngine;

namespace PxM.TableOfContents.Renderers
{
    /// <summary>
    /// Defines the XML dynamic content renderer class.
    /// </summary>
    public class XmlDynamicContentRenderer : XmlContentRenderer
    {
        /// <summary>
        /// Gets the data key.
        /// </summary>
        protected string DataKey
        {
            get
            {
                return this.RenderingItem[Constants.Fields.DataKey];
            }
        }

        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <returns>
        /// The field name
        /// </returns>
        protected override string GetFieldName()
        {
            return this.RenderingItem[Constants.Fields.ItemField];
        }

        /// <summary>
        /// Preliminary render action invoked before RenderContent <see cref="InDesignItemRendererBase.RenderContent"/>.
        /// </summary>
        /// <param name="printContext">The print context.</param>
        protected override void BeginRender(PrintContext printContext)
        {
            string dataSource = string.Empty;
            var dataItem = this.GetDataItem(printContext);
            if (dataItem != null)
            {
                dataSource = dataItem.ID.ToString();
            }

            if (!string.IsNullOrEmpty(this.RenderingItem[Constants.Fields.ItemReference]) && dataSource == null)
            {
                dataSource = this.RenderingItem[Constants.Fields.ItemReference];
            }

            try
            {
                if (!string.IsNullOrEmpty(this.DataKey) && printContext.Settings.Parameters.ContainsKey(this.DataKey))
                {
                    string dataItemId = printContext.Settings.Parameters[this.DataKey].ToString();
                    if (!string.IsNullOrEmpty(dataItemId))
                    {
                        dataSource = dataItemId;
                    }
                }

                if (!string.IsNullOrEmpty(dataSource))
                {
                    this.ContentItem = printContext.Database.GetItem(dataSource);

                    var xpath = this.RenderingItem[Constants.Fields.ItemSelector];
                    if (!string.IsNullOrEmpty(xpath))
                    {
                        Item selectorDataItem = this.ContentItem.Axes.SelectSingleItem(RenderingHelper.EscapeInvalidCharacters(xpath));
                        if (selectorDataItem != null)
                        {
                            this.ContentItem = selectorDataItem;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message, this);
            }
        }

    }
}
