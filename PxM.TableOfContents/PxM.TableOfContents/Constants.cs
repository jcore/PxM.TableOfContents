using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxM.TableOfContents
{
    public struct Constants
    {
        public struct Fields
        {
            public const string ItemField = "Item Field";
            public const string ItemReference = "Item Reference";
            public const string ItemSelector = "Item Selector";
            public const string DataKey = "Data Key";
            public const string Style = "Style";
            public const string StaticTextFieldName = "Content";
            public const string DefaultParagraphStyle = "NormalParagraphStyle";
            public const string RelativePath = "Relative Path";
            public const string ReferenceItemTemplateId = "Reference Item Template ID";
            public const string TransformationSet = "Transformation Set";
            public const string PublishWhenEmpty = "Publish when empty";            
        }

        public const string DefaultDevice = "{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}";
        public const string ParagraphStyle = "ParagraphStyle";

        public struct XmlAttributes
        {
            public const string Style = "Style";
        }
    }
}
