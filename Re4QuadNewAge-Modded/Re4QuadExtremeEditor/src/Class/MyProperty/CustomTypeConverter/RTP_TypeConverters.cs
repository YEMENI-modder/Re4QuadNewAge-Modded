using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using DataBase = Re4QuadExtremeEditor.src.DataBase;

namespace Re4QuadExtremeEditor.src.Class.MyProperty.CustomTypeConverter
{
    /// <summary>
    /// dropdown + manual entry for linking route nodes.
    /// <para>The list shows every waypoint exactly as named in the tree
    /// ("Node 0", "Node 1", ...) in plain DECIMAL - no hex anywhere.</para>
    /// <para>Manual entry accepts "Node 17", plain "17" (always decimal),
    /// or "0x11"/"#11" for hex users. Anything invalid or out of range is
    /// ignored instead of mis-linking.</para>
    /// </summary>
    public abstract class Rtp_NodeLinkConverterBase : TypeConverter
    {
        /// <summary>true = list only the nodes this waypoint is already linked to</summary>
        protected abstract bool OnlyLinkedTargets { get; }

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
                var file = DataBase.FileRTP;
                if (file != null && file.Nodes != null)
                {
                    ushort self = GetContextId(context);
                    HashSet<ushort> linked = null;
                    if (OnlyLinkedTargets && self < file.Nodes.Count)
                    {
                        linked = new HashSet<ushort>();
                        foreach (var e in file.GetNodeEntries(self))
                        {
                            linked.Add(e.TargetNode);
                        }
                    }
                    for (ushort i = 0; i < file.Nodes.Count; i++)
                    {
                        if (i == self) continue;
                        if (linked != null && !linked.Contains(i)) continue;
                        list.Add("Node " + i);
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
                return ParseNodeId((string)value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is int)
            {
                int id = (int)value;
                if (id < 0)
                {
                    return string.Empty;
                }
                return "Node " + id;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private static ushort GetContextId(ITypeDescriptorContext context)
        {
            NewAge_RTP_Property prop = context != null ? context.Instance as NewAge_RTP_Property : null;
            return prop != null ? prop.GetInternalID() : ushort.MaxValue;
        }

        /// <summary>
        /// "Node 17" / "node 17" / "17" / "0x11" / "#11" -> 17 ;
        /// garbage or out-of-range -> -1 (setter then ignores it)
        /// </summary>
        private static int ParseNodeId(string raw)
        {
            string text = raw == null ? string.Empty : raw.Trim();
            if (text.Length == 0)
            {
                return -1;
            }

            // strip a pasted "value - description" suffix if present
            int dash = text.IndexOf(" - ", StringComparison.Ordinal);
            if (dash >= 0)
            {
                text = text.Substring(0, dash).Trim();
            }

            // drop the "Node " prefix so typing/pasting the tree name just works
            if (text.Length > 5 && text.StartsWith("Node ", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(5).Trim();
            }
            if (text.Length == 0)
            {
                return -1;
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

            uint parsed;
            if (!uint.TryParse(text, hex ? NumberStyles.HexNumber : NumberStyles.Integer,
                CultureInfo.InvariantCulture, out parsed))
            {
                return -1;
            }
            if (parsed > ushort.MaxValue)
            {
                return -1;
            }

            var file = DataBase.FileRTP;
            if (file != null && file.Nodes != null && parsed >= file.Nodes.Count)
            {
                return -1;
            }
            return (int)parsed;
        }
    }

    /// <summary>Connect To: dropdown of all other waypoints</summary>
    public class Rtp_ConnectNodeConverter : Rtp_NodeLinkConverterBase
    {
        protected override bool OnlyLinkedTargets { get { return false; } }
    }

    /// <summary>Disconnect From: dropdown of the waypoints this node is linked to</summary>
    public class Rtp_DisconnectNodeConverter : Rtp_NodeLinkConverterBase
    {
        protected override bool OnlyLinkedTargets { get { return true; } }
    }
}
