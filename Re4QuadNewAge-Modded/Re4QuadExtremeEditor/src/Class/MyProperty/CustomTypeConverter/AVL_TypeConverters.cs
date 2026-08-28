using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.Files;
using DataBase = Re4QuadExtremeEditor.src.DataBase;
using ListBoxProperty = Re4QuadExtremeEditor.src.ListBoxProperty;
using Globals = Re4QuadExtremeEditor.src.Globals;

namespace Re4QuadExtremeEditor.src.Class.MyProperty.CustomTypeConverter
{
    /// <summary>
    /// <para>Conversor que exibe o valor em HEX e DEC ao mesmo tempo: "0x2F (47)";</para>
    /// <para>entrada com prefixo "0x"/"#": hexadecimal; apenas digitos: decimal;</para>
    /// </summary>
    public class AVL_HexDecTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null && IsNumeric(value.GetType()))
            {
                ulong v = ToUlong(value);
                return FormatValue(v, value.GetType(), null);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string && context != null && context.PropertyDescriptor != null)
            {
                return ValueFromText((string)value, context.PropertyDescriptor.PropertyType);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// <para>formata o valor no modo global (HEX ou DEC), com nome opcional;</para>
        /// </summary>
        protected static string FormatValue(ulong v, Type type, string name)
        {
            string r;
            if (Globals.AvlRenderDecimal)
            {
                r = v.ToString();
            }
            else
            {
                string hex;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte: hex = v.ToString("X2"); break;
                    case TypeCode.UInt16:
                    case TypeCode.Int16: hex = v.ToString("X4"); break;
                    default: hex = v.ToString("X8"); break;
                }
                r = "0x" + hex;
            }
            if (name != null)
            {
                r += " - " + name;
            }
            return r;
        }

        protected static bool IsNumeric(Type t)        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.UInt32:
                case TypeCode.Int32:
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        protected static ulong ToUlong(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte: return (byte)value;
                case TypeCode.SByte: return (ulong)(sbyte)value;
                case TypeCode.UInt16: return (ushort)value;
                case TypeCode.Int16: return (ulong)(short)value;
                case TypeCode.UInt32: return (uint)value;
                case TypeCode.Int32: return (ulong)(int)value;
                case TypeCode.UInt64: return (ulong)value;
                case TypeCode.Int64: return (ulong)(long)value;
                default: return 0;
            }
        }

        /// <summary>
        /// <para>interpreta a entrada seguindo o modo de exibicao atual (WYSIWYG):</para>
        /// <para>prefixo "0x"/"#" = sempre hexadecimal;</para>
        /// <para>digitos soltos = hex no modo HEX, decimal no modo DEC;</para>
        /// retorna null se nao conseguir interpretar;
        /// </summary>
        protected static ulong? ParseInput(string text)
        {
            if (text == null)
            {
                return 0;
            }
            text = text.Trim().ToUpperInvariant();
            if (text.Length == 0)
            {
                return 0;
            }

            bool hex;
            int i;
            if (text.StartsWith("0X"))
            {
                hex = true;
                i = 2;
            }
            else if (text.StartsWith("#"))
            {
                hex = true;
                i = 1;
            }
            else
            {
                // sem prefixo: segue o modo global de exibicao
                hex = !Globals.AvlRenderDecimal;
                i = 0;
            }

            string digits = "";
            while (i < text.Length && (hex ? Uri.IsHexDigit(text[i]) : char.IsDigit(text[i])))
            {
                digits += text[i];
                i++;
            }

            if (digits.Length == 0)
            {
                return null;
            }

            try
            {
                return Convert.ToUInt64(digits, hex ? 16 : 10);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// converte o texto digitado para o valor do tipo da propriedade;
        /// </summary>
        protected virtual object ValueFromText(string text, Type propType)
        {
            ulong? parsed = ParseInput(text);
            ulong v = parsed ?? 0;

            switch (Type.GetTypeCode(propType))
            {
                case TypeCode.Byte: return (byte)v;
                case TypeCode.SByte: return unchecked((sbyte)v);
                case TypeCode.UInt16: return (ushort)v;
                case TypeCode.Int16: return unchecked((short)v);
                case TypeCode.UInt32: return (uint)v;
                case TypeCode.Int32: return unchecked((int)v);
                default: return v;
            }
        }
    }

    /// <summary>
    /// <para>conversor do Key ID com lista suspensa de todos os itens/chaves do jogo;</para>
    /// exibe "0x0081 (129) - Iron Key"; permite digitar manualmente;
    /// </summary>
    public class AVL_KeyIdTypeConverter : AVL_HexDecTypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            // false: permite digitar valores fora da lista
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List<string>();
            foreach (var pair in AVL_ItemNames.List.OrderBy(p => p.Key))
            {
                if (pair.Value.Length > 0)
                {
                    list.Add(FormatValue(pair.Key, typeof(ushort), pair.Value));
                }
            }
            return new StandardValuesCollection(list);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null && IsNumeric(value.GetType()))
            {
                ulong v = ToUlong(value);
                string name = AVL_ItemNames.GetName((ushort)v);
                if (name == null && v <= byte.MaxValue)
                {
                    name = AVL_ItemNames.GetName((byte)v);
                }
                return FormatValue(v, value.GetType(), name);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string && context != null && context.PropertyDescriptor != null)
            {
                string text = ((string)value).Trim();
                // colagem do nome do item
                foreach (var pair in AVL_ItemNames.List)
                {
                    if (pair.Value.Length > 0 && string.Equals(pair.Value, text, StringComparison.OrdinalIgnoreCase))
                    {
                        return ValueFromText("0x" + pair.Key.ToString("X"), context.PropertyDescriptor.PropertyType);
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <summary>
    /// <para>conversor do "Number of Aev" com lista suspensa dos eventos AEV carregados;</para>
    /// <para>se o arquivo AEV estiver aberto lista os eventos existentes; senao aceita digitar;</para>
    /// </summary>
    public class AVL_AevNumberTypeConverter : AVL_HexDecTypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            // false: sempre permite digitar um numero manualmente
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List<string>();
            try
            {
                if (DataBase.FileAEV != null && DataBase.FileAEV.Lines != null && DataBase.NodeAEV != null)
                {
                    foreach (ushort lineID in DataBase.FileAEV.Lines.Keys.OrderBy(x => x))
                    {
                        if (lineID > byte.MaxValue)
                        {
                            continue;
                        }
                        string typeName = GetAevTypeName(lineID);
                        list.Add(FormatValue(lineID, typeof(byte), "AEV " + typeName));
                    }
                }
            }
            catch (Exception)
            {
                // arquivo sendo carregado/descarregado: lista vazia
            }
            return new StandardValuesCollection(list);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null && IsNumeric(value.GetType()))
            {
                ulong v = ToUlong(value);
                string name = null;
                try
                {
                    if (DataBase.FileAEV != null && DataBase.FileAEV.Lines != null
                        && v <= byte.MaxValue && DataBase.FileAEV.Lines.ContainsKey((byte)v))
                    {
                        name = "AEV " + GetAevTypeName((byte)v);
                    }
                }
                catch (Exception)
                {
                    name = null;
                }
                return FormatValue(v, value.GetType(), name);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static string GetAevTypeName(ushort lineID)
        {
            try
            {
                SpecialType st = DataBase.FileAEV.Methods.GetSpecialType(lineID);
                if (ListBoxProperty.SpecialTypeList != null && ListBoxProperty.SpecialTypeList.ContainsKey(st))
                {
                    return ListBoxProperty.SpecialTypeList[st].Description;
                }
                return st.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
