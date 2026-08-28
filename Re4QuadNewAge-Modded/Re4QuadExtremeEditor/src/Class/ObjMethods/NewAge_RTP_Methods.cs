using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Re4QuadExtremeEditor.src.Class.CustomDelegates;

namespace Re4QuadExtremeEditor.src.Class.ObjMethods
{
    public delegate void RtpLinkAction(ushort ID, ushort Target);

    /// <summary>
    /// ids of the nodes a waypoint is currently linked to (file state)
    /// </summary>
    public delegate ushort[] ReturnLinkedIds(ushort ID);

    public class NewAge_RTP_Methods : BaseMethods
    {
        /// <summary>
        /// the 16 raw bytes of the node record
        /// </summary>
        public ReturnByteArray ReturnLine;
        public SetByteArray SetLine;

        public ReturnFloat ReturnPosX;
        public SetFloat SetPosX;

        public ReturnFloat ReturnPosY;
        public SetFloat SetPosY;

        public ReturnFloat ReturnPosZ;
        public SetFloat SetPosZ;

        /// <summary>
        /// start index of this node inside the shared distance table
        /// </summary>
        public ReturnUshort ReturnDistanceTableIndex;
        public SetUshort SetDistanceTableIndex;

        /// <summary>
        /// how many distance entries belong to this node
        /// </summary>
        public ReturnUshort ReturnConnectionCount;
        public SetUshort SetConnectionCount;

        /// <summary>
        /// human readable list of "target (distance)" pairs
        /// </summary>
        public ReturnString ReturnLinksSummary;

        /// <summary>
        /// ids of the nodes this waypoint is linked to, in file order
        /// </summary>
        public ReturnLinkedIds ReturnLinkedIds;

        /// <summary>
        /// creates a two-way link ID<->Target using the real 3D distance
        /// </summary>
        public RtpLinkAction ConnectTo;

        /// <summary>
        /// removes the two-way link between ID and Target
        /// </summary>
        public RtpLinkAction DisconnectFrom;
    }
}
