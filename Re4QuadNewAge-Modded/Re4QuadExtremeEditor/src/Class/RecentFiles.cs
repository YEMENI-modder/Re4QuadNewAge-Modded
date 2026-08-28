using System;
using System.Collections.Generic;
using System.IO;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// Session + persisted list of recently opened data files.
    /// Each entry stores the exact loader kind (captured inside FileManager)
    /// plus the full path, so reopening always uses the correct variant
    /// (2007/UHD/PS4-NS, little/big endian, ...).
    /// Persisted through Configs.json as "Kind|Path" strings.
    /// </summary>
    public static class RecentFiles
    {
        private const int MaxEntries = 12;

        private static readonly List<KeyValuePair<string, string>> items =
            new List<KeyValuePair<string, string>>();

        /// <summary>Most recent first.</summary>
        public static IReadOnlyList<KeyValuePair<string, string>> Items
        {
            get { return items; }
        }

        /// <summary>
        /// Records a successfully opened file. Same path moves to the top;
        /// the list never grows past MaxEntries.
        /// </summary>
        public static void Note(string kind, string path)
        {
            if (string.IsNullOrEmpty(kind) || string.IsNullOrEmpty(path)) return;
            try { path = Path.GetFullPath(path); } catch (Exception) { return; }

            for (int i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i].Value, path, StringComparison.OrdinalIgnoreCase))
                {
                    items.RemoveAt(i);
                    break;
                }
            }

            items.Insert(0, new KeyValuePair<string, string>(kind, path));
            if (items.Count > MaxEntries) items.RemoveRange(MaxEntries, items.Count - MaxEntries);
        }

        /// <summary>Hydrates from Configs.json strings ("Kind|Path").</summary>
        public static void Restore(IEnumerable<string> stored)
        {
            items.Clear();
            if (stored == null) return;
            foreach (string entry in stored)
            {
                if (string.IsNullOrEmpty(entry)) continue;
                int sep = entry.IndexOf('|');
                if (sep <= 0 || sep == entry.Length - 1) continue;
                Note(entry.Substring(0, sep), entry.Substring(sep + 1));
            }
        }

        /// <summary>Serializes back into Configs.json strings.</summary>
        public static List<string> ToStoredList()
        {
            var list = new List<string>();
            foreach (KeyValuePair<string, string> kv in items)
            {
                list.Add(kv.Key + "|" + kv.Value);
            }
            return list;
        }
    }
}
