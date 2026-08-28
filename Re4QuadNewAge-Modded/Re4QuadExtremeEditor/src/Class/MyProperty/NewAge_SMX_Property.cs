using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing.Design;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.Files;
using Re4QuadExtremeEditor.src.Class.Interfaces;
using Re4QuadExtremeEditor.src.Class.ObjMethods;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomAttribute;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomTypeConverter;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomUITypeEditor;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomCollection;

namespace Re4QuadExtremeEditor.src.Class.MyProperty
{
    [DefaultProperty(nameof(InternalLineID))]
    public class NewAge_SMX_Property : GenericProperty, IInternalID
    {
        public override Type GetClassType()
        {
            return typeof(NewAge_SMX_Property);
        }

        private const GroupType groupType = GroupType.SMX;

        private ushort InternalID = ushort.MaxValue;

        private NewAge_SMX_Methods Methods = null;
        private UpdateMethods updateMethods = null;

        private byte _cachedMode = 0xFF;

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
            ChangePropertyName(nameof(Line), Lang.GetAttributeText(aLang.NewAge_LineArrayDisplayName).Replace("<<Lenght>>", "144"));
        }

        public NewAge_SMX_Property(NewAge_SMX_Property prop, bool ForMultiSelection = false)
        {
            NewAge_SMX_PropertyConstructor(prop.InternalID, prop.updateMethods, prop.Methods, ForMultiSelection);
        }

        public NewAge_SMX_Property(ushort InternalID, UpdateMethods updateMethods, NewAge_SMX_Methods Methods, bool ForMultiSelection = false) : base()
        {
            NewAge_SMX_PropertyConstructor(InternalID, updateMethods, Methods, ForMultiSelection);
        }

        private void NewAge_SMX_PropertyConstructor(ushort InternalID, UpdateMethods updateMethods, NewAge_SMX_Methods Methods, bool ForMultiSelection = false)
        {
            this.InternalID = InternalID;
            this.updateMethods = updateMethods;
            this.Methods = Methods;

            if (!ForMultiSelection)
            {
                SetThis(this);
            }

            _cachedMode = this.Methods.ReturnByteFromPosition(InternalID, 0x01);

            SetPropertyTexts();
        }

        #region Category Ids
        private const int CategoryID0_InternalLineID = 0;
        private const int CategoryID2_LineArray = 2;
        private const int CategoryID3_SMX = 3;
        private const int CategoryID4_SMXMode0 = 4;
        private const int CategoryID5_SMXMode1 = 5;
        private const int CategoryID6_SMXMode2 = 6;
        #endregion

