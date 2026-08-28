using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.Files;
using DataBase = Re4QuadExtremeEditor.src.DataBase;
using Globals = Re4QuadExtremeEditor.src.Globals;

namespace Re4QuadExtremeEditor.src.Class.MyProperty.CustomTypeConverter
{
    /// <summary>
    /// camera type dropdown: values 0-8 with game behavior names;
    /// manual entry accepted: "0x.." = hex, bare digits = decimal,
    /// values above 255 are clamped (no silent byte wrap-around)
    /// </summary>
    public class CAM_TypeDropdownConverter : AVL_HexDecTypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < File_CAM_Group.CamTypeNames.Length; i++)
            {
                list.Add(FormatValue((ulong)i, typeof(byte), File_CAM_Group.CamTypeNames[i]));
            }
            return new StandardValuesCollection(list);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            object parsed = CAM_ManualByteParser.Parse(value);
            if (parsed != null)
            {
                return parsed;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null && IsNumeric(value.GetType()))
            {
                ulong v = ToUlong(value);
                string name = v < (ulong)File_CAM_Group.CamTypeNames.Length ? File_CAM_Group.CamTypeNames[v] : null;
                return FormatValue(v, value.GetType(), name);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// trigger/link type dropdown with the values observed in original files;
    /// manual entry accepted: "0x.." = hex, bare digits = decimal,
    /// values above 255 are clamped (no silent byte wrap-around)
    /// </summary>
    public class CAM_TriggerTypeDropdownConverter : AVL_HexDecTypeConverter
    {
        private static readonly ushort[] Known = new ushort[]
        {
            0x00, 0x01, 0x03, 0x04, 0x23, 0x3B, 0x41, 0x43, 0x63, 0x81, 0x83
        };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List<string>();
            foreach (ushort v in Known)
            {
                list.Add(FormatValue(v, typeof(byte), DescribeTrigger(v)));
            }
            return new StandardValuesCollection(list);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            object parsed = CAM_ManualByteParser.Parse(value);
            if (parsed != null)
            {
                return parsed;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null && IsNumeric(value.GetType()))
            {
                ulong v = ToUlong(value);
                string name = null;
                if (Known.Contains((ushort)v))
                {
                    name = DescribeTrigger((ushort)v);
                }
                return FormatValue(v, value.GetType(), name);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static string DescribeTrigger(ushort v)
        {
            switch (v)
            {
                case 0x03: return "Walk Into Zone";
                case 0x04: return "AEV Event (Inspect/Open)";
                case 0x23: return "Walk Into Zone (Alt)";
                default: return "";
            }
        }
    }

    /// <summary>
    /// manual numeric entry shared by the CAM dropdown converters:
    /// <para>strips a pasted "value - name" suffix back to the raw number;</para>
    /// <para>"0x"/"#" prefix = hexadecimal; bare digits = ALWAYS decimal</para>
    /// <para>(predictable, independent of the global HEX/DEC view mode);</para>
    /// <para>returns null when the text is not plain numeric (falls back to base)</para>
    /// </summary>
    internal static class CAM_ManualByteParser
    {
        public static object Parse(object value)
        {
            if (!(value is string))
            {
                return null;
            }
            string text = ((string)value).Trim();
            int dash = text.IndexOf(" - ", StringComparison.Ordinal);
            if (dash >= 0)
            {
                text = text.Substring(0, dash).Trim();
            }
            if (text.Length == 0)
            {
                return null;
            }

            bool hex;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hex = true;
                text = text.Substring(2);
            }
            else if (text.StartsWith("#"))
            {
                hex = true;
                text = text.Substring(1);
            }
            else
            {
                hex = false;
            }

            if (text.Length == 0 || (hex && text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }
            foreach (char c in text)
            {
                if (!(hex ? Uri.IsHexDigit(c) : char.IsDigit(c)))
                {
                    return null;
                }
            }

            ulong v;
            try
            {
                v = Convert.ToUInt64(text, hex ? 16 : 10);
            }
            catch (Exception)
            {
                return null;
            }
            // clamp instead of the silent byte wrap-around the base does
            if (v > 255)
            {
                v = 255;
            }
            return (byte)v;
        }
    }

    /// <summary>
    /// zone-to-camera link dropdown listing the loaded cameras
    /// </summary>
    public class CAM_LinkedCameraConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List<string>();
            try
            {
                if (DataBase.FileCAM != null && DataBase.FileCAM.Cameras != null)
                {
                    foreach (var pair in DataBase.FileCAM.Cameras.OrderBy(p => p.Key))
                    {
                        CamCameraRecord c = pair.Value;
                        string typeName = c.CamType < File_CAM_Group.CamTypeNames.Length
                            ? File_CAM_Group.CamTypeNames[c.CamType] : "?";
                        list.Add("r" + pair.Key.ToString("X3")
                            + " ID:" + c.CamId
                            + " Keys:" + c.Positions.Count
                            + " [" + typeName + "]");
                    }
                }
            }
            catch (Exception)
            {
            }
            return new StandardValuesCollection(list);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string text = ((string)value).Trim();
                if (text.StartsWith("r") || text.StartsWith("R"))
                {
                    text = text.Substring(1);
                    int sp = text.IndexOf(' ');
                    if (sp >= 0) text = text.Substring(0, sp);
                }
                else if (text.StartsWith("No", StringComparison.OrdinalIgnoreCase))
                {
                    return -1;
                }
                int parsed;
                string hexSource = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? text.Substring(2) : text;
                if (int.TryParse(hexSource, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parsed))
                {
                    return parsed;
                }
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                {
                    return parsed;
                }
                return -1;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is int)
            {
                int idx = (int)value;
                if (idx < 0)
                {
                    return "None";
                }
                try
                {
                    if (DataBase.FileCAM != null && DataBase.FileCAM.Cameras != null
                        && DataBase.FileCAM.Cameras.ContainsKey((ushort)idx))
                    {
                        CamCameraRecord c = DataBase.FileCAM.Cameras[(ushort)idx];
                        string typeName = c.CamType < File_CAM_Group.CamTypeNames.Length
                            ? File_CAM_Group.CamTypeNames[c.CamType] : "?";
                        return "r" + idx.ToString("X3")
                            + " ID:" + c.CamId
                            + " Keys:" + c.Positions.Count
                            + " [" + typeName + "]";
                    }
                }
                catch (Exception)
                {
                }
                return "r" + idx.ToString("X3");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// plain float converter that accepts both "." and "," decimal separators
    /// </summary>
    public class CAM_FloatTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string text = ((string)value).Trim().Replace(",", ".");
                float f;
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                {
                    return f;
                }
                return 0f;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is float)
            {
                return ((float)value).ToString("0.####", CultureInfo.InvariantCulture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// SMX float converter: displays 9 decimal places (e.g. 0.000000000) matching JADERLINK idxsmx format.
    /// Accepts both "." and "," decimal separators.
    /// </summary>
    public class SMX_FloatTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string text = ((string)value).Trim().Replace(",", ".");
                float f;
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                {
                    return f;
                }
                return 0f;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is float)
            {
                return ((float)value).ToString("0.000000000", CultureInfo.InvariantCulture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
