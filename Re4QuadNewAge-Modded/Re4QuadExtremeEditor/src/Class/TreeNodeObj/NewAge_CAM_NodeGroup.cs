using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.ObjMethods;
using Re4QuadExtremeEditor.src.Class.Interfaces;

namespace Re4QuadExtremeEditor.src.Class.TreeNodeObj
{
    public class NewAge_CAM_NodeGroup : TreeNodeGroup, INodeChangeAmount
    {
        public NewAge_CAM_NodeGroup() : base() { }
        public NewAge_CAM_NodeGroup(string text) : base(text) { }
        public NewAge_CAM_NodeGroup(string text, TreeNode[] children) : base(text, children) { }

        public NodeChangeAmountMethods ChangeAmountMethods { get; set; }
        public NewAge_CAM_Methods PropertyMethods { get; set; }
        public NewAge_CAM_MethodsForGL MethodsForGL { get; set; }
    }

    public class NewAge_CAM_Zone_NodeGroup : TreeNodeGroup, INodeChangeAmount
    {
        public NewAge_CAM_Zone_NodeGroup() : base() { }
        public NewAge_CAM_Zone_NodeGroup(string text) : base(text) { }
        public NewAge_CAM_Zone_NodeGroup(string text, TreeNode[] children) : base(text, children) { }

        public NodeChangeAmountMethods ChangeAmountMethods { get; set; }
        public NewAge_CAM_Zone_Methods PropertyMethods { get; set; }
        public BaseTriggerZoneMethodsForGL MethodsForGL { get; set; }
    }
}
