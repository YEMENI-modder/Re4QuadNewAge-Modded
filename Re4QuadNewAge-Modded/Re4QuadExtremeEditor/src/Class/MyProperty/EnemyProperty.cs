using Re4QuadExtremeEditor.src.Class;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.Interfaces;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomAttribute;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomCollection;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomTypeConverter;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomUITypeEditor;
using Re4QuadExtremeEditor.src.Class.ObjMethods;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;

namespace Re4QuadExtremeEditor.src.Class.MyProperty
{

    [DefaultProperty(nameof(Order))]
    public class EnemyProperty : GenericProperty, IInternalID
    {
        public override Type GetClassType() 
        {
            return typeof(EnemyProperty);
        }

        private const GroupType groupType = GroupType.ESL;

        private ushort InternalID = ushort.MaxValue;
        private EnemyMethods Methods = null;
        private UpdateMethods updateMethods = null;
        private ushort SelectedITAID = ushort.MaxValue;

        public ushort GetInternalID()
        {
            return InternalID;
        }

        public GroupType GetGroupType()
        {
            return groupType;
        }

        public EnemyProperty(EnemyProperty prop, bool ForMultiSelection = false) 
        {
            EnemyPropertyConstructor(prop.InternalID, prop.updateMethods, prop.Methods, ForMultiSelection);
            SelectedITAID = prop.SelectedITAID;
        }

        public EnemyProperty(ushort InternalID, UpdateMethods updateMethods, EnemyMethods Methods, bool ForMultiSelection = false) : base()
        {
            EnemyPropertyConstructor(InternalID, updateMethods, Methods, ForMultiSelection);
        }

        private void EnemyPropertyConstructor(ushort InternalID, UpdateMethods updateMethods, EnemyMethods Methods, bool ForMultiSelection = false)
        {
            this.InternalID = InternalID;
            this.Methods = Methods;
            this.updateMethods = updateMethods;

            if (!ForMultiSelection)
            {
                SetThis(this);
                RefreshAssociatedITAPropertiesVisibility();
            }
        }

        #region Category Ids
        private const int CategoryID0_Order = 0;
        private const int CategoryID1_AssociatedSpecialEvent = 1;
        private const int CategoryID2_LineArray = 2;
        private const int CategoryID3_Enemy = 3;
        private const int CategoryID4_AssociatedITAItem = 4;
        #endregion

        #region parte1

        [CustomCategory(aLang.Enemy_OrderCategory)]
        [CustomDisplayName(aLang.Enemy_OrderDisplayName)]
        [CustomDescription(aLang.Enemy_OrderDescription)]
        [DefaultValue(null)]
        [ReadOnly(true)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(1, CategoryID0_Order)]
        public string Order { get => "0x" + GetInternalID().ToString("X2"); }

        [CustomCategory(aLang.Enemy_AssociatedSpecialEventCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedSpecialEventTypeDisplayName)]
        [CustomDescription(aLang.Enemy_AssociatedSpecialEventTypeDescription)]
        [DefaultValue(null)]
        [ReadOnly(true)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(3, CategoryID1_AssociatedSpecialEvent)]
        public string AssociatedSpecialEventType { get { return DataBase.Extras.AssociatedSpecialEventType(RefInteractionType.Enemy, InternalID); } }

        [CustomCategory(aLang.Enemy_AssociatedSpecialEventCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedSpecialEventFromSpecialIndexDisplayName)]
        [CustomDescription(aLang.Enemy_AssociatedSpecialEventFromSpecialIndexFromDescription)]
        [DefaultValue(null)]
        [ReadOnly(true)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(4, CategoryID1_AssociatedSpecialEvent)]
        public string AssociatedSpecialEventFromSpecialIndex { get { return DataBase.Extras.AssociatedSpecialEventFromSpecialIndex(RefInteractionType.Enemy, InternalID); } }

        [CustomCategory(aLang.Enemy_AssociatedSpecialEventCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedSpecialEventObjNameDisplayName)]
        [CustomDescription(aLang.Enemy_AssociatedSpecialEventObjNameDescription)]
        [DefaultValue(null)]
        [ReadOnly(true)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(5, CategoryID1_AssociatedSpecialEvent)]
        public string AssociatedSpecialEventObjName { get { return DataBase.Extras.AssociatedSpecialEventObjName(RefInteractionType.Enemy, InternalID); } }    

