using System.Collections.Generic;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// Per-object viewport isolation used by the H shortcut.
    /// When active, only the captured tree nodes are drawn/pickable;
    /// everything else in every loaded file type is skipped at render time.
    /// Shift+H clears the filter and restores the full scene.
    /// </summary>
    public static class IsolateFilter
    {
        // reference set of TreeNodes - stale nodes after a file reload simply
        // never match, so there is no false-positive risk
        private static readonly HashSet<TreeNode> allowed = new HashSet<TreeNode>();

        public static bool Active { get { return allowed.Count > 0; } }

        public static void Set(IEnumerable<TreeNode> nodes)
        {
            allowed.Clear();
            if (nodes == null) return;
            foreach (TreeNode n in nodes)
            {
                if (n != null) allowed.Add(n);
            }
        }

        public static void Clear()
        {
            allowed.Clear();
        }

        /// <summary>true when isolation is on and this node must not be drawn.</summary>
        public static bool IsBlocked(TreeNode item)
        {
            return allowed.Count > 0 && !allowed.Contains(item);
        }
    }
}
