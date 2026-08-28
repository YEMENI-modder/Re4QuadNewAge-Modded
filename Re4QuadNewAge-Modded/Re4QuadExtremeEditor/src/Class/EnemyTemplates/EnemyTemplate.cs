using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Re4QuadExtremeEditor.src.Class.EnemyTemplates
{
    public class EnemyTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public ushort EnemyId { get; set; }
        public string EnemyName { get; set; }
        public int Life { get; set; }
        public string LineHex { get; set; }

        public EnemyTemplate()
        {
            Name = "";
            Description = "";
            Category = "Village";
            EnemyId = 0;
            EnemyName = "Unknown";
            Life = 0;
            LineHex = "";
        }

        public static EnemyTemplate FromEnemy(ushort enemyId, string enemyName, byte[] line)
        {
            var t = new EnemyTemplate();
            t.EnemyId = enemyId;
            t.EnemyName = enemyName ?? "Unknown";
            t.Life = BitConverter.ToInt16(line, 0x08);
            t.LineHex = BitConverter.ToString(line).Replace("-", "");
            return t;
        }

        public byte[] GetLineBytes()
        {
            if (string.IsNullOrEmpty(LineHex) || LineHex.Length < 64)
                return new byte[32];
            byte[] r = new byte[32];
            for (int i = 0; i < 32; i++)
                r[i] = Convert.ToByte(LineHex.Substring(i * 2, 2), 16);
            return r;
        }

        public void ApplyToTarget(ushort targetIndex)
        {
            if (DataBase.FileESL == null || !DataBase.FileESL.Lines.ContainsKey(targetIndex))
                return;
            byte[] src = GetLineBytes();
            byte[] dst = DataBase.FileESL.Lines[targetIndex];

            // Copy only: Enable, EnemyID, Unknown bytes, Life (0x00-0x0B)
            // Also copy: tail unknowns (0x1A-0x1F)
            // Skip: Position (0x0C-0x11), Rotation (0x12-0x17), RoomID (0x18-0x19)

            dst[0x00] = src[0x00]; // Enable
            dst[0x01] = src[0x01]; // EnemyID high
            dst[0x02] = src[0x02]; // EnemyID low
            dst[0x03] = src[0x03]; // Unknown
            dst[0x04] = src[0x04]; // Unknown
            dst[0x05] = src[0x05]; // Unknown
            dst[0x06] = src[0x06]; // Unknown
            dst[0x07] = src[0x07]; // Unknown
            dst[0x08] = src[0x08]; // Life low
            dst[0x09] = src[0x09]; // Life high
            dst[0x0A] = src[0x0A]; // Unknown
            dst[0x0B] = src[0x0B]; // Unknown
            // 0x0C-0x11: Position - SKIPPED
            // 0x12-0x17: Rotation - SKIPPED
            // 0x18-0x19: RoomID - SKIPPED
            dst[0x1A] = src[0x1A]; // Unknown
            dst[0x1B] = src[0x1B]; // Unknown
            dst[0x1C] = src[0x1C]; // Unknown
            dst[0x1D] = src[0x1D]; // Unknown
            dst[0x1E] = src[0x1E]; // Unknown
            dst[0x1F] = src[0x1F]; // Unknown
        }

        public JObject ToJson()
        {
            return new JObject
            {
                ["Name"] = Name,
                ["Description"] = Description,
                ["Category"] = Category,
                ["EnemyId"] = EnemyId.ToString("X4"),
                ["EnemyName"] = EnemyName,
                ["Life"] = Life,
                ["LineHex"] = LineHex,
            };
        }

        public static EnemyTemplate FromJson(JObject o)
        {
            if (o == null) return null;
            var t = new EnemyTemplate();
            t.Name = o["Name"]?.ToString() ?? "";
            t.Description = o["Description"]?.ToString() ?? "";
            t.Category = o["Category"]?.ToString() ?? "Village";
            try { t.EnemyId = ushort.Parse(o["EnemyId"]?.ToString() ?? "0", System.Globalization.NumberStyles.HexNumber); } catch { }
            t.EnemyName = o["EnemyName"]?.ToString() ?? "Unknown";
            try { t.Life = int.Parse(o["Life"]?.ToString() ?? "0"); } catch { }
            t.LineHex = o["LineHex"]?.ToString() ?? "";
            return t;
        }
    }
}
