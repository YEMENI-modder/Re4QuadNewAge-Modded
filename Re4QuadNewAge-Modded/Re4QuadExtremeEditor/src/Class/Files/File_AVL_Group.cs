using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.ObjMethods;
using SimpleEndianBinaryIO;
using System.Drawing;

namespace Re4QuadExtremeEditor.src.Class.Files
{
    /// <summary>
    /// <para>Classe que representa o arquivo .AVL (door lock events, data Qingsheng);</para>
    /// <para>cada registro possui 128 bytes fixos: 7 campos de 16 bytes + rodape de 16 bytes (0x3E);</para>
    /// </summary>
    public class File_AVL_Group : BaseGroup
    {
        /// <summary>
        /// tamanho fixo de cada registro do arquivo;
        /// </summary>
        public const int RecordSize = 128;

        // offsets fixos dos valores dentro de cada registro
        private const int OffAev = 0x0F;         // byte
        private const int OffKeyId = 0x1E;       // ushort little endian (2 bytes)
        private const int OffLockMessage = 0x2F; // byte
        private const int OffLockSound = 0x3F;   // byte
        private const int OffLockCamera = 0x4F;  // byte (0xFF off / 0x01 on)
        private const int OffUnlockMessage = 0x5F; // byte
        private const int OffUnlockSound = 0x6F;   // byte

        /// <summary>
        /// valor que desativa a camera fixa do evento;
        /// </summary>
        public const byte CameraOffValue = 0xFF;
        /// <summary>
        /// valor que ativa a camera fixa do evento;
        /// </summary>
        public const byte CameraOnValue = 0x01;

        /// <summary>
        /// <para>conteudo bruto de cada registro do arquivo;</para>
        /// <para>id da linha, sequencia de bytes do registro;</para>
        /// </summary>
        public Dictionary<ushort, byte[]> Lines { get; private set; }
        /// <summary>
        /// aqui contem o comeco do arquivo, a parte nao usada antes do primeiro registro;
        /// </summary>
        public byte[] StartFile = new byte[0];
        /// <summary>
        /// Id para ser usado para adicionar novas linhas;
        /// </summary>
        public ushort IdForNewLine = 0;

        public File_AVL_Group()
        {
            Lines = new Dictionary<ushort, byte[]>();

            DisplayMethods = new NodeDisplayMethods();
            DisplayMethods.GetNodeText = GetNodeText;
            DisplayMethods.GetNodeColor = GetNodeColor;

            MoveMethods = new NodeMoveMethods();
            MoveMethods.GetObjPostion_ToCamera = Utils.GetObjPostion_ToCamera_Null;
            MoveMethods.GetObjAngleY_ToCamera = Utils.GetObjAngleY_ToCamera_Null;
            MoveMethods.GetObjPostion_ToMove_General = Utils.GetObjPostion_ToMove_General_Null;
            MoveMethods.SetObjPostion_ToMove_General = Utils.SetObjPostion_ToMove_General_Null;
            MoveMethods.GetObjRotationAngles_ToMove = Utils.GetObjRotationAngles_ToMove_Null;
            MoveMethods.SetObjRotationAngles_ToMove = Utils.SetObjRotationAngles_ToMove_Null;
            MoveMethods.GetObjScale_ToMove = Utils.GetObjScale_ToMove_Null;
            MoveMethods.SetObjScale_ToMove = Utils.SetObjScale_ToMove_Null;
            MoveMethods.GetTriggerZoneCategory = Utils.GetTriggerZoneCategory_Null;

            ChangeAmountMethods = new NodeChangeAmountMethods();
            ChangeAmountMethods.AddNewLineID = AddNewLineID;
            ChangeAmountMethods.RemoveLineID = RemoveLineID;

            Methods = new NewAge_AVL_Methods();
            SetBaseMethods(Methods);
            Methods.ReturnLine = ReturnLine;
            Methods.SetLine = SetLine;
            Methods.ReturnNumberOfAev = ReturnNumberOfAevValue;
            Methods.SetNumberOfAev = SetNumberOfAevValue;
            Methods.ReturnKeyId = ReturnKeyIdValue;
            Methods.SetKeyId = SetKeyIdValue;
            Methods.ReturnLockMessage = ReturnLockMessageValue;
            Methods.SetLockMessage = SetLockMessageValue;
            Methods.ReturnLockSound = ReturnLockSoundValue;
            Methods.SetLockSound = SetLockSoundValue;
            Methods.ReturnLockCamera = ReturnLockCameraValue;
            Methods.SetLockCamera = SetLockCameraValue;
            Methods.ReturnLockCameraEnabled = ReturnLockCameraEnabled;
            Methods.SetLockCameraEnabled = SetLockCameraEnabled;
            Methods.ReturnUnlockMessage = ReturnUnlockMessageValue;
            Methods.SetUnlockMessage = SetUnlockMessageValue;
            Methods.ReturnUnlockSound = ReturnUnlockSoundValue;
            Methods.SetUnlockSound = SetUnlockSoundValue;
        }

