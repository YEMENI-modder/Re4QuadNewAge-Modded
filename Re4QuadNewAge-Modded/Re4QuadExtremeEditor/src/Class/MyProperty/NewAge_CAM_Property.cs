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
    public class NewAge_CAM_Property : GenericProperty, IInternalID
    {
        public override Type GetClassType()
        {
            return typeof(NewAge_CAM_Property);
        }

        private const GroupType groupType = GroupType.CAM;

        private ushort InternalID = ushort.MaxValue;

        private NewAge_CAM_Methods Methods = null;
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
            ChangePropertyName(nameof(Line), Lang.GetAttributeText(aLang.NewAge_LineArrayDisplayName).Replace("<<Lenght>>", "52"));
        }

        public NewAge_CAM_Property(NewAge_CAM_Property prop, bool ForMultiSelection = false)
        {
            NewAge_CAM_PropertyConstructor(prop.InternalID, prop.updateMethods, prop.Methods, ForMultiSelection);
        }

        public NewAge_CAM_Property(ushort InternalID, UpdateMethods updateMethods, NewAge_CAM_Methods Methods, bool ForMultiSelection = false) : base()
        {
            NewAge_CAM_PropertyConstructor(InternalID, updateMethods, Methods, ForMultiSelection);
        }

        private void NewAge_CAM_PropertyConstructor(ushort InternalID, UpdateMethods updateMethods, NewAge_CAM_Methods Methods, bool ForMultiSelection = false)
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
        private const int CategoryID3_CAM = 3;
        private const int CategoryID4_Keyframe = 4;
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

        #region CAM header values

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_CamId_DisplayName)]
        [CustomDescription(aLang.CAM_CamId_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x100, CategoryID3_CAM)]
        public byte CAM_CamId
        {
            get => Methods.ReturnCamId(InternalID);
            set { Methods.SetCamId(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_Type_DisplayName)]
        [CustomDescription(aLang.CAM_Type_Description)]
        [TypeConverter(typeof(CAM_TypeDropdownConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x200, CategoryID3_CAM)]
        public byte CAM_Type
        {
            get => Methods.ReturnCamType(InternalID);
            set { Methods.SetCamType(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_Flags_DisplayName)]
        [CustomDescription(aLang.CAM_Flags_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x300, CategoryID3_CAM)]
        public byte CAM_Flags
        {
            get => Methods.ReturnFlags(InternalID);
            set { Methods.SetFlags(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_Distance_DisplayName)]
        [CustomDescription(aLang.CAM_Distance_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x400, CategoryID3_CAM)]
        public float CAM_Distance
        {
            get => Methods.ReturnDistance(InternalID);
            set { Methods.SetDistance(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_Unk021_DisplayName)]
        [CustomDescription(aLang.CAM_Unk021_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x500, CategoryID3_CAM)]
        public byte CAM_Unk021
        {
            get => Methods.ReturnUnk021(InternalID);
            set { Methods.SetUnk021(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_Unk025_DisplayName)]
        [CustomDescription(aLang.CAM_Unk025_Description)]
        [TypeConverter(typeof(CAM_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x600, CategoryID3_CAM)]
        public uint CAM_Unk025
        {
            get => Methods.ReturnUnk025(InternalID);
            set { Methods.SetUnk025(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_Unk027_DisplayName)]
        [CustomDescription(aLang.CAM_Unk027_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x700, CategoryID3_CAM)]
        public float CAM_Unk027
        {
            get => Methods.ReturnUnk027(InternalID);
            set { Methods.SetUnk027(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_Raw12_DisplayName)]
        [CustomDescription(aLang.CAM_Raw12_Description)]
        [TypeConverter(typeof(ByteArrayTypeConverter))]
        [Editor(typeof(NoneUITypeEditor), typeof(UITypeEditor))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(0x800, CategoryID3_CAM)]
        public byte[] CAM_Raw12
        {
            get => Methods.ReturnRaw12(InternalID);
            set
            {
                int len = 12;
                byte[] insert = value.Take(len).ToArray();
                byte[] _set = new byte[len];
                insert.CopyTo(_set, 0);
                Methods.SetRaw12(InternalID, _set);
            }
        }

        #endregion

        #region flags

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagNormal_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA00, CategoryID3_CAM)]
        public bool CAM_FlagNormal
        {
            get => (Methods.ReturnFlags(InternalID) & 1) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 1, value)); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagBattle_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA01, CategoryID3_CAM)]
        public bool CAM_FlagBattle
        {
            get => (Methods.ReturnFlags(InternalID) & 2) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 2, value)); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagEvent_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA02, CategoryID3_CAM)]
        public bool CAM_FlagEvent
        {
            get => (Methods.ReturnFlags(InternalID) & 4) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 4, value)); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagDoor_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA03, CategoryID3_CAM)]
        public bool CAM_FlagDoor
        {
            get => (Methods.ReturnFlags(InternalID) & 8) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 8, value)); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagOnce_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA04, CategoryID3_CAM)]
        public bool CAM_FlagOnce
        {
            get => (Methods.ReturnFlags(InternalID) & 16) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 16, value)); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagAhead_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA05, CategoryID3_CAM)]
        public bool CAM_FlagAhead
        {
            get => (Methods.ReturnFlags(InternalID) & 32) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 32, value)); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagDirect_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA06, CategoryID3_CAM)]
        public bool CAM_FlagDirect
        {
            get => (Methods.ReturnFlags(InternalID) & 64) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 64, value)); }
        }

        [CustomCategory(aLang.NewAge_CAM_Category)]
        [CustomDisplayName(aLang.CAM_FlagDislgt_DisplayName)]
        [CustomDescription(aLang.CAM_FlagBit_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0xA07, CategoryID3_CAM)]
        public bool CAM_FlagDislgt
        {
            get => (Methods.ReturnFlags(InternalID) & 128) != 0;
            set { Methods.SetFlags(InternalID, SetBit(Methods.ReturnFlags(InternalID), 128, value)); }
        }

        private static byte SetBit(byte b, int bit, bool on)
        {
            return on ? (byte)(b | bit) : (byte)(b & ~bit);
        }

        #endregion

        #region keyframes

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_KeyframeCount_DisplayName)]
        [CustomDescription(aLang.CAM_KeyframeCount_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1000, CategoryID4_Keyframe)]
        public int CAM_KeyframeCount
        {
            get => Methods.ReturnKeyframeCount(InternalID);
            set { Methods.SetKeyframeCount(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_SelectedKeyframe_DisplayName)]
        [CustomDescription(aLang.CAM_SelectedKeyframe_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(0x1001, CategoryID4_Keyframe)]
        public int CAM_SelectedKeyframe
        {
            get => Methods.ReturnSelectedKeyframe(InternalID);
            set { Methods.SetSelectedKeyframe(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_PosX_DisplayName)]
        [CustomDescription(aLang.CAM_Pos_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1010, CategoryID4_Keyframe)]
        public float CAM_PosX
        {
            get => Methods.ReturnPosX(InternalID);
            set { Methods.SetPosX(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_PosY_DisplayName)]
        [CustomDescription(aLang.CAM_Pos_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1011, CategoryID4_Keyframe)]
        public float CAM_PosY
        {
            get => Methods.ReturnPosY(InternalID);
            set { Methods.SetPosY(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_PosZ_DisplayName)]
        [CustomDescription(aLang.CAM_Pos_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1012, CategoryID4_Keyframe)]
        public float CAM_PosZ
        {
            get => Methods.ReturnPosZ(InternalID);
            set { Methods.SetPosZ(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_TargetX_DisplayName)]
        [CustomDescription(aLang.CAM_Target_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1020, CategoryID4_Keyframe)]
        public float CAM_TargetX
        {
            get => Methods.ReturnTargetX(InternalID);
            set { Methods.SetTargetX(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_TargetY_DisplayName)]
        [CustomDescription(aLang.CAM_Target_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1021, CategoryID4_Keyframe)]
        public float CAM_TargetY
        {
            get => Methods.ReturnTargetY(InternalID);
            set { Methods.SetTargetY(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_TargetZ_DisplayName)]
        [CustomDescription(aLang.CAM_Target_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1022, CategoryID4_Keyframe)]
        public float CAM_TargetZ
        {
            get => Methods.ReturnTargetZ(InternalID);
            set { Methods.SetTargetZ(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_Zoom_DisplayName)]
        [CustomDescription(aLang.CAM_Zoom_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1030, CategoryID4_Keyframe)]
        public float CAM_Zoom
        {
            get => Methods.ReturnZoom(InternalID);
            set { Methods.SetZoom(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_Fov_DisplayName)]
        [CustomDescription(aLang.CAM_Fov_Description)]
        [TypeConverter(typeof(CAM_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1040, CategoryID4_Keyframe)]
        public float CAM_Fov
        {
            get => Methods.ReturnFov(InternalID);
            set { Methods.SetFov(InternalID, value); }
        }

        [CustomCategory(aLang.NewAge_CAMKeys_Category)]
        [CustomDisplayName(aLang.CAM_TimeFrame_DisplayName)]
        [CustomDescription(aLang.CAM_TimeFrame_Description)]
        [TypeConverter(typeof(DecNumberTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x1050, CategoryID4_Keyframe)]
        public ushort CAM_TimeFrame
        {
            get => Methods.ReturnTimeFrame(InternalID);
            set { Methods.SetTimeFrame(InternalID, value); }
        }

        #endregion
    }

    /// <summary>
    /// same as AVL hex/dec converter, reused so CAM follows the global HEX/DEC view toggle
    /// </summary>
    public class CAM_HexDecTypeConverter : AVL_HexDecTypeConverter
    {
    }
}
