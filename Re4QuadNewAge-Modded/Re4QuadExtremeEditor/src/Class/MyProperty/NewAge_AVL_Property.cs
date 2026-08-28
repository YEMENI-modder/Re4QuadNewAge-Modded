using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing.Design;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.Interfaces;
using Re4QuadExtremeEditor.src.Class.ObjMethods;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomAttribute;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomTypeConverter;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomUITypeEditor;
using Re4QuadExtremeEditor.src.Class.MyProperty.CustomCollection;

namespace Re4QuadExtremeEditor.src.Class.MyProperty
{
    [DefaultProperty(nameof(InternalLineID))]
    public class NewAge_AVL_Property : GenericProperty, IInternalID
    {
        public override Type GetClassType()
        {
            return typeof(NewAge_AVL_Property);
        }

        private const GroupType groupType = GroupType.AVL;

        private ushort InternalID = ushort.MaxValue;

        private NewAge_AVL_Methods Methods = null;
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
            ChangePropertyName(nameof(Line), Lang.GetAttributeText(aLang.NewAge_LineArrayDisplayName).Replace("<<Lenght>>", "128"));
        }
        public NewAge_AVL_Property(NewAge_AVL_Property prop, bool ForMultiSelection = false)
        {
            NewAge_AVL_PropertyConstructor(prop.InternalID, prop.updateMethods, prop.Methods, ForMultiSelection);
        }

        public NewAge_AVL_Property(ushort InternalID, UpdateMethods updateMethods, NewAge_AVL_Methods Methods, bool ForMultiSelection = false) : base()
        {
            NewAge_AVL_PropertyConstructor(InternalID, updateMethods, Methods, ForMultiSelection);
        }

        private void NewAge_AVL_PropertyConstructor(ushort InternalID, UpdateMethods updateMethods, NewAge_AVL_Methods Methods, bool ForMultiSelection = false)
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
        private const int CategoryID3_AVL = 3;
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
                int len = Methods.ReturnLine(InternalID).Length;
                byte[] insert = value.Take(len).ToArray();
                byte[] _set = new byte[len];
                insert.CopyTo(_set, 0);
                Methods.SetLine(InternalID, _set);
            }
        }

        #endregion

        #region values

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_NumberOfAev_DisplayName)]
        [CustomDescription(aLang.AVL_NumberOfAev_Description)]
        [TypeConverter(typeof(AVL_AevNumberTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x100, CategoryID3_AVL)]
        public byte AVL_NumberOfAev
        {
            get => Methods.ReturnNumberOfAev(InternalID);
            set
            {
                Methods.SetNumberOfAev(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_KeyId_DisplayName)]
        [CustomDescription(aLang.AVL_KeyId_Description)]
        [TypeConverter(typeof(AVL_KeyIdTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x200, CategoryID3_AVL)]
        public ushort AVL_KeyId
        {
            get => Methods.ReturnKeyId(InternalID);
            set
            {
                Methods.SetKeyId(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_LockMessage_DisplayName)]
        [CustomDescription(aLang.AVL_LockMessage_Description)]
        [TypeConverter(typeof(AVL_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x300, CategoryID3_AVL)]
        public byte AVL_LockMessage
        {
            get => Methods.ReturnLockMessage(InternalID);
            set
            {
                Methods.SetLockMessage(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_LockSound_DisplayName)]
        [CustomDescription(aLang.AVL_LockSound_Description)]
        [TypeConverter(typeof(AVL_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x400, CategoryID3_AVL)]
        public byte AVL_LockSound
        {
            get => Methods.ReturnLockSound(InternalID);
            set
            {
                Methods.SetLockSound(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_LockCameraEnabled_DisplayName)]
        [CustomDescription(aLang.AVL_LockCameraEnabled_Description)]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x500, CategoryID3_AVL)]
        public bool AVL_LockCameraEnabled
        {
            get => Methods.ReturnLockCameraEnabled(InternalID);
            set
            {
                Methods.SetLockCameraEnabled(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_LockCamera_DisplayName)]
        [CustomDescription(aLang.AVL_LockCamera_Description)]
        [TypeConverter(typeof(AVL_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x600, CategoryID3_AVL)]
        public byte AVL_LockCamera
        {
            get => Methods.ReturnLockCamera(InternalID);
            set
            {
                Methods.SetLockCamera(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_UnlockMessage_DisplayName)]
        [CustomDescription(aLang.AVL_UnlockMessage_Description)]
        [TypeConverter(typeof(AVL_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x700, CategoryID3_AVL)]
        public byte AVL_UnlockMessage
        {
            get => Methods.ReturnUnlockMessage(InternalID);
            set
            {
                Methods.SetUnlockMessage(InternalID, value);
            }
        }

        [CustomCategory(aLang.NewAge_AVL_Category)]
        [CustomDisplayName(aLang.AVL_UnlockSound_DisplayName)]
        [CustomDescription(aLang.AVL_UnlockSound_Description)]
        [TypeConverter(typeof(AVL_HexDecTypeConverter))]
        [HexNumber()]
        [DefaultValue(null)]
        [ReadOnly(false)]
        [Browsable(true)]
        [AllowInMultiSelect()]
        [DynamicTypeDescriptor.Id(0x800, CategoryID3_AVL)]
        public byte AVL_UnlockSound
        {
            get => Methods.ReturnUnlockSound(InternalID);
            set
            {
                Methods.SetUnlockSound(InternalID, value);
            }
        }

        #endregion
    }
}
