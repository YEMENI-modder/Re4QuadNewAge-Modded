using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Re4QuadExtremeEditor.src.Class.Enums;

namespace Re4QuadExtremeEditor.src.Class.MyProperty.CustomAttribute
{
    public class CustomDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly aLang AttributeTextId;

        public CustomDisplayNameAttribute(aLang AttributeTextId)
        {
            this.AttributeTextId = AttributeTextId;
        }

        public override string DisplayName
        {
            get { return Lang.GetAttributeText(AttributeTextId); }
        }
    }

}