        #region firt propertys

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
                byte[] _set = new byte[File_SMX_Group.SMX_RECORD_SIZE];
                byte[] insert = value.Take(File_SMX_Group.SMX_RECORD_SIZE).ToArray();
                Line.CopyTo(_set, 0);
                insert.CopyTo(_set, 0);
                Methods.SetLine(InternalID, _set);
            }
        }

        #endregion

        #region SMX Common

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_UseSMXID_Byte_DisplayName)]
        [CustomDescription(aLang.SMX_UseSMXID_Byte_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x100, CategoryID3_SMX)]
        public byte SMX_UseSMXID
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x00);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x00, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Mode_Byte_DisplayName)]
        [CustomDescription(aLang.SMX_Mode_Byte_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x101, CategoryID3_SMX)]
        public byte SMX_Mode
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x01);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x01, value);
                _cachedMode = value;
                this.updateMethods.UpdatePropertyGrid();
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_OpacityHierarchy_Byte_DisplayName)]
        [CustomDescription(aLang.SMX_OpacityHierarchy_Byte_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x102, CategoryID3_SMX)]
        public byte SMX_OpacityHierarchy
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x02);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x02, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_FaceCulling_Byte_DisplayName)]
        [CustomDescription(aLang.SMX_FaceCulling_Byte_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x103, CategoryID3_SMX)]
        public byte SMX_FaceCulling
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x03);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x03, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_LightSwitch_Uint32_DisplayName)]
        [CustomDescription(aLang.SMX_LightSwitch_Uint32_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x104, CategoryID3_SMX)]
        public uint SMX_LightSwitch
        {
            get => Methods.ReturnUInt32FromPosition(InternalID, 0x04);
            set
            {
                Methods.SetUInt32FromPosition(InternalID, 0x04, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_AlphaHierarchy_Byte_DisplayName)]
        [CustomDescription(aLang.SMX_AlphaHierarchy_Byte_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x105, CategoryID3_SMX)]
        public byte SMX_AlphaHierarchy
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x08);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x08, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_ColorRGB_DisplayName)]
        [CustomDescription(aLang.SMX_ColorRGB_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x106, CategoryID3_SMX)]
        public string SMX_ColorRGB
        {
            get => Methods.ReturnColorRGB(InternalID);
            set
            {
                Methods.SetColorRGB(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_ColorAlpha_Byte_DisplayName)]
        [CustomDescription(aLang.SMX_ColorAlpha_Byte_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x107, CategoryID3_SMX)]
        public byte SMX_ColorAlpha
        {
            get => Methods.ReturnByteFromPosition(InternalID, 0x0F);
            set
            {
                Methods.SetByteFromPosition(InternalID, 0x0F, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_SpecularColor_Uint32_DisplayName)]
        [CustomDescription(aLang.SMX_SpecularColor_Uint32_Description)]
        [TypeConverter(typeof(HexNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x108, CategoryID3_SMX)]
        public uint SMX_SpecularColor
        {
            get => Methods.ReturnUInt32FromPosition(InternalID, 0x84);
            set
            {
                Methods.SetUInt32FromPosition(InternalID, 0x84, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_TextureMovement_X_Float_DisplayName)]
        [CustomDescription(aLang.SMX_TextureMovement_X_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x109, CategoryID3_SMX)]
        public float SMX_TextureMovement_X
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x88);
            set
            {
                Methods.SetFloatFromPosition(InternalID, 0x88, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_TextureMovement_Y_Float_DisplayName)]
        [CustomDescription(aLang.SMX_TextureMovement_Y_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x10A, CategoryID3_SMX)]
        public float SMX_TextureMovement_Y
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x8C);
            set
            {
                Methods.SetFloatFromPosition(InternalID, 0x8C, value);
            }
        }

        #endregion

        #region Mode 0x00 Normal

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Line_ByteArray_DisplayName)]
        [CustomDescription(aLang.SMX_Line_ByteArray_Description)]
        [TypeConverter(typeof(ByteArrayTypeConverter))]
        [Editor(typeof(NoneUITypeEditor), typeof(UITypeEditor))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [DynamicTypeDescriptor.Id(0x400, CategoryID4_SMXMode0)]
        public byte[] SMX_Normal_Bytes
        {
            get
            {
                byte[] data = Methods.ReturnLine(InternalID);
                byte[] result = new byte[116];
                Array.Copy(data, 0x10, result, 0, 116);
                return result;
            }
            set
            {
                byte[] _line = Methods.ReturnLine(InternalID);
                byte[] insert = value.Take(116).ToArray();
                insert.CopyTo(_line, 0x10);
                Methods.SetLine(InternalID, _line);
            }
        }

        #endregion

        #region Mode 0x01 Rotate

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_RotationSpeed_X_Float_DisplayName)]
        [CustomDescription(aLang.SMX_RotationSpeed_X_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x500, CategoryID5_SMXMode1)]
        public float SMX_RotationSpeed_X
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x10);
            set
            {
                Methods.SetFloatFromPosition(InternalID, 0x10, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_RotationSpeed_Y_Float_DisplayName)]
        [CustomDescription(aLang.SMX_RotationSpeed_Y_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x501, CategoryID5_SMXMode1)]
        public float SMX_RotationSpeed_Y
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x14);
            set
            {
                Methods.SetFloatFromPosition(InternalID, 0x14, value);
            }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_RotationSpeed_Z_Float_DisplayName)]
        [CustomDescription(aLang.SMX_RotationSpeed_Z_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x502, CategoryID5_SMXMode1)]
        public float SMX_RotationSpeed_Z
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x18);
            set
            {
                Methods.SetFloatFromPosition(InternalID, 0x18, value);
            }
        }

        #endregion

        #region Mode 0x02 Swing

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing0_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing0_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x600, CategoryID6_SMXMode2)]
        public float SMX_Swing0
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x10);
            set { Methods.SetFloatFromPosition(InternalID, 0x10, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing1_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing1_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x601, CategoryID6_SMXMode2)]
        public float SMX_Swing1
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x14);
            set { Methods.SetFloatFromPosition(InternalID, 0x14, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing2_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing2_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x602, CategoryID6_SMXMode2)]
        public float SMX_Swing2
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x18);
            set { Methods.SetFloatFromPosition(InternalID, 0x18, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing3_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing3_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x603, CategoryID6_SMXMode2)]
        public float SMX_Swing3
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x1C);
            set { Methods.SetFloatFromPosition(InternalID, 0x1C, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing4_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing4_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x604, CategoryID6_SMXMode2)]
        public float SMX_Swing4
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x20);
            set { Methods.SetFloatFromPosition(InternalID, 0x20, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing5_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing5_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x605, CategoryID6_SMXMode2)]
        public float SMX_Swing5
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x24);
            set { Methods.SetFloatFromPosition(InternalID, 0x24, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing6_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing6_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x606, CategoryID6_SMXMode2)]
        public float SMX_Swing6
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x28);
            set { Methods.SetFloatFromPosition(InternalID, 0x28, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing7_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing7_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x607, CategoryID6_SMXMode2)]
        public float SMX_Swing7
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x2C);
            set { Methods.SetFloatFromPosition(InternalID, 0x2C, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing8_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing8_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x608, CategoryID6_SMXMode2)]
        public float SMX_Swing8
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x30);
            set { Methods.SetFloatFromPosition(InternalID, 0x30, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_Swing9_Float_DisplayName)]
        [CustomDescription(aLang.SMX_Swing9_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x609, CategoryID6_SMXMode2)]
        public float SMX_Swing9
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x34);
            set { Methods.SetFloatFromPosition(InternalID, 0x34, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_SwingA_Float_DisplayName)]
        [CustomDescription(aLang.SMX_SwingA_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x60A, CategoryID6_SMXMode2)]
        public float SMX_SwingA
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x38);
            set { Methods.SetFloatFromPosition(InternalID, 0x38, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_SwingB_Float_DisplayName)]
        [CustomDescription(aLang.SMX_SwingB_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x60B, CategoryID6_SMXMode2)]
        public float SMX_SwingB
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x3C);
            set { Methods.SetFloatFromPosition(InternalID, 0x3C, value); }
        }

        [CustomCategory(aLang.NewAge_SMX_Category)]
        [CustomDisplayName(aLang.SMX_SwingC_Float_DisplayName)]
        [CustomDescription(aLang.SMX_SwingC_Float_Description)]
        [TypeConverter(typeof(SMX_FloatTypeConverter))]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x60C, CategoryID6_SMXMode2)]
        public float SMX_SwingC
        {
            get => Methods.ReturnFloatFromPosition(InternalID, 0x40);
            set { Methods.SetFloatFromPosition(InternalID, 0x40, value); }
        }

        #endregion
    }
}
