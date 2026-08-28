using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Re4QuadExtremeEditor.src.Class;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.ObjMethods;

namespace Re4QuadExtremeEditor.src.Class.MyProperty.CustomUITypeEditor
{
    /// <summary>
    /// Drop-down list of ITA entries. The value is the ITA line index.
    /// </summary>
    public class AssociatedITAEntryGridComboBox : GridComboBox
    {
        protected override void RetrieveDataList(ITypeDescriptorContext context)
        {
            var list = new List<UshortObjForListBox>();
            list.Add(new UshortObjForListBox(0xFFFF, "None - Unlink ITA"));

            if (DataBase.FileITA == null)
            {
                DataList = list.ToArray();
                return;
            }

            foreach (ushort lineID in DataBase.FileITA.Lines.Keys.OrderBy(x => x))
            {
                list.Add(new UshortObjForListBox(lineID, GetDescription(lineID)));
            }

            DataList = list.ToArray();
        }

        /// <summary>
        /// Creates the concise ITA label used by both the list and the selected value.
        /// The displayed ITA number is the Special Index shown first in the ITA tree.
        /// The underlying list value remains the line ID so selecting an entry is exact.
        /// </summary>
        public static string GetDescription(ushort lineID)
        {
            if (DataBase.FileITA == null || !DataBase.FileITA.Lines.ContainsKey(lineID))
            {
                return $"ITA [{lineID:X2}]";
            }

            var methods = DataBase.FileITA.Methods;
            byte itaIndex = methods.ReturnSpecialIndex(lineID);
            string description = $"ITA [{itaIndex:X2}]";
            RefInteractionType refInteractionType = methods.GetRefInteractionType(lineID);

            if (refInteractionType == RefInteractionType.Enemy)
            {
                byte enemyOrderID = methods.ReturnRefInteractionIndex(lineID);
                description += $" [LINKED: {GetEnemyDescription(enemyOrderID)}]";
            }
            else if (refInteractionType == RefInteractionType.EtcModel)
            {
                description += $" [LINKED: ETS [{methods.ReturnRefInteractionIndex(lineID):X2}]]";
            }
            else if (refInteractionType == RefInteractionType.Disable)
            {
                description += " [FREE]";
            }
            else
            {
                description += " [IN USE]";
            }

            return description + $" - {GetITAObjectName(methods, lineID)}";
        }

        private static string GetITAObjectName(SpecialMethods methods, ushort lineID)
        {
            SpecialType specialType = methods.GetSpecialType(lineID);
            if (specialType == SpecialType.T03_Items)
            {
                ushort itemNumber = methods.ReturnItemNumber(lineID);
                if (DataBase.ItemsIDs != null && DataBase.ItemsIDs.List.ContainsKey(itemNumber))
                {
                    return DataBase.ItemsIDs.List[itemNumber].Name;
                }

                return Lang.GetAttributeText(aLang.ListBoxUnknownItem);
            }

            if (ListBoxProperty.SpecialTypeList.ContainsKey(specialType))
            {
                string typeName = ListBoxProperty.SpecialTypeList[specialType].Description;
                int separator = typeName.IndexOf(':');
                return separator >= 0 ? typeName.Substring(separator + 1).Trim() : typeName;
            }

            return Lang.GetAttributeText(aLang.SpecialTypeUnspecifiedType);
        }

        private static string GetEnemyDescription(byte enemyOrderID)
        {
            string description = $"Enemy [{enemyOrderID:X2}]";
            string enemyName = GetEnemyName(enemyOrderID);
            return string.IsNullOrEmpty(enemyName) ? description : $"{description} - {enemyName}";
        }

        private static string GetEnemyName(byte enemyOrderID)
        {
            if (DataBase.FileESL == null || !DataBase.FileESL.Lines.ContainsKey(enemyOrderID) || DataBase.EnemiesIDs == null)
            {
                return null;
            }

            ushort enemyModelID = DataBase.FileESL.Methods.ReturnEnemyID(enemyOrderID);
            string enemyModelIDHex = enemyModelID.ToString("X4");

            if (DataBase.EnemiesIDs.List.ContainsKey(enemyModelID) &&
                !(enemyModelIDHex[2] == 'F' && enemyModelIDHex[3] == 'F'))
            {
                return DataBase.EnemiesIDs.List[enemyModelID].Name;
            }

            ushort enemyModelIDFF = ushort.Parse(
                enemyModelIDHex[0].ToString() + enemyModelIDHex[1].ToString() + "FF",
                System.Globalization.NumberStyles.HexNumber);

            return DataBase.EnemiesIDs.List.ContainsKey(enemyModelIDFF) && enemyModelIDFF != 0xFFFF
                ? DataBase.EnemiesIDs.List[enemyModelIDFF].Name
                : null;
        }

        protected override object GetDataObjectSelected(ITypeDescriptorContext context)
        {
            return ListBox.SelectedItem;
        }

        protected override void onStart()
        {
        }
    }
}