        /// <summary>
        /// Classe com os metodos que serao passados para classe NewAge_AVL_Property;
        /// </summary>
        public NewAge_AVL_Methods Methods { get; }

        /// <summary>
        /// classe com os metodos responsaveis pelo oque sera exibido no node;
        /// </summary>
        public NodeDisplayMethods DisplayMethods { get; }

        /// <summary>
        /// classe com os metodos responsaveis pela movimentacao dos objetos e da camera
        /// </summary>
        public NodeMoveMethods MoveMethods { get; }

        /// <summary>
        /// Classe com os metodos responsaveis para adicinar e remover linhas/lines
        /// </summary>
        public NodeChangeAmountMethods ChangeAmountMethods { get; }

        //metodos:

        #region metodos para os Nodes

        // texto do treeNode
        public string GetNodeText(ushort ID)
        {
            if (!Lines.ContainsKey(ID))
            {
                return "AVL Error Internal Line ID " + ID;
            }

            if (Globals.TreeNodeRenderHexValues)
            {
                return BitConverter.ToString(Lines[ID]).Replace("-", "_");
            }

            ushort keyId = ReturnKeyIdValue(ID);
            string camStatus = ReturnLockCameraEnabled(ID) ? "ON" : "OFF";
            byte aev = ReturnNumberOfAevValue(ID);
            string aevText = Globals.AvlRenderDecimal ? aev.ToString() : "0x" + aev.ToString("X2");
            return "Door r" + ID.ToString("X3")
                + " KeyID 0x" + keyId.ToString("X4")
                + " Cam:" + camStatus
                + " Lock[Msg 0x" + ReturnLockMessageValue(ID).ToString("X2")
                + " Snd 0x" + ReturnLockSoundValue(ID).ToString("X2") + "]"
                + " Unlock[Msg 0x" + ReturnUnlockMessageValue(ID).ToString("X2")
                + " Snd 0x" + ReturnUnlockSoundValue(ID).ToString("X2") + "]"
                + " => AEV " + aevText;
        }

        public Color GetNodeColor(ushort ID)
        {
            return Globals.NodeColorEntry;
        }

        private ushort AddNewLineID(byte initType)
        {
            ushort newID = IdForNewLine;
            if (IdForNewLine == ushort.MaxValue)
            {
                var Ushots = Utils.AllUshots();
                var Useds = Lines.Keys.ToList();
                Ushots.RemoveAll(x => Useds.Contains(x));
                newID = Ushots[0];
            }
            else
            {
                IdForNewLine++;
            }

            Lines.Add(newID, CreateEmptyRecord());
            // o "Number of Aev" acompanha a numeracao sequencial da porta
            SetNumberOfAevValue(newID, (byte)(Lines.Count & 0xFF));
            return newID;
        }

        private void RemoveLineID(ushort ID)
        {
            Lines.Remove(ID);
        }

        #endregion

        #region metodos das propriedades

        protected override byte[] GetInternalLine(ushort ID)
        {
            return Lines[ID];
        }

        protected override Endianness GetEndianness()
        {
            return Endianness.LittleEndian;
        }

        private byte[] ReturnLine(ushort ID)
        {
            return (byte[])Lines[ID].Clone();
        }

        private void SetLine(ushort ID, byte[] value)
        {
            value.CopyTo(Lines[ID], 0);
        }

        #endregion

        #region motor de valores do registro

        private static bool CanRead(byte[] rec, int offset, int size)
        {
            return rec != null && offset >= 0 && offset + size <= rec.Length;
        }

