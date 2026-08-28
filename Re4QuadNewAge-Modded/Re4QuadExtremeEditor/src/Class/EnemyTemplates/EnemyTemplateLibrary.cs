using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Re4QuadExtremeEditor.src.Class.EnemyTemplates
{
    public static class EnemyTemplateLibrary
    {
        private static readonly string Dir = Path.Combine("data", "EnemyTemplates");
        private static List<EnemyTemplate> _list = null;

        public static List<EnemyTemplate> Templates { get { if (_list == null) Load(); return _list; } }

        public static void Load()
        {
            _list = new List<EnemyTemplate>();
            try
            {
                if (!Directory.Exists(Dir))
                    Directory.CreateDirectory(Dir);

                foreach (string f in Directory.GetFiles(Dir, "*.json"))
                {
                    try
                    {
                        var t = EnemyTemplate.FromJson(JObject.Parse(File.ReadAllText(f)));
                        if (t != null && !_list.Any(x => x.Name == t.Name))
                            _list.Add(t);
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void Save(EnemyTemplate t)
        {
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
            string safe = string.Join("_", t.Name.Split(Path.GetInvalidFileNameChars()));
            File.WriteAllText(Path.Combine(Dir, safe + ".json"), t.ToJson().ToString());
            int idx = _list.FindIndex(x => x.Name == t.Name);
            if (idx >= 0) _list[idx] = t; else _list.Add(t);
        }

        public static void Delete(EnemyTemplate t)
        {
            string safe = string.Join("_", t.Name.Split(Path.GetInvalidFileNameChars()));
            try { File.Delete(Path.Combine(Dir, safe + ".json")); } catch { }
            _list.Remove(t);
        }

        public static List<EnemyTemplate> Search(string category, string search)
        {
            var list = Templates;
            if (!string.IsNullOrEmpty(category) && category != "All")
                list = list.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                string q = search.ToLowerInvariant();
                list = list.Where(t => t.Name.ToLowerInvariant().Contains(q) || t.EnemyName.ToLowerInvariant().Contains(q) || t.EnemyId.ToString("X4").ToLowerInvariant().Contains(q)).ToList();
            }
            return list;
        }
    }
}
