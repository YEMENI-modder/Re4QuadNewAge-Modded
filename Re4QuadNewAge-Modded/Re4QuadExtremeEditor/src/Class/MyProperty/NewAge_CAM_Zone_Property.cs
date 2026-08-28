using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.Interfaces;
using Re4QuadExtremeEditor.src.Class.ObjMethods;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomAttribute;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomTypeConverter;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomUITypeEditor;

namespace Re4QuadExtremeEditor.src.Class.MyProperty
{
    [DefaultProperty(nameof(InternalLineID))]
    public class NewAge_CAM_Zone_Property : GenericProperty, IInternalID
    {
        public override Type GetClassType()
        {
            return typeof(NewAge_CAM_Zone_Property);
        }

        private const GroupType groupType = GroupType.CAM_ZONE;

        private ushort InternalID = ushort.MaxValue;

        private NewAge_CAM_Zone_Methods Methods = null;
        private UpdateMethods updateMethods = null;

        public ushort GetInternalID()
        {
            return InternalID;
        }

        public GroupType GetGroupType()
        {
            return groupType;
        }

        private void SetPropertyTexts()
        {
            ChangePropertyName(nameof(Line), Lang.GetAttributeText(aLang.NewAge_LineArrayDisplayName).Replace("<<Lenght>>", "48"));
            ChangePropertyName(nameof(LinkLine), Lang.GetAttributeText(aLang.ZN_LinkLine_DisplayName));
        }

        public NewAge_CAM_Zone_Property(NewAge_CAM_Zone_Property prop, bool ForMultiSelection = false)
        {
            NewAge_CAM_Zone_PropertyConstructor(prop.InternalID, prop.updateMethods, prop.Methods, ForMultiSelection);
        }

        public NewAge_CAM_Zone_Property(ushort InternalID, UpdateMethods updateMethods, NewAge_CAM_Zone_Methods Methods, bool ForMultiSelection = false) : base()
        {
            NewAge_CAM_Zone_PropertyConstructor(InternalID, updateMethods, Methods, ForMultiSelection);
        }

        private void NewAge_CAM_Zone_PropertyConstructor(ushort InternalID, UpdateMethods updateMethods, NewAge_CAM_Zone_Methods Methods, bool ForMultiSelection = false)
        {
            this.InternalID = InternalID;
            this.updateMethods = updateMethods;
            this.Methods = Methods;

            if (!ForMultiSelection)
            {
                SetThis(this);
            }

            SetPropertyTexts();
        }

        #region Category Ids
        private const int CategoryID0_InternalLineID = 0;
        private const int CategoryID2_LineArray = 2;
        private const int CategoryID3_Link = 3;
        private const int CategoryID4_Zone = 4;
        private const int CategoryID5_Points = 5;
        #endregion

        [CustomCategory(aLang.NewAge_InternalLineIDCategory)]
        [CustomDisplayName(aLang.NewAge_InternalLineIDDisplayName)]
        [CustomDescription(aLang.NewAge_InternalLineIDDescription)]
        [DefaultValue(null)]
        [ReadOnly(true)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(1, CategoryID0_InternalLineID)]
        public string InternalLineID { get => GetInternalID().ToString(); }

