using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Re4QuadExtremeEditor.src.Class.CustomDelegates;
using OpenTK;

namespace Re4QuadExtremeEditor.src.Class.ObjMethods
{
    public delegate List<Vector3[]> ReturnRtpLinkSegments();

    public class NewAge_RTP_MethodsForGL
    {
        /// <summary>
        /// node position in render units (game/100, Y up)
        /// </summary>
        public ReturnVector3 GetNodePosition;

        /// <summary>
        /// true when there is at least one node to draw
        /// </summary>
        public ReturnBool GetHasData;

        /// <summary>
        /// all connection segments (pairs of render-space points)
        /// </summary>
        public ReturnRtpLinkSegments GetLinkSegments;
    }
}
