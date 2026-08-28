using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Re4QuadExtremeEditor.src.Class.CustomDelegates;
using Re4QuadExtremeEditor.src.Class.Files;

namespace Re4QuadExtremeEditor.src.Class.ObjMethods
{
    public class NewAge_SMX_Methods : BaseMethods
    {
        public ReturnByteArray ReturnLine;
        public SetByteArray SetLine;

        public File_SMX_Group SmxFile;

        public byte ReturnByteFromPosition(ushort ID, int offset)
        {
            return SmxFile.GetByte(ID, offset);
        }

        public void SetByteFromPosition(ushort ID, int offset, byte value)
        {
            SmxFile.SetByte(ID, offset, value);
        }

        public uint ReturnUInt32FromPosition(ushort ID, int offset)
        {
            return SmxFile.GetUInt32_LE(ID, offset);
        }

        public void SetUInt32FromPosition(ushort ID, int offset, uint value)
        {
            SmxFile.SetUInt32_LE(ID, offset, value);
        }

        public float ReturnFloatFromPosition(ushort ID, int offset)
        {
            return SmxFile.GetFloat(ID, offset);
        }

        public void SetFloatFromPosition(ushort ID, int offset, float value)
        {
            SmxFile.SetFloat(ID, offset, value);
        }

        public string ReturnColorRGB(ushort ID)
        {
            return SmxFile.GetColorRGB(ID);
        }

        public void SetColorRGB(ushort ID, string hex)
        {
            SmxFile.SetColorRGB(ID, hex);
        }
    }
}
