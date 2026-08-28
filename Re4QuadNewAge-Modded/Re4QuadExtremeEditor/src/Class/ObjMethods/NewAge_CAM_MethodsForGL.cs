using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Re4QuadExtremeEditor.src.Class.CustomDelegates;
using OpenTK;

namespace Re4QuadExtremeEditor.src.Class.ObjMethods
{
    public class NewAge_CAM_MethodsForGL
    {
        /// <summary>
        /// position of this keyframe (render units, game/100)
        /// </summary>
        public ReturnVector3 GetCameraPosition;

        /// <summary>
        /// look-at target of this keyframe (render units, game/100)
        /// </summary>
        public ReturnVector3 GetCameraTarget;

        /// <summary>
        /// true when the camera has at least one keyframe to draw
        /// </summary>
        public ReturnBool GetHasData;

        /// <summary>
        /// marker color for this keyframe (camera type palette)
        /// </summary>
        public ReturnVector4 GetCameraColor;
    }

    public class NewAge_CAM_Zone_MethodsForGL : BaseTriggerZoneMethodsForGL
    {
        /// <summary>
        /// base-ring corner points of the trigger polygon (render units; XZ used)
        /// </summary>
        public ReturnVector3Array GetZonePoints;

        /// <summary>
        /// bottom height of the prism (render units)
        /// </summary>
        public ReturnFloat GetZoneBottom;

        /// <summary>
        /// top height of the prism (render units)
        /// </summary>
        public ReturnFloat GetZoneTop;

        /// <summary>
        /// fill/outline color of the zone (camera type palette)
        /// </summary>
        public ReturnVector4 GetZoneColor;
    }
}