        [CustomCategory(aLang.Enemy_AssociatedSpecialEventCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedSpecialEventFromFileDisplayName)]
        [CustomDescription(aLang.Enemy_AssociatedSpecialEventFromFileFromDescription)]
        [DefaultValue(null)]
        [ReadOnly(true)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(6, CategoryID1_AssociatedSpecialEvent)]
        public string AssociatedSpecialEventFromFile { get { return DataBase.Extras.AssociatedSpecialEventFromFile(RefInteractionType.Enemy, InternalID); } }


        #region Associated ITA item shortcut

        private bool HasITAFile()
        {
            return DataBase.FileITA != null;
        }

        public void RefreshAssociatedITAPropertiesVisibility()
        {
            bool show = DataBase.FileITA != null;

            ChangePropertyIsBrowsable(nameof(AssociatedITA), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITARefInteractionType_ListBox), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITARefInteractionIndex), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAItemNumber), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAItemNumber_ListBox), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAUnknownRU), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAItemAmount), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAPromptMessage), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAPromptMessage_ListBox), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITASecundIndex), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAItemAuraType), show);
            ChangePropertyIsBrowsable(nameof(AssociatedITAItemAuraType_ListBox), show);
        }

        private ushort FindAssociatedITAID()
        {
            if (!HasITAFile())
            {
                return ushort.MaxValue;
            }

            if (InternalID > 0xFF)
            {
                return ushort.MaxValue;
            }

            byte enemyID = (byte)InternalID;
            if (SelectedITAID != ushort.MaxValue && DataBase.FileITA.Lines.ContainsKey(SelectedITAID) &&
                DataBase.FileITA.Methods.GetRefInteractionType(SelectedITAID) == RefInteractionType.Enemy &&
                DataBase.FileITA.Methods.ReturnRefInteractionIndex(SelectedITAID) == enemyID)
            {
                return SelectedITAID;
            }

            SelectedITAID = ushort.MaxValue;
            foreach (ushort lineID in DataBase.FileITA.Lines.Keys.OrderBy(x => x))
            {
                if (DataBase.FileITA.Methods.GetRefInteractionType(lineID) == RefInteractionType.Enemy &&
                    DataBase.FileITA.Methods.ReturnRefInteractionIndex(lineID) == enemyID)
                {
                    SelectedITAID = lineID;
                    return lineID;
                }
            }

            return ushort.MaxValue;
        }

        private SpecialMethods GetSelectedITAMethods()
        {
            ushort itaID = FindAssociatedITAID();
            if (itaID == ushort.MaxValue || DataBase.FileITA == null)
            {
                return null;
            }

            return DataBase.FileITA.Methods;
        }

        private ushort GetSelectedITAID()
        {
            return FindAssociatedITAID();
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedITAItemDisplayName)]
        [CustomDescription(aLang.Enemy_AssociatedITAItemDescription)]
        [Editor(typeof(AssociatedITAEntryGridComboBox), typeof(UITypeEditor))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(0x4000, CategoryID4_AssociatedITAItem)]
        public UshortObjForListBox AssociatedITA
        {
            get
            {
                ushort id = FindAssociatedITAID();
                if (id != ushort.MaxValue)
                {
                    return new UshortObjForListBox(id, AssociatedITAEntryGridComboBox.GetDescription(id));
                }

                return new UshortObjForListBox(ushort.MaxValue, "None - Unlink ITA");
            }
            set
            {
                if (!HasITAFile() || value == null)
                {
                    return;
                }

                ushort id = value.ID;

                if (id == ushort.MaxValue)
                {
                    ushort oldID = FindAssociatedITAID();
                    if (oldID != ushort.MaxValue)
                    {
                        DataBase.FileITA.Methods.SetRefInteractionType(oldID, (byte)RefInteractionType.Disable);
                        DataBase.FileITA.Methods.SetRefInteractionIndex(oldID, 0);
                    }

                    SelectedITAID = ushort.MaxValue;
                }
                else
                {
                    if (InternalID > 0xFF || !DataBase.FileITA.Lines.ContainsKey(id))
                    {
                        return;
                    }

                    SelectedITAID = id;
                    DataBase.FileITA.Methods.SetRefInteractionType(id, (byte)RefInteractionType.Enemy);
                    DataBase.FileITA.Methods.SetRefInteractionIndex(id, (byte)InternalID);
                }

                updateMethods?.UpdateGL?.Invoke();
                updateMethods?.UpdatePropertyGrid?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedITARefInteractionTypeDisplayName)]
        [Editor(typeof(RefInteractionTypeGridComboBox), typeof(UITypeEditor))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4100, CategoryID4_AssociatedITAItem)]
        public ByteObjForListBox AssociatedITARefInteractionType_ListBox
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return new ByteObjForListBox(0xFF, "XX: Unavailable");
                }

                byte value = methods.ReturnRefInteractionType(GetSelectedITAID());
                if (value <= 0x02 && ListBoxProperty.RefInteractionTypeList.ContainsKey(value))
                {
                    return ListBoxProperty.RefInteractionTypeList[value];
                }

                return new ByteObjForListBox(0xFF, "XX: Unknown");
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null || value == null || value.ID == 0xFF)
                {
                    return;
                }

                methods.SetRefInteractionType(GetSelectedITAID(), value.ID);
                updateMethods?.UpdateGL?.Invoke();
                updateMethods?.UpdatePropertyGrid?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedITARefInteractionIndexDisplayName)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4200, CategoryID4_AssociatedITAItem)]
        public byte AssociatedITARefInteractionIndex
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                return methods == null ? (byte)0xFF : methods.ReturnRefInteractionIndex(GetSelectedITAID());
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetRefInteractionIndex(GetSelectedITAID(), value);
                updateMethods?.UpdateGL?.Invoke();
                updateMethods?.UpdatePropertyGrid?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedITAItemNumberDisplayName)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4300, CategoryID4_AssociatedITAItem)]
        public ushort AssociatedITAItemNumber
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                return methods == null ? ushort.MaxValue : methods.ReturnItemNumber(GetSelectedITAID());
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetItemNumber(GetSelectedITAID(), value);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.ItemNumber_List_DisplayName)]
        [Editor(typeof(ItemIDGridComboBox), typeof(UITypeEditor))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4400, CategoryID4_AssociatedITAItem)]
        public UshortObjForListBox AssociatedITAItemNumber_ListBox
        {
            get
            {
                ushort value = AssociatedITAItemNumber;
                if (ListBoxProperty.ItemsList.ContainsKey(value) && value != ushort.MaxValue)
                {
                    return ListBoxProperty.ItemsList[value];
                }

                return new UshortObjForListBox(ushort.MaxValue, "XXXX: " + Lang.GetAttributeText(aLang.ListBoxUnknownItem));
            }
            set
            {
                if (value == null || value.ID == ushort.MaxValue)
                {
                    return;
                }

                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetItemNumber(GetSelectedITAID(), value.ID);
                updateMethods?.UpdateGL?.Invoke();
            }
        }


        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.Unknown_RU_ByteArray2_DisplayName)]
        [TypeConverter(typeof(ByteArrayTypeConverter))]
        [Editor(typeof(NoneUITypeEditor), typeof(UITypeEditor))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4800, CategoryID4_AssociatedITAItem)]
        public byte[] AssociatedITAUnknownRU
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return new byte[2];
                }

                byte[] value = methods.ReturnUnknown_RU(GetSelectedITAID());
                if (value == null)
                {
                    return new byte[2];
                }

                byte[] result = new byte[2];
                Array.Copy(value, result, Math.Min(value.Length, result.Length));
                return result;
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                byte[] result = new byte[2];
                if (value != null)
                {
                    Array.Copy(value, result, Math.Min(value.Length, result.Length));
                }

                methods.SetUnknown_RU(GetSelectedITAID(), result);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.ItemAmount_Ushort_DisplayName)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4900, CategoryID4_AssociatedITAItem)]
        public ushort AssociatedITAItemAmount
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                return methods == null ? (ushort)0 : methods.ReturnItemAmount(GetSelectedITAID());
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetItemAmount(GetSelectedITAID(), value);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.PromptMessage_Byte_DisplayName)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4A00, CategoryID4_AssociatedITAItem)]
        public byte AssociatedITAPromptMessage
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                return methods == null ? (byte)0xFF : methods.ReturnPromptMessage(GetSelectedITAID());
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetPromptMessage(GetSelectedITAID(), value);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.PromptMessage_List_DisplayName)]
        [Editor(typeof(PromptMessageGridComboBox), typeof(UITypeEditor))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4A01, CategoryID4_AssociatedITAItem)]
        public ByteObjForListBox AssociatedITAPromptMessage_ListBox
        {
            get
            {
                byte value = AssociatedITAPromptMessage;
                if (ListBoxProperty.PromptMessageList.ContainsKey(value))
                {
                    return ListBoxProperty.PromptMessageList[value];
                }

                return new ByteObjForListBox(0xFF, "XX: " + Lang.GetAttributeText(aLang.ListBoxPromptMessageTypeAnotherValue));
            }
            set
            {
                if (value == null || value.ID == 0xFF)
                {
                    return;
                }

                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetPromptMessage(GetSelectedITAID(), value.ID);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedITASecundIndexDisplayName)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4500, CategoryID4_AssociatedITAItem)]
        public ushort AssociatedITASecundIndex
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                return methods == null ? ushort.MaxValue : methods.ReturnSecundIndex(GetSelectedITAID());
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetSecundIndex(GetSelectedITAID(), value);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.Enemy_AssociatedITAItemAuraTypeDisplayName)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x4600, CategoryID4_AssociatedITAItem)]
        public ushort AssociatedITAItemAuraType
        {
            get
            {
                SpecialMethods methods = GetSelectedITAMethods();
                return methods == null ? ushort.MaxValue : methods.ReturnItemAuraType(GetSelectedITAID());
            }
            set
            {
                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetItemAuraType(GetSelectedITAID(), value);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        [CustomCategory(aLang.Enemy_AssociatedITAItemCategory)]
        [CustomDisplayName(aLang.ItemAuraType_List_DisplayName)]
        [Editor(typeof(ItemAuraTypeGridComboBox), typeof(UITypeEditor))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(0x4700, CategoryID4_AssociatedITAItem)]
        public UshortObjForListBox AssociatedITAItemAuraType_ListBox
        {
            get
            {
                ushort value = AssociatedITAItemAuraType;
                if (ListBoxProperty.ItemAuraTypeList.ContainsKey(value))
                {
                    return ListBoxProperty.ItemAuraTypeList[value];
                }

                return new UshortObjForListBox(ushort.MaxValue, "XX: " + Lang.GetAttributeText(aLang.ListBoxItemAuraTypeAnotherValue));
            }
            set
            {
                if (value == null || value.ID == ushort.MaxValue)
                {
                    return;
                }

                SpecialMethods methods = GetSelectedITAMethods();
                if (methods == null)
                {
                    return;
                }

                methods.SetItemAuraType(GetSelectedITAID(), value.ID);
                updateMethods?.UpdateGL?.Invoke();
            }
        }

        #endregion

        [CustomCategory(aLang.Enemy_LineArrayCategory)]
        [CustomDisplayName(aLang.Enemy_LineArrayDisplayName)]
        [CustomDescription(aLang.Enemy_LineArrayDescription)]
        [TypeConverter(typeof(ByteArrayTypeConverter))]
        [Editor(typeof(NoneUITypeEditor), typeof(UITypeEditor))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(8, CategoryID2_LineArray)]
        public byte[] Line 
        { 
            get => Methods.ReturnLine(InternalID); 
            set 
            {
                byte[] _set = new byte[32];
                byte[] insert = value.Take(32).ToArray();
                Line.CopyTo(_set, 0);
                insert.CopyTo(_set, 0);
                Methods.SetLine(InternalID, _set);
                updateMethods.UpdateOrbitCamera();
                updateMethods.UpdateGL();
            } 
        }


        #endregion


        #region  // propriedades do imimigo

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_ENABLE_Byte_Name)]
        [CustomDescription(aLang.ESL_ENABLE_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(100, CategoryID3_Enemy)]
        public byte ESL_ENABLE
        {
            get => Methods.ReturnOffset0x00Enable(InternalID);
            set
            {
                Methods.SetOffset0x00Enable(InternalID, value);
                updateMethods.UpdateGL();
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_ENABLE_List_Name)]
        [CustomDescription(aLang.ESL_ENABLE_Byte_Description)]
        [Editor(typeof(EnemyEnableGridComboBox), typeof(UITypeEditor))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(200, CategoryID3_Enemy)]
        public ByteObjForListBox ESL_ENABLE_ListBox
        {
            get
            {
                byte v = Methods.ReturnOffset0x00Enable(InternalID);
                if (v == 0x00)
                {
                    return ListBoxProperty.EnemyEnableList[0x00];
                }
                else if (v == 0x01)
                {
                    return ListBoxProperty.EnemyEnableList[0x01];
                }
                else
                {
                    return new ByteObjForListBox(0xFF, "XX: " + Lang.GetAttributeText(aLang.ListBoxAnotherValue));
                }
            }
            set
            {
                if (value.ID < 0xFF)
                {
                    Methods.SetOffset0x00Enable(InternalID, value.ID);
                    updateMethods.UpdateGL();
                }
            }
        }


        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_ENEMY_ID_UshotUnflip_Name)]
        [CustomDescription(aLang.ESL_ENEMY_ID_UshotUnflip_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(300, CategoryID3_Enemy)]
        public ushort ESL_ENEMY_ID
        {
            get => Methods.ReturnEnemyID(InternalID);
            set
            {
                Methods.SetEnemyID(InternalID, value);
                updateMethods.UpdateGL();
            }
        }


        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_ENEMY_ID_List_Name)]
        [CustomDescription(aLang.ESL_ENEMY_ID_UshotUnflip_Description)]
        [Editor(typeof(EnemyIDGridComboBox), typeof(UITypeEditor))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(400, CategoryID3_Enemy)]
        public UshortObjForListBox ESL_ENEMY_ID_ListBox
        {
            get
            {
                ushort v = Methods.ReturnEnemyID(InternalID);
                string sv = v.ToString("X4");
                string svff = sv[0].ToString() + sv[1].ToString() + "FF";
                ushort vff = ushort.Parse(svff, System.Globalization.NumberStyles.HexNumber);
                if (ListBoxProperty.EnemiesList.ContainsKey(v) && v != 0xFFFF)
                {
                    return ListBoxProperty.EnemiesList[v];
                }
                else if (DataBase.EnemiesIDs.List.ContainsKey(vff) && vff != 0xFFFF)
                {
                    return new UshortObjForListBox(vff, sv[0].ToString() + sv[1].ToString() + "XX: " + DataBase.EnemiesIDs.List[vff].Description);
                }
                else
                {
                    return new UshortObjForListBox(0xFFFF, "XXXX: " + Lang.GetAttributeText(aLang.ListBoxUnknownEnemy));
                }
            }
            set
            {
                if (value.ID < 0xFFFF)
                {
                    Methods.SetEnemyID(InternalID, value.ID);
                    updateMethods.UpdateGL();
                }
            }
        }




        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX03_Byte_Name)]
        [CustomDescription(aLang.ESL_HX03_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(500, CategoryID3_Enemy)]
        public byte ESL_HX03
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x03);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x03, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX04_Byte_Name)]
        [CustomDescription(aLang.ESL_HX04_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(600, CategoryID3_Enemy)]
        public byte ESL_HX04
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x04);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x04, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX05_Byte_Name)]
        [CustomDescription(aLang.ESL_HX05_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(700, CategoryID3_Enemy)]
        public byte ESL_HX05
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x05);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x05, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX06_Byte_Name)]
        [CustomDescription(aLang.ESL_HX06_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(800, CategoryID3_Enemy)]
        public byte ESL_HX06
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x06);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x06, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX07_Byte_Name)]
        [CustomDescription(aLang.ESL_HX07_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(900, CategoryID3_Enemy)]
        public byte ESL_HX07
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x07);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x07, value);
            }
        }


        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_EnemyLifeAmount_Short_Name)]
        [CustomDescription(aLang.ESL_EnemyLifeAmount_Short_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNegativeNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1000, CategoryID3_Enemy)]
        public short ESL_LIFE
        {
            get => Methods.ReturnLife(InternalID);
            set
            {
                Methods.SetLife(InternalID, value);
            }
        }


        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX0A_Byte_Name)]
        [CustomDescription(aLang.ESL_HX0A_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1100, CategoryID3_Enemy)]
        public byte ESL_HX0A
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x0A);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x0A, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX0B_Byte_Name)]
        [CustomDescription(aLang.ESL_HX0B_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1200, CategoryID3_Enemy)]
        public byte ESL_HX0B
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x0B);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x0B, value);
            }
        }


        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_PositionX_Short_Name)]
        [CustomDescription(aLang.ESL_PositionX_Short_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNegativeNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1300, CategoryID3_Enemy)]
        public short ESL_PositionX
        {
            get => Methods.ReturnPositionX(InternalID);
            set
            {
                Methods.SetPositionX(InternalID, value);
                updateMethods.UpdateOrbitCamera();
                updateMethods.UpdateGL();
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_PositionY_Short_Name)]
        [CustomDescription(aLang.ESL_PositionY_Short_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNegativeNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1400, CategoryID3_Enemy)]
        public short ESL_PositionY
        {
            get => Methods.ReturnPositionY(InternalID);
            set
            {
                Methods.SetPositionY(InternalID, value);
                updateMethods.UpdateOrbitCamera();
                updateMethods.UpdateGL();
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_PositionZ_Short_Name)]
        [CustomDescription(aLang.ESL_PositionZ_Short_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNegativeNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1500, CategoryID3_Enemy)]
        public short ESL_PositionZ
        {
            get => Methods.ReturnPositionZ(InternalID);
            set
            {
                Methods.SetPositionZ(InternalID, value);
                updateMethods.UpdateOrbitCamera();
                updateMethods.UpdateGL();
            }
        }


        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_AngleX_Short_Name)]
        [CustomDescription(aLang.ESL_AngleX_Short_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNegativeNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1600, CategoryID3_Enemy)]
        public short ESL_RotationX
        {
            get => Methods.ReturnRotationX(InternalID);
            set
            {
                Methods.SetRotationX(InternalID, value);
                updateMethods.UpdateGL();
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_AngleY_Short_Name)]
        [CustomDescription(aLang.ESL_AngleY_Short_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNegativeNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1700, CategoryID3_Enemy)]
        public short ESL_RotationY
        {
            get => Methods.ReturnRotationY(InternalID);
            set
            {
                Methods.SetRotationY(InternalID, value);
                updateMethods.UpdateGL();
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_AngleZ_Short_Name)]
        [CustomDescription(aLang.ESL_AngleZ_Short_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DecNegativeNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1800, CategoryID3_Enemy)]
        public short ESL_RotationZ
        {
            get => Methods.ReturnRotationZ(InternalID);
            set
            {
                Methods.SetRotationZ(InternalID, value);
                updateMethods.UpdateGL();
            }
        }


        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_ROOM_ID_Ushort_Name)]
        [CustomDescription(aLang.ESL_ROOM_ID_Ushort_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(1900, CategoryID3_Enemy)]
        public ushort ESL_ROOM_ID
        {
            get => Methods.ReturnRoomID(InternalID);
            set
            {
                Methods.SetRoomID(InternalID, value);
                updateMethods.UpdateGL();
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX1A_Byte_Name)]
        [CustomDescription(aLang.ESL_HX1A_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(2000, CategoryID3_Enemy)]
        public byte ESL_HX1A
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x1A);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x1A, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX1B_Byte_Name)]
        [CustomDescription(aLang.ESL_HX1B_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(2100, CategoryID3_Enemy)]
        public byte ESL_HX1B
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x1B);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x1B, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX1C_Byte_Name)]
        [CustomDescription(aLang.ESL_HX1C_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(2200, CategoryID3_Enemy)]
        public byte ESL_HX1C
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x1C);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x1C, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX1D_Byte_Name)]
        [CustomDescription(aLang.ESL_HX1D_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(2300, CategoryID3_Enemy)]
        public byte ESL_HX1D
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x1D);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x1D, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX1E_Byte_Name)]
        [CustomDescription(aLang.ESL_HX1E_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(2400, CategoryID3_Enemy)]
        public byte ESL_HX1E
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x1E);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x1E, value);
            }
        }

        [CustomCategory(aLang.EnemyCategory)]
        [CustomDisplayName(aLang.ESL_HX1F_Byte_Name)]
        [CustomDescription(aLang.ESL_HX1F_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(2500, CategoryID3_Enemy)]
        public byte ESL_HX1F
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x1F);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x1F, value);
            }
        }

        #endregion


        #region Search Methods


        public ushort ReturnUshortFirstSearchSelect() 
        {
            ushort v = Methods.ReturnEnemyID(InternalID);
            string sv = v.ToString("X4");
            string svff = sv[0].ToString() + sv[1].ToString() + "00";
            ushort vff = ushort.Parse(svff, System.Globalization.NumberStyles.HexNumber);
            if (ListBoxProperty.EnemiesList.ContainsKey(v))
            {
                return v;
            }
            else if (ListBoxProperty.EnemiesList.ContainsKey(vff))
            {
                return vff;
            }
            return v;
        }

        public void Searched(object obj) 
        {
            if (obj is UshortObjForListBox ushortObj)
            {
                Methods.SetEnemyID(InternalID, ushortObj.ID);
                updateMethods.UpdateTreeViewObjs();
                updateMethods.UpdatePropertyGrid();
                updateMethods.UpdateOrbitCamera();
                updateMethods.UpdateGL();
            }
        }

        #endregion
    }


}