        private byte ReturnNumberOfAevValue(ushort ID)
        {
            if (!Lines.ContainsKey(ID)) return 0;
            byte[] rec = Lines[ID];
            if (!CanRead(rec, OffAev, 1)) return 0;
            return rec[OffAev];
        }

        private void SetNumberOfAevValue(ushort ID, byte value)
        {
            if (!Lines.ContainsKey(ID)) return;
            byte[] rec = Lines[ID];
            if (!CanRead(rec, OffAev, 1)) return;
            rec[OffAev] = value;
        }

        private ushort ReturnKeyIdValue(ushort ID)
        {
            if (!Lines.ContainsKey(ID)) return 0;
            byte[] rec = Lines[ID];
            if (!CanRead(rec, OffKeyId, 2)) return 0;
            return BitConverter.ToUInt16(rec, OffKeyId);
        }

        private void SetKeyIdValue(ushort ID, ushort value)
        {
            if (!Lines.ContainsKey(ID)) return;
            byte[] rec = Lines[ID];
            if (!CanRead(rec, OffKeyId, 2)) return;
            EndianBitConverter.WriteTo(value, rec, OffKeyId, GetEndianness());
        }

        private byte ReturnByteAt(ushort ID, int offset)
        {
            if (!Lines.ContainsKey(ID)) return 0;
            byte[] rec = Lines[ID];
            if (!CanRead(rec, offset, 1)) return 0;
            return rec[offset];
        }

        private void SetByteAt(ushort ID, int offset, byte value)
        {
            if (!Lines.ContainsKey(ID)) return;
            byte[] rec = Lines[ID];
            if (!CanRead(rec, offset, 1)) return;
            rec[offset] = value;
        }

        private byte ReturnLockMessageValue(ushort ID) => ReturnByteAt(ID, OffLockMessage);
        private void SetLockMessageValue(ushort ID, byte value) => SetByteAt(ID, OffLockMessage, value);

        private byte ReturnLockSoundValue(ushort ID) => ReturnByteAt(ID, OffLockSound);
        private void SetLockSoundValue(ushort ID, byte value) => SetByteAt(ID, OffLockSound, value);

        private byte ReturnLockCameraValue(ushort ID) => ReturnByteAt(ID, OffLockCamera);
        private void SetLockCameraValue(ushort ID, byte value) => SetByteAt(ID, OffLockCamera, value);

        private bool ReturnLockCameraEnabled(ushort ID)
        {
            return ReturnByteAt(ID, OffLockCamera) == CameraOnValue;
        }

        private void SetLockCameraEnabled(ushort ID, bool enabled)
        {
            SetByteAt(ID, OffLockCamera, enabled ? CameraOnValue : CameraOffValue);
        }

        private byte ReturnUnlockMessageValue(ushort ID) => ReturnByteAt(ID, OffUnlockMessage);
        private void SetUnlockMessageValue(ushort ID, byte value) => SetByteAt(ID, OffUnlockMessage, value);

        private byte ReturnUnlockSoundValue(ushort ID) => ReturnByteAt(ID, OffUnlockSound);
        private void SetUnlockSoundValue(ushort ID, byte value) => SetByteAt(ID, OffUnlockSound, value);

        #endregion

        #region template de novo registro

        /// <summary>
        /// cria um registro vazio de 128 bytes no mesmo layout da ferramenta de referencia;
        /// </summary>
        public static byte[] CreateEmptyRecord()
        {
            byte[] res = new byte[RecordSize];

            WriteLabel(res, 0x00, "Number of Aev--");
            res[OffAev] = 0x00;

            WriteLabel(res, 0x10, "Key ID________");

            WriteLabel(res, 0x20, "lock message---");
            WriteLabel(res, 0x30, "lock sound-----");
            WriteLabel(res, 0x40, "lock camera----");
            res[OffLockCamera] = CameraOffValue;

            WriteLabel(res, 0x50, "unlock message-");
            WriteLabel(res, 0x60, "unlock sound---");

            for (int i = 0x70; i < RecordSize; i++)
            {
                res[i] = 0x3E;
            }
            return res;
        }

        private static void WriteLabel(byte[] target, int start, string label)
        {
            byte[] lb = Encoding.ASCII.GetBytes(label);
            for (int i = 0; i < 15; i++)
            {
                target[start + i] = i < lb.Length ? lb[i] : (byte)0x2D;
            }
        }

        #endregion
    }
}
