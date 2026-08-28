using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Re4QuadExtremeEditor.src.Class.Enums;

namespace Re4QuadExtremeEditor.src.Class.MyProperty.CustomAttribute
{
    public class CustomDescriptionAttribute : DescriptionAttribute
    {
        private readonly aLang AttributeTextId;

        public CustomDescriptionAttribute(aLang AttributeTextId)
        {
            this.AttributeTextId = AttributeTextId;
        }

        public override string Description
        {
            get { return Lang.GetAttributeText(AttributeTextId); }
        }
    }

}
