using OpenTK;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.ObjMethods;
using Re4QuadExtremeEditor.src.Class.TreeNodeObj;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor.src.Class.Files
{
    /// <summary>
    /// one waypoint of the enemy route network.
    /// Raw keeps the exact 16 bytes of the record so untouched files round-trip
    /// byte-perfect; the typed properties read/write straight into it.
    /// File layout: float X, float Y(up), float Z, ushort DistanceTableIndex, ushort ConnectionCount.
    /// Verified against Son of Persia's RTP obj export: he writes Blender Z-up as (X, -Z, Y),
    /// which means the stored floats are already game-native X/Y/Z with Y vertical.
    /// </summary>
    public class RtpNodeRecord
    {
        public byte[] Raw = new byte[16];

        public float FileX
        {
            get { return BitConverter.ToSingle(Raw, 0); }
            set { BitConverter.GetBytes(value).CopyTo(Raw, 0); }
        }

        public float FileY
        {
            get { return BitConverter.ToSingle(Raw, 4); }
            set { BitConverter.GetBytes(value).CopyTo(Raw, 4); }
        }

        public float FileZ
        {
            get { return BitConverter.ToSingle(Raw, 8); }
            set { BitConverter.GetBytes(value).CopyTo(Raw, 8); }
        }

        public ushort DistanceTableIndex
        {
            get { return BitConverter.ToUInt16(Raw, 12); }
            set { BitConverter.GetBytes(value).CopyTo(Raw, 12); }
        }

        public ushort ConnectionCount
        {
            get { return BitConverter.ToUInt16(Raw, 14); }
            set { BitConverter.GetBytes(value).CopyTo(Raw, 14); }
        }

        // game-space helpers (editor convention: Y up, everything /100 for GL)
        public float GameX { get { return FileX; } set { FileX = value; } }
        public float GameY { get { return FileY; } set { FileY = value; } }
        public float GameZ { get { return FileZ; } set { FileZ = value; } }
    }

    public struct RtpDistanceEntry
    {
        public ushort TargetNode;
        public ushort Distance;
    }

    /// <summary>
    /// RE4 UHD "PTR2" route/path file (enemy navigation network).
    /// Header (24 bytes): "PTR2", u16 unknown(always 0), u16 NodeCount,
    /// u16 DistanceCount, u16 ConnectionTableSize(N*N), u32 offsets of
    /// nodes(24) / distances / connection table. Zero padding to 32-byte
    /// multiple at the end is preserved verbatim.
    /// </summary>
    public class File_RTP_Group : BaseGroup
    {
        public const uint Magic = 0x32525450; // 'PTR2'
        public const float UnitScale = 100f;

        public ushort HdrUnk004 = 0;
        public List<RtpNodeRecord> Nodes = new List<RtpNodeRecord>();
        public List<RtpDistanceEntry> Distances = new List<RtpDistanceEntry>();
        public byte[] Connections = new byte[0];
        private byte[] TrailingPadding = new byte[0];

        public ushort IdForNewNode = 0;

        public File_RTP_Group()
        {
            DisplayMethods = new NodeDisplayMethods();
            DisplayMethods.GetNodeText = GetNodeTextInternal;
            DisplayMethods.GetNodeColor = GetNodeColorInternal;

            MoveMethods = new NodeMoveMethods();
            MoveMethods.GetObjPostion_ToCamera = GetNodePos_ToCamera;
            MoveMethods.GetObjAngleY_ToCamera = Utils.GetObjAngleY_ToCamera_Null;
            MoveMethods.GetObjPostion_ToMove_General = GetNodePostion_ToMove;
            MoveMethods.SetObjPostion_ToMove_General = SetNodePostion_ToMove;
            MoveMethods.GetObjRotationAngles_ToMove = Utils.GetObjRotationAngles_ToMove_Null;
            MoveMethods.SetObjRotationAngles_ToMove = Utils.SetObjRotationAngles_ToMove_Null;
            MoveMethods.GetObjScale_ToMove = Utils.GetObjScale_ToMove_Null;
            MoveMethods.SetObjScale_ToMove = Utils.SetObjScale_ToMove_Null;
            MoveMethods.GetTriggerZoneCategory = Utils.GetTriggerZoneCategory_Null;

            ChangeAmountMethods = new NodeChangeAmountMethods();
            ChangeAmountMethods.AddNewLineID = AddNewNodeID;
            ChangeAmountMethods.RemoveLineID = RemoveNodeID;

            MethodsForGL = new NewAge_RTP_MethodsForGL();
            MethodsForGL.GetNodePosition = GetNodePositionGL;
            MethodsForGL.GetHasData = delegate (ushort ignore) { return Nodes.Count > 0; };
            MethodsForGL.GetLinkSegments = GetLinkSegmentsGL;

            Methods = new NewAge_RTP_Methods();
            SetBaseMethods(Methods);
            WireNodeMethods();
        }

        public NewAge_RTP_Methods Methods { get; }
        public NodeDisplayMethods DisplayMethods { get; }
        public NodeMoveMethods MoveMethods { get; }
        public NodeChangeAmountMethods ChangeAmountMethods { get; }
        public NewAge_RTP_MethodsForGL MethodsForGL { get; }

        protected override byte[] GetInternalLine(ushort ID)
        {
            if (ID >= Nodes.Count) return new byte[16];
            return Nodes[ID].Raw;
        }

        protected override Endianness GetEndianness()
        {
            return Endianness.LittleEndian;
        }

        #region parse

        public void Load(byte[] all)
        {
            if (all == null || all.Length < 24)
            {
                throw new InvalidDataException("Invalid RTP file! File is too small.");
            }

            uint magic = BitConverter.ToUInt32(all, 0);
            if (magic != Magic)
            {
                throw new InvalidDataException("Invalid RTP file! Expected magic PTR2, got 0x" + magic.ToString("X8"));
            }

            HdrUnk004 = BitConverter.ToUInt16(all, 4);
            int nodeCount = BitConverter.ToUInt16(all, 6);
            int distCount = BitConverter.ToUInt16(all, 8);
            int connSize = BitConverter.ToUInt16(all, 10);
            uint offNodes = BitConverter.ToUInt32(all, 12);
            uint offDist = BitConverter.ToUInt32(all, 16);
            uint offConn = BitConverter.ToUInt32(all, 20);

            if (offNodes + nodeCount * 16 > all.Length
                || offDist + distCount * 4 > all.Length
                || offConn + connSize > all.Length)
            {
                throw new InvalidDataException("Invalid RTP file! Declared regions exceed the file size.");
            }

            Nodes.Clear();
            Distances.Clear();

            for (int i = 0; i < nodeCount; i++)
            {
                RtpNodeRecord n = new RtpNodeRecord();
                Array.Copy(all, (int)offNodes + i * 16, n.Raw, 0, 16);
                Nodes.Add(n);
            }

            for (int i = 0; i < distCount; i++)
            {
                RtpDistanceEntry e = new RtpDistanceEntry();
                e.TargetNode = BitConverter.ToUInt16(all, (int)offDist + i * 4);
                e.Distance = BitConverter.ToUInt16(all, (int)offDist + i * 4 + 2);
                Distances.Add(e);
            }

            Connections = new byte[nodeCount * nodeCount];
            Array.Copy(all, (int)offConn, Connections, 0, nodeCount * nodeCount);

            int used = (int)(offConn + nodeCount * nodeCount);
            TrailingPadding = new byte[Math.Max(0, all.Length - used)];
            Array.Copy(all, used, TrailingPadding, 0, TrailingPadding.Length);
        }

        public void WriteTo(Stream stream)
        {
            int nodeCount = Nodes.Count;
            int distCount = Distances.Count;

            uint offNodes = 24;
            uint offDist = offNodes + (uint)(nodeCount * 16);
            uint offConn = offDist + (uint)(distCount * 4);

            stream.WriteByte(0x50); stream.WriteByte(0x54); stream.WriteByte(0x52); stream.WriteByte(0x32);
            WriteU16(stream, HdrUnk004);
            WriteU16(stream, (ushort)nodeCount);
            WriteU16(stream, (ushort)distCount);
            WriteU16(stream, (ushort)(nodeCount * nodeCount));
            WriteU32(stream, offNodes);
            WriteU32(stream, offDist);
            WriteU32(stream, offConn);

            for (int i = 0; i < nodeCount; i++)
            {
                stream.Write(Nodes[i].Raw, 0, 16);
            }

            for (int i = 0; i < distCount; i++)
            {
                WriteU16(stream, Distances[i].TargetNode);
                WriteU16(stream, Distances[i].Distance);
            }

            if (Connections != null && Connections.Length > 0)
            {
                stream.Write(Connections, 0, Math.Min(Connections.Length, nodeCount * nodeCount));
            }
            else
            {
                stream.Write(new byte[nodeCount * nodeCount], 0, nodeCount * nodeCount);
            }

            if (TrailingPadding != null && TrailingPadding.Length > 0)
            {
                stream.Write(TrailingPadding, 0, TrailingPadding.Length);
            }
        }

        private static void WriteU16(Stream s, ushort v)
        {
            byte[] b = BitConverter.GetBytes(v);
            s.Write(b, 0, 2);
        }

        private static void WriteU32(Stream s, uint v)
        {
            byte[] b = BitConverter.GetBytes(v);
            s.Write(b, 0, 4);
        }

        #endregion

        #region links and routing

        /// <summary>
        /// ordered (target, distance) pairs belonging to one node, exactly as stored
        /// </summary>
        public List<RtpDistanceEntry> GetNodeEntries(ushort node)
        {
            List<RtpDistanceEntry> r = new List<RtpDistanceEntry>();
            if (node >= Nodes.Count) return r;
            int dti = Nodes[node].DistanceTableIndex;
            int cc = Nodes[node].ConnectionCount;
            for (int i = 0; i < cc; i++)
            {
                int idx = dti + i;
                if (idx < 0 || idx >= Distances.Count) break;
                r.Add(Distances[idx]);
            }
            return r;
        }

        /// <summary>
        /// rebuilds DistanceTableIndex/ConnectionCount from the per-node entry lists,
        /// keeping each node's internal order intact
        /// </summary>
        private void FlattenFromLocalLists(List<List<RtpDistanceEntry>> local)
        {
            Distances.Clear();
            ushort running = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].DistanceTableIndex = running;
                Nodes[i].ConnectionCount = (ushort)local[i].Count;
                for (int k = 0; k < local[i].Count; k++)
                {
                    Distances.Add(local[i][k]);
                    running++;
                }
            }
        }

        private static ushort ComputeEdgeDistance(RtpNodeRecord a, RtpNodeRecord b)
        {
            float dx = a.GameX - b.GameX;
            float dy = a.GameY - b.GameY;
            float dz = a.GameZ - b.GameZ;
            double len = Math.Sqrt(dx * dx + dy * dy + dz * dz) / 10.0;
            long v = (long)Math.Round(len);
            if (v < 1) v = 1;
            if (v > ushort.MaxValue) v = ushort.MaxValue;
            return (ushort)v;
        }

        /// <summary>
        /// two-way link between two nodes using the real 3D distance, then full routing recompute
        /// </summary>
        public void LinkNodes(int a, int b)
        {
            if (a == b || a < 0 || b < 0 || a >= Nodes.Count || b >= Nodes.Count) return;
            List<List<RtpDistanceEntry>> local = new List<List<RtpDistanceEntry>>();
            for (int i = 0; i < Nodes.Count; i++) local.Add(GetNodeEntries((ushort)i));

            if (!local[a].Any(x => x.TargetNode == b))
            {
                RtpDistanceEntry e = new RtpDistanceEntry();
                e.TargetNode = (ushort)b;
                e.Distance = ComputeEdgeDistance(Nodes[a], Nodes[b]);
                local[a].Add(e);
            }
            if (!local[b].Any(x => x.TargetNode == a))
            {
                RtpDistanceEntry e = new RtpDistanceEntry();
                e.TargetNode = (ushort)a;
                e.Distance = ComputeEdgeDistance(Nodes[b], Nodes[a]);
                local[b].Add(e);
            }

            FlattenFromLocalLists(local);
            RebuildRoutingMatrix();
        }

        /// <summary>
        /// removes the two-way link between two nodes, then full routing recompute
        /// </summary>
        public void UnlinkNodes(int a, int b)
        {
            if (a < 0 || b < 0 || a >= Nodes.Count || b >= Nodes.Count) return;
            List<List<RtpDistanceEntry>> local = new List<List<RtpDistanceEntry>>();
            for (int i = 0; i < Nodes.Count; i++) local.Add(GetNodeEntries((ushort)i));

            local[a].RemoveAll(x => x.TargetNode == b);
            local[b].RemoveAll(x => x.TargetNode == a);

            FlattenFromLocalLists(local);
            RebuildRoutingMatrix();
        }

        /// <summary>
        /// Floyd-Warshall over the link graph; writes the next-hop matrix the game uses:
        /// Connections[i*N+j] = neighbor of i that leads toward j (self when unreachable/self)
        /// </summary>
        public void RebuildRoutingMatrix()
        {
            int n = Nodes.Count;
            if (n == 0)
            {
                Connections = new byte[0];
                return;
            }

            const double INF = double.MaxValue;
            double[,] cost = new double[n, n];
            int[,] next = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    cost[i, j] = INF;
                    next[i, j] = i;
                }
                cost[i, i] = 0;
                next[i, i] = i;
            }

            // map every global distance entry back to its owning node via DTI/CC blocks
            int[] owner = new int[Distances.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                int dti = Nodes[i].DistanceTableIndex;
                int cc = Nodes[i].ConnectionCount;
                for (int k = 0; k < cc; k++)
                {
                    int idx = dti + k;
                    if (idx >= 0 && idx < owner.Length) owner[idx] = i;
                }
            }

            for (int idx = 0; idx < Distances.Count; idx++)
            {
                RtpDistanceEntry e = Distances[idx];
                int src = owner[idx];
                if (e.TargetNode >= n || e.TargetNode == src) continue;
                double w = e.Distance;
                if (w < cost[src, e.TargetNode])
                {
                    cost[src, e.TargetNode] = w;
                    next[src, e.TargetNode] = e.TargetNode;
                }
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    if (cost[i, k] == INF) continue;
                    for (int j = 0; j < n; j++)
                    {
                        if (cost[k, j] == INF) continue;
                        double through = cost[i, k] + cost[k, j];
                        if (through < cost[i, j])
                        {
                            cost[i, j] = through;
                            next[i, j] = next[i, k];
                        }
                    }
                }
            }

            Connections = new byte[n * n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Connections[i * n + j] = (byte)next[i, j];
                }
            }
        }

        #endregion

        #region add/remove nodes

        public ushort AddNewNode(byte initType)
        {
            if (Nodes.Count >= Consts.AmountLimitRTP)
            {
                return ushort.MaxValue;
            }

            RtpNodeRecord n = new RtpNodeRecord();
            // spawn at origin; user moves it into place afterwards
            n.FileX = 0f;
            n.FileY = 0f;
            n.FileZ = 0f;
            n.DistanceTableIndex = 0;
            n.ConnectionCount = 0;
            Nodes.Add(n);

            RebuildRoutingMatrix();
            IdForNewNode = (ushort)(Nodes.Count - 1);
            LastAddedNodeID = IdForNewNode;
            return IdForNewNode;
        }

        /// <summary>
        /// Ctrl+D duplicate for route nodes: clones the position, offsets it a
        /// little so both stay visible, and links the clone to its source with
        /// a real two-way edge (distance + routing recomputed).
        /// Only X/Y/Z are copied - never DistanceTableIndex/ConnectionCount,
        /// those belong exclusively to the source node.
        /// </summary>
        public ushort DuplicateNode(ushort sourceId)
        {
            if (sourceId >= Nodes.Count || Nodes.Count >= Consts.AmountLimitRTP)
            {
                return ushort.MaxValue;
            }

            RtpNodeRecord src = Nodes[sourceId];
            ushort newId = AddNewNode(0);
            if (newId == ushort.MaxValue || newId == sourceId)
            {
                return ushort.MaxValue;
            }

            RtpNodeRecord dst = Nodes[newId];
            dst.FileX = src.GameX + 300f;   // 3 editor units aside, keeps the link line readable
            dst.GameY = src.GameY;
            dst.GameZ = src.GameZ;

            LinkNodes(newId, sourceId);
            return newId;
        }

        public ushort LastAddedNodeID = 0;

        private void RemoveNodeInternal(int index)
        {
            if (index < 0 || index >= Nodes.Count) return;

            // snapshot per-node entry lists BEFORE touching anything
            List<List<RtpDistanceEntry>> local = new List<List<RtpDistanceEntry>>();
            for (int i = 0; i < Nodes.Count; i++) local.Add(GetNodeEntries((ushort)i));

            Nodes.RemoveAt(index);
            local.RemoveAt(index);

            foreach (List<RtpDistanceEntry> list in local)
            {
                list.RemoveAll(x => x.TargetNode == index);
                for (int k = 0; k < list.Count; k++)
                {
                    if (list[k].TargetNode > index)
                    {
                        RtpDistanceEntry e = list[k];
                        e.TargetNode--;
                        list[k] = e;
                    }
                }
            }

            FlattenFromLocalLists(local);
            RebuildRoutingMatrix();
        }

        public void RemoveNodeID(ushort ID)
        {
            if (ID >= Nodes.Count) return;
            RemoveNodeInternal(ID);
            SyncTreeNodesToKeys();
        }

        private ushort AddNewNodeID(byte initType)
        {
            return AddNewNode(initType);
        }

        private void RebuildAfterStructureChange()
        {
            // the new/removed node changes only the matrix dimensions; the distance
            // table edges are already up to date, so just recompute the next-hop view
            RebuildRoutingMatrix();
        }

        #endregion

        #region tree sync

        public void SyncTreeNodesToKeys()
        {
            if (DataBase.NodeRTP == null) return;
            TreeNodeCollection nodes = DataBase.NodeRTP.Nodes;
            List<ushort> keys = new List<ushort>();
            for (ushort i = 0; i < Nodes.Count; i++) keys.Add(i);

            bool same = nodes.Count == keys.Count;
            if (same)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    object o = nodes[i];
                    if (!(o is Object3D obj) || obj.ObjLineRef != keys[i]) { same = false; break; }
                }
            }
            if (same) return;

            nodes.Clear();
            foreach (ushort k in keys)
            {
                Object3D o = Object3D.CreateNewInstance(GroupType.RTP, k);
                nodes.Add(o);
            }
            DataBase.NodeRTP.Expand();
        }

        private string GetNodeTextInternal(ushort ID)
        {
            if (ID >= Nodes.Count) return "?";
            return "Node " + ID + " [" + Nodes[ID].ConnectionCount + " links]";
        }

        private Color GetNodeColorInternal(ushort ID)
        {
            return Globals.NodeColorRTP;
        }

        #endregion

        #region move methods

        private Vector3 GetNodePos_ToCamera(ushort ID)
        {
            if (ID >= Nodes.Count) return Vector3.Zero;
            Vector3 position = new Vector3(
                Nodes[ID].GameX / UnitScale,
                Nodes[ID].GameY / UnitScale,
                Nodes[ID].GameZ / UnitScale);
            Utils.ToCameraCheckValue(ref position);
            return position;
        }

        private Vector3[] GetNodePostion_ToMove(ushort ID)
        {
            Vector3[] pos = new Vector3[1];
            if (ID < Nodes.Count)
            {
                pos[0] = new Vector3(Nodes[ID].GameX, Nodes[ID].GameY, Nodes[ID].GameZ);
            }
            else
            {
                pos[0] = Vector3.Zero;
            }
            Utils.ToMoveCheckLimits(ref pos);
            return pos;
        }

        private void SetNodePostion_ToMove(ushort ID, Vector3[] value)
        {
            if (value == null || value.Length < 1) return;
            if (ID >= Nodes.Count) return;

            Utils.ToMoveCheckLimits(ref value);

            Nodes[ID].GameX = value[0].X;
            Nodes[ID].GameY = value[0].Y;
            Nodes[ID].GameZ = value[0].Z;
        }

        #endregion

        #region GL methods

        private Vector3 GetNodePositionGL(ushort ID)
        {
            if (ID >= Nodes.Count) return Vector3.Zero;
            return new Vector3(
                Nodes[ID].GameX / UnitScale,
                Nodes[ID].GameY / UnitScale,
                Nodes[ID].GameZ / UnitScale);
        }

        /// <summary>
        /// all link segments in GL scale, built from the distance table
        /// </summary>
        public List<Vector3[]> GetLinkSegmentsGL()
        {
            List<Vector3[]> segs = new List<Vector3[]>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                ushort dti = Nodes[i].DistanceTableIndex;
                ushort cc = Nodes[i].ConnectionCount;
                for (int k = 0; k < cc; k++)
                {
                    int idx = dti + k;
                    if (idx >= Distances.Count) break;
                    ushort t = Distances[idx].TargetNode;
                    if (t >= Nodes.Count || t <= i) continue; // draw each pair once, from lower index
                    segs.Add(new Vector3[]
                    {
                        new Vector3(Nodes[i].GameX / UnitScale, Nodes[i].GameY / UnitScale, Nodes[i].GameZ / UnitScale),
                        new Vector3(Nodes[t].GameX / UnitScale, Nodes[t].GameY / UnitScale, Nodes[t].GameZ / UnitScale)
                    });
                }
            }
            return segs;
        }

        #endregion

        #region property methods wiring

        private void WireNodeMethods()
        {
            Methods.ReturnLine = delegate (ushort ID)
            {
                if (ID >= Nodes.Count) return new byte[16];
                return Nodes[ID].Raw;
            };
            Methods.SetLine = delegate (ushort ID, byte[] value)
            {
                if (ID >= Nodes.Count || value == null) return;
                int len = Math.Min(16, value.Length);
                Array.Copy(value, Nodes[ID].Raw, len);
            };

            Methods.ReturnPosX = delegate (ushort ID) { return ID < Nodes.Count ? Nodes[ID].GameX : 0f; };
            Methods.SetPosX = delegate (ushort ID, float v) { if (ID < Nodes.Count) Nodes[ID].GameX = v; };
            Methods.ReturnPosY = delegate (ushort ID) { return ID < Nodes.Count ? Nodes[ID].GameY : 0f; };
            Methods.SetPosY = delegate (ushort ID, float v) { if (ID < Nodes.Count) Nodes[ID].GameY = v; };
            Methods.ReturnPosZ = delegate (ushort ID) { return ID < Nodes.Count ? Nodes[ID].GameZ : 0f; };
            Methods.SetPosZ = delegate (ushort ID, float v) { if (ID < Nodes.Count) Nodes[ID].GameZ = v; };

            Methods.ReturnDistanceTableIndex = delegate (ushort ID) { return ID < Nodes.Count ? Nodes[ID].DistanceTableIndex : (ushort)0; };
            Methods.SetDistanceTableIndex = delegate (ushort ID, ushort v) { if (ID < Nodes.Count) Nodes[ID].DistanceTableIndex = v; };

            Methods.ReturnConnectionCount = delegate (ushort ID) { return ID < Nodes.Count ? Nodes[ID].ConnectionCount : (ushort)0; };
            Methods.SetConnectionCount = delegate (ushort ID, ushort v) { if (ID < Nodes.Count) Nodes[ID].ConnectionCount = v; };

            Methods.ReturnLinksSummary = delegate (ushort ID)
            {
                if (ID >= Nodes.Count) return "";
                var entries = GetNodeEntries(ID);
                if (entries.Count == 0) return "(no links)";
                return string.Join(", ", entries.Select(x => "->" + x.TargetNode + " (" + x.Distance + ")").ToArray());
            };

            Methods.ConnectTo = delegate (ushort ID, ushort target)
            {
                if (ID < Nodes.Count && target < Nodes.Count)
                {
                    LinkNodes(ID, target);
                }
            };

            Methods.DisconnectFrom = delegate (ushort ID, ushort target)
            {
                if (ID < Nodes.Count && target < Nodes.Count)
                {
                    UnlinkNodes(ID, target);
                }
            };

            Methods.ReturnLinkedIds = delegate (ushort ID)
            {
                List<ushort> ids = new List<ushort>();
                if (ID < Nodes.Count)
                {
                    foreach (var e in GetNodeEntries(ID))
                    {
                        ids.Add(e.TargetNode);
                    }
                }
                return ids.ToArray();
            };
        }

        #endregion
    }
}