        [CustomCategory(aLang.NewAge_LineArrayCategory)]
        [CustomDisplayName(aLang.NewAge_LineArrayDisplayName)]
        [CustomDescription(aLang.NewAge_LineArrayDescription)]
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
                int len = Methods.ReturnLine(InternalID).Length;
                byte[] insert = value.Take(len).ToArray();
                byte[] _set = new byte[len];
                insert.CopyTo(_set, 0);
                Methods.SetLine(InternalID, _set);
            }
        }

        [CustomCategory(aLang.NewAge_LineArrayCategory)]
        [CustomDisplayName(aLang.ZN_LinkLine_DisplayName)]
        [CustomDescription(aLang.ZN_LinkLine_Description)]
        [TypeConverter(typeof(ByteArrayTypeConverter))]
        [Editor(typeof(NoneUITypeEditor), typeof(UITypeEditor))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(9, CategoryID2_LineArray)]
        public byte[] LinkLine
        {
            get => Methods.ReturnLinkLine(InternalID);
            set
            {
                byte[] insert = value.Take(8).ToArray();
                byte[] _set = new byte[16];
                insert.CopyTo(_set, 0);
                Methods.SetLinkLine(InternalID, _set);
            }
        }

        #region link (Table1)

        [CustomCategory(aLang.NewAge_CAMZoneLink_Category)]
        [CustomDisplayName(aLang.ZN_TriggerType_DisplayName)]
        [CustomDescription(aLang.ZN_TriggerType_Description)]
        [TypeConverter(typeof(CAM_TriggerTypeDropdownConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x100, CategoryID3_Link)]
        public byte ZN_TriggerType
        {
            get => Methods.ReturnTriggerType(InternalID);
            set { Methods.SetTriggerType(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZoneLink_Category)]
        [CustomDisplayName(aLang.ZN_LinkedCamera_DisplayName)]
        [CustomDescription(aLang.ZN_LinkedCamera_Description)]
        [TypeConverter(typeof(CAM_LinkedCameraConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x200, CategoryID3_Link)]
        public int ZN_LinkedCamera
        {
            get => Methods.ReturnLinkedCamera(InternalID);
            set { Methods.SetLinkedCamera(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZoneLink_Category)]
        [CustomDisplayName(aLang.ZN_LinkUnk012_DisplayName)]
        [CustomDescription(aLang.ZN_LinkUnk012_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x300, CategoryID3_Link)]
        public byte ZN_LinkUnk012
        {
            get => Methods.ReturnLinkUnk012(InternalID);
            set { Methods.SetLinkUnk012(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZoneLink_Category)]
        [CustomDisplayName(aLang.ZN_Unk015_DisplayName)]
        [CustomDescription(aLang.ZN_Unk015_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x310, CategoryID3_Link)]
        public ushort ZN_Unk015
        {
            get => Methods.ReturnUnk015(InternalID);
            set { Methods.SetUnk015(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZoneLink_Category)]
        [CustomDisplayName(aLang.ZN_Unk016_DisplayName)]
        [CustomDescription(aLang.ZN_Unk016_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x311, CategoryID3_Link)]
        public ushort ZN_Unk016
        {
            get => Methods.ReturnUnk016(InternalID);
            set { Methods.SetUnk016(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZoneLink_Category)]
        [CustomDisplayName(aLang.ZN_Unk017_DisplayName)]
        [CustomDescription(aLang.ZN_Unk017_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x312, CategoryID3_Link)]
        public ushort ZN_Unk017
        {
            get => Methods.ReturnUnk017(InternalID);
            set { Methods.SetUnk017(InternalID, value); }
        }

        #endregion

        #region zone body (Table2)

        [CustomCategory(aLang.NewAge_CAMZone_Category)]
        [CustomDisplayName(aLang.ZN_EntryNumber_DisplayName)]
        [CustomDescription(aLang.ZN_EntryNumber_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x400, CategoryID4_Zone)]
        public byte ZN_EntryNumber
        {
            get => Methods.ReturnEntryNumber(InternalID);
            set { Methods.SetEntryNumber(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZone_Category)]
        [CustomDisplayName(aLang.ZN_Height_DisplayName)]
        [CustomDescription(aLang.ZN_Height_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x500, CategoryID4_Zone)]
        public float ZN_Height
        {
            get => Methods.ReturnHeight(InternalID);
            set { Methods.SetHeight(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZone_Category)]
        [CustomDisplayName(aLang.ZN_Bottom_DisplayName)]
        [CustomDescription(aLang.ZN_Bottom_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x501, CategoryID4_Zone)]
        public float ZN_Bottom
        {
            get => Methods.ReturnBottom(InternalID);
            set { Methods.SetBottom(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZone_Category)]
        [CustomDisplayName(aLang.ZN_CamTypeTz_DisplayName)]
        [CustomDescription(aLang.ZN_CamTypeTz_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x600, CategoryID4_Zone)]
        public byte ZN_CamTypeTz
        {
            get => Methods.ReturnCamTypeTz(InternalID);
            set { Methods.SetCamTypeTz(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZone_Category)]
        [CustomDisplayName(aLang.ZN_Subtype_DisplayName)]
        [CustomDescription(aLang.ZN_Subtype_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x601, CategoryID4_Zone)]
        public byte ZN_Subtype
        {
            get => Methods.ReturnSubtype(InternalID);
            set { Methods.SetSubtype(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMZone_Category)]
        [CustomDisplayName(aLang.ZN_Unk051_DisplayName)]
        [CustomDescription(aLang.ZN_Unk051_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x602, CategoryID4_Zone)]
        public byte ZN_Unk051
        {
            get => Methods.ReturnUnk051(InternalID);
            set { Methods.SetUnk051(InternalID, value); }
        }

        #endregion

        #region points

        [CustomCategory(aLang.NewAge_CAMPoints_Category)]
        [CustomDisplayName(aLang.ZN_PointCount_DisplayName)]
        [CustomDescription(aLang.ZN_PointCount_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x700, CategoryID5_Points)]
        public int ZN_PointCount
        {
            get => Methods.ReturnPointCount(InternalID);
            set { Methods.SetPointCount(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMPoints_Category)]
        [CustomDisplayName(aLang.ZN_SelectedPoint_DisplayName)]
        [CustomDescription(aLang.ZN_SelectedPoint_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(0x701, CategoryID5_Points)]
        public int ZN_SelectedPoint
        {
            get => Methods.ReturnSelectedPoint(InternalID);
            set { Methods.SetSelectedPoint(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMPoints_Category)]
        [CustomDisplayName(aLang.ZN_PointX_DisplayName)]
        [CustomDescription(aLang.ZN_Point_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x710, CategoryID5_Points)]
        public float ZN_PointX
        {
            get => Methods.ReturnPointX(InternalID);
            set { Methods.SetPointX(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMPoints_Category)]
        [CustomDisplayName(aLang.ZN_PointY_DisplayName)]
        [CustomDescription(aLang.ZN_Point_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x711, CategoryID5_Points)]
        public float ZN_PointY
        {
            get => Methods.ReturnPointY(InternalID);
            set { Methods.SetPointY(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMPoints_Category)]
        [CustomDisplayName(aLang.ZN_PointZ_DisplayName)]
        [CustomDescription(aLang.ZN_Point_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x712, CategoryID5_Points)]
        public float ZN_PointZ
        {
            get => Methods.ReturnPointZ(InternalID);
            set { Methods.SetPointZ(InternalID, value); }
        }

        #endregion
    }
}
