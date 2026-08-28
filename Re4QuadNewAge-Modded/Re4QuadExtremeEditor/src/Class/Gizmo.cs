using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Re4QuadExtremeEditor.src.Class.Shaders;
using Re4QuadExtremeEditor.src.Class.TreeNodeObj;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// Blender-style translate gizmo: X/Y/Z axis arrows plus a center
    /// square for free camera-plane movement. Rendered on top of the scene,
    /// picked in screen space, applied through Object3D.SetObjPosition_ToCamera.
    /// </summary>
    public static class Gizmo
    {
        public static bool Enabled = false;

        public static Action TransformApplied = null;

        /// <summary>
        /// Movement snapping grid in camera-scale world units.
        /// 0 disables snapping. Toggled with the 1 / 2 / 3 / 0 keys
        /// (1.0 / 0.1 / 0.01 / off); Shift is NOT required while dragging -
        /// the quantization applies whenever a step is active.
        /// </summary>
        public static float SnapStep = 0f;

        private const int PartNone = -1;
        private const int PartX = 0;
        private const int PartY = 1;
        private const int PartZ = 2;
        private const int PartCenter = 3;

        private static int hoverPart = PartNone;
        private static bool dragging = false;
        private static int dragPart = PartNone;

        private static Vector3 planePoint = Vector3.Zero;
        private static Vector3 planeNormal = Vector3.UnitZ;
        private static Vector3 planeHitStart = Vector3.Zero;

        private static readonly List<Object3D> dragObjects = new List<Object3D>();
        private static readonly List<Vector3> dragStartPositions = new List<Vector3>();

        private static readonly Vector4 ColX = new Vector4(0.95f, 0.25f, 0.25f, 1f);
        private static readonly Vector4 ColY = new Vector4(0.45f, 0.90f, 0.35f, 1f);
        private static readonly Vector4 ColZ = new Vector4(0.35f, 0.50f, 0.95f, 1f);
        private static readonly Vector4 ColCenter = new Vector4(0.95f, 0.95f, 0.95f, 1f);

        public static bool IsDragging { get { return dragging; } }

        /// <summary>
        /// World-space center of the current selection (gizmo pivot).
        /// Returns false when nothing usable is selected.
        /// Independent of the gizmo enable flag - used by the F jump shortcut.
        /// </summary>
        public static bool TryGetPivot(out Vector3 pivot)
        {
            pivot = Vector3.Zero;
            if (!HasSelection()) return false;
            pivot = GetPivot();
            return true;
        }

        // ---------------- per-part hover/drag glow animation ----------------

        private static readonly float[] partGlow = new float[4];
        private static readonly System.Diagnostics.Stopwatch glowClock = System.Diagnostics.Stopwatch.StartNew();
        private static double glowLastMs = 0;

        /// <summary>
        /// Advances the smooth highlight transition for every gizmo part.
        /// Called once per rendered frame from GlControl_Paint.
        /// </summary>
        public static void Tick()
        {
            double now = glowClock.Elapsed.TotalMilliseconds;
            float dt = (float)(now - glowLastMs) / 1000f;
            glowLastMs = now;
            if (dt <= 0f || dt > 0.25f) dt = 0.016f;

            for (int i = 0; i < partGlow.Length; i++)
            {
                bool hot = hoverPart == i || (dragging && dragPart == i);
                float target = hot ? 1f : 0f;
                float speed = hot ? 14f : 9f;   // fast in, slower out
                partGlow[i] += (target - partGlow[i]) * Math.Min(1f, speed * dt);
            }
        }

        // temporary diagnostics
        internal static readonly string LogPath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            "gizmo_debug_log.txt");

        internal static void Log(string msg)
        {
            try { System.IO.File.AppendAllText(LogPath, DateTime.Now.ToString("HH:mm:ss.fff ") + msg + "\r\n"); }
            catch { }
        }

        // ---------------- selection helpers ----------------

        private static Vector3 GetPivot()
        {
            Vector3 acc = Vector3.Zero;
            int n = 0;
            var sel = Re4QuadExtremeEditor.src.DataBase.SelectedNodes;
            if (sel != null)
            {
                foreach (KeyValuePair<int, TreeNode> kv in sel)
                {
                    Object3D obj = kv.Value as Object3D;
                    if (obj != null)
                    {
                        acc += obj.GetObjPosition_ToCamera();
                        n++;
                    }
                }
            }
            return n > 0 ? acc / n : Vector3.Zero;
        }

        // ---------------- math helpers ----------------

        private static bool BuildRay(int mx, int my, int w, int h, Vector3 camPos, Vector3 camFront, out Vector3 ro, out Vector3 rd)
        {
            ro = camPos;
            rd = -Vector3.UnitZ;
            if (w <= 1 || h <= 1) return false;

            float nx = 2f * mx / w - 1f;
            float ny = 1f - 2f * my / h;

            Vector3 front = camFront;
            if (front.LengthSquared < 1e-8f) front = -Vector3.UnitZ;
            front.Normalize();

            // build an orthonormal basis from the view direction (no roll)
            Vector3 right = Vector3.Cross(front, Vector3.UnitY);
            if (right.LengthSquared < 1e-6f) right = Vector3.Cross(front, Vector3.UnitZ);
            right.Normalize();
            Vector3 up = Vector3.Cross(right, front);

            float tanV = (float)Math.Tan(Globals.FOV * Math.PI / 360.0);
            float aspect = (float)w / h;

            rd = front + right * (nx * tanV * aspect) + up * (ny * tanV);
            if (rd.LengthSquared < 1e-10f) return false;
            rd.Normalize();
            return true;
        }

        private static bool WorldToScreen(Vector3 p, Matrix4 vp, int w, int h, out PointF s)
        {
            s = new PointF();
            Vector4 c = new Vector4(p, 1f) * vp;
            if (c.W <= 1e-6f) return false;
            s.X = (c.X / c.W * 0.5f + 0.5f) * w;
            s.Y = (1f - (c.Y / c.W * 0.5f + 0.5f)) * h;
            return true;
        }

        private struct PointF { public float X, Y; }

        private static float DistToSegment(float px, float py, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax, dy = by - ay;
            float lenSq = dx * dx + dy * dy;
            float t = 0f;
            if (lenSq > 1e-6f)
            {
                t = ((px - ax) * dx + (py - ay) * dy) / lenSq;
                if (t < 0f) t = 0f; else if (t > 1f) t = 1f;
            }
            float cx = ax + dx * t - px;
            float cy = ay + dy * t - py;
            return (float)Math.Sqrt(cx * cx + cy * cy);
        }

        private static bool RayPlaneHit(Vector3 ro, Vector3 rd, Vector3 point, Vector3 normal, out Vector3 hit)
        {
            hit = Vector3.Zero;
            float denom = Vector3.Dot(normal, rd);
            if (Math.Abs(denom) < 1e-6f) return false;
            float t = Vector3.Dot(point - ro, normal) / denom;
            if (t < 0f) return false;
            hit = ro + rd * t;
            return true;
        }

        private static Vector4 Highlight(Vector4 baseColor)
        {
            return new Vector4(
                baseColor.X + (1f - baseColor.X) * 0.55f,
                baseColor.Y + (1f - baseColor.Y) * 0.55f,
                baseColor.Z + (1f - baseColor.Z) * 0.55f,
                1f);
        }

        // rotation matrices mapping the local +X shaft onto each world axis
        // (row-vector convention: world = local * M, so local X -> first row)
        private static Matrix4 RotForAxis(int axis)
        {
            switch (axis)
            {
                case PartY:
                    return new Matrix4(0f, 1f, 0f, 0f,
                                       -1f, 0f, 0f, 0f,
                                       0f, 0f, 1f, 0f,
                                       0f, 0f, 0f, 1f);
                case PartZ:
                    return new Matrix4(0f, 0f, 1f, 0f,
                                       0f, 1f, 0f, 0f,
                                       -1f, 0f, 0f, 0f,
                                       0f, 0f, 0f, 1f);
                default:
                    return Matrix4.Identity;
            }
        }

        private static Vector3 AxisDir(int part)
        {
            if (part == PartX) return Vector3.UnitX;
            if (part == PartY) return Vector3.UnitY;
            if (part == PartZ) return Vector3.UnitZ;
            return Vector3.Zero;
        }

        // ---------------- picking ----------------

        private static int PickPart(int mx, int my, int w, int h, Matrix4 view, Matrix4 proj)
        {
            Vector3 pivot = GetPivot();
            Matrix4 vp = view * proj;

            PointF cs;
            if (!WorldToScreen(pivot, vp, w, h, out cs))
            {
                return PartNone;
            }

            // center square first (priority like Blender)
            float len = L(w, h, view, proj, pivot);
            float dist = DistanceToCam(view, pivot);
            float tanHalf = (float)Math.Tan(Globals.FOV * Math.PI / 360.0);
            float pxPerWorld = (h * 0.5f) / Math.Max(0.001f, dist * tanHalf);
            float screenHalf = len * 0.07f * pxPerWorld;
            if (screenHalf < 8f) screenHalf = 8f;
            if (screenHalf > 80f) screenHalf = 80f;
            if (Math.Abs(mx - cs.X) <= screenHalf && Math.Abs(my - cs.Y) <= screenHalf)
            {
                return PartCenter;
            }

            float best = 12f; // pixel threshold for axis lines
            int found = PartNone;
            for (int axis = PartX; axis <= PartZ; axis++)
            {
                Vector3 tipW = pivot + AxisDir(axis) * len;
                PointF ts;
                if (!WorldToScreen(tipW, vp, w, h, out ts)) continue;
                float d = DistToSegment(mx, my, cs.X, cs.Y, ts.X, ts.Y);
                if (d < best)
                {
                    best = d;
                    found = axis;
                }
            }
            return found;
        }

        private static float L(int w, int h, Matrix4 view, Matrix4 proj, Vector3 pivot)
        {
            float dist = DistanceToCam(view, pivot);
            float len = dist * 0.15f;
            if (len < 1f) len = 1f;
            return len;
        }

        private static float DistanceToCam(Matrix4 view, Vector3 pivot)
        {
            // distance from camera (view space origin) to pivot
            Vector4 v = new Vector4(pivot, 1f) * view;
            return v.Z < 0f ? -v.Z : v.Z;
        }

        // ---------------- public interaction API ----------------

        internal static int dragLogCount = 0;

        public static bool TryBeginDrag(int mx, int my, int w, int h, Matrix4 view, Matrix4 proj,
            Vector3 camPos, Vector3 camFront)
        {
            if (!Enabled || !HasSelection()) { Log(string.Format("Begin skip enabled={0} sel={1}", Enabled, HasSelection())); return false; }

            int part = PickPart(mx, my, w, h, view, proj);
            if (part == PartNone) { Log("Begin: no part under cursor"); return false; }

            Vector3 ro, rd;
            if (!BuildRay(mx, my, w, h, camPos, camFront, out ro, out rd)) { Log("Begin: ray fail"); return false; }

            Vector3 pivot = GetPivot();

            if (part == PartCenter)
            {
                planeNormal = camFront;
                if (planeNormal.LengthSquared < 1e-6f) planeNormal = -Vector3.UnitZ;
                planeNormal.Normalize();
            }
            else
            {
                Vector3 axis = AxisDir(part);
                Vector3 n = camFront - Vector3.Dot(camFront, axis) * axis;
                if (n.LengthSquared < 1e-6f) n = Vector3.UnitY - Vector3.Dot(Vector3.UnitY, axis) * axis;
                if (n.LengthSquared < 1e-6f) n = Vector3.UnitZ;
                planeNormal = n.Normalized();
            }
            planePoint = pivot;

            Vector3 hit;
            if (!RayPlaneHit(ro, rd, planePoint, planeNormal, out hit)) { Log("Begin: plane fail"); return false; }
            planeHitStart = hit;

            dragObjects.Clear();
            dragStartPositions.Clear();
            var sel = Re4QuadExtremeEditor.src.DataBase.SelectedNodes;
            if (sel != null)
            {
                foreach (KeyValuePair<int, TreeNode> kv in sel)
                {
                    Object3D obj = kv.Value as Object3D;
                    if (obj != null)
                    {
                        dragObjects.Add(obj);
                        dragStartPositions.Add(obj.GetObjPosition_ToCamera());
                    }
                }
            }
            if (dragObjects.Count == 0) { Log("Begin: zero objects"); return false; }

            dragging = true;
            dragPart = part;
            hoverPart = part;
            Log(string.Format("Begin OK part={0} objs={1} pivot=({2:F2},{3:F2},{4:F2})",
                part, dragObjects.Count, planePoint.X, planePoint.Y, planePoint.Z));
            return true;
        }

        public static void UpdateDrag(int mx, int my, int w, int h, Vector3 camPos, Vector3 camFront)
        {
            if (!dragging) return;

            Vector3 ro, rd;
            if (!BuildRay(mx, my, w, h, camPos, camFront, out ro, out rd)) return;

            Vector3 hit;
            if (!RayPlaneHit(ro, rd, planePoint, planeNormal, out hit)) return;

            Vector3 delta = hit - planeHitStart;
            if (dragPart != PartCenter)
            {
                Vector3 axis = AxisDir(dragPart);
                delta = axis * Vector3.Dot(delta, axis);
            }

            // grid snapping: quantize the delta so every object lands on the grid
            if (SnapStep > 0f)
            {
                delta.X = (float)Math.Round(delta.X / SnapStep) * SnapStep;
                delta.Y = (float)Math.Round(delta.Y / SnapStep) * SnapStep;
                delta.Z = (float)Math.Round(delta.Z / SnapStep) * SnapStep;
            }

            for (int i = 0; i < dragObjects.Count; i++)
            {
                dragObjects[i].SetObjPosition_ToCamera(dragStartPositions[i] + delta);
            }
        }

        public static void EndDrag()
        {
            if (!dragging) return;
            dragging = false;
            dragPart = PartNone;

            // record the completed drag for Ctrl+Z before dropping the capture
            if (dragObjects.Count > 0)
            {
                UndoSystem.PushMove(dragObjects, dragStartPositions, delegate ()
                {
                    Vector3[] cur = new Vector3[dragObjects.Count];
                    for (int i = 0; i < cur.Length; i++)
                    {
                        cur[i] = dragObjects[i].GetObjPosition_ToCamera();
                    }
                    return cur;
                });
            }

            dragObjects.Clear();
            dragStartPositions.Clear();
            if (TransformApplied != null) TransformApplied.Invoke();
        }

        public static int UpdateHover(int mx, int my, int w, int h, Matrix4 view, Matrix4 proj)
        {
            if (!Enabled || !HasSelection())
            {
                hoverPart = PartNone;
                return PartNone;
            }
            hoverPart = PickPart(mx, my, w, h, view, proj);
            return hoverPart;
        }

        // ---------------- rendering ----------------

        private static int gizmoProgram = 0;
        private static int gizmoVao = 0;
        private static int gizmoVbo = 0;
        private static int uMvpLocation = -1;
        private static int uColorLocation = -1;
        private static readonly List<float> partVerts = new List<float>(2048);
        private static readonly List<float> allVerts = new List<float>(8192);
        //reused GPU upload buffers - avoids per-frame allocations (GC stutter)
        private static float[] uploadArr = null;
        private static readonly int[] rangeStart = new int[4];
        private static readonly int[] rangeCount = new int[4];

        private static void EnsureGizmoGlObjects()
        {
            if (gizmoProgram != 0) return;

            const string vertSrc =
                "#version 330 core\n" +
                "layout(location = 0) in vec3 aPos;\n" +
                "uniform mat4 uMVP;\n" +
                "void main(){ gl_Position = vec4(aPos, 1.0) * uMVP; }\n";

            const string fragSrc =
                "#version 330\n" +
                "uniform vec4 uColor;\n" +
                "out vec4 fragColor;\n" +
                "void main(){ fragColor = uColor; }\n";

            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertSrc);
            GL.CompileShader(vs);
            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragSrc);
            GL.CompileShader(fs);

            gizmoProgram = GL.CreateProgram();
            GL.AttachShader(gizmoProgram, vs);
            GL.AttachShader(gizmoProgram, fs);
            GL.LinkProgram(gizmoProgram);
            GL.DetachShader(gizmoProgram, vs);
            GL.DetachShader(gizmoProgram, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            uMvpLocation = GL.GetUniformLocation(gizmoProgram, "uMVP");
            uColorLocation = GL.GetUniformLocation(gizmoProgram, "uColor");

            gizmoVao = GL.GenVertexArray();
            gizmoVbo = GL.GenBuffer();
            GL.BindVertexArray(gizmoVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, gizmoVbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private static void EmitTri(List<float> list, Vector3 a, Vector3 b, Vector3 c)
        {
            list.Add(a.X); list.Add(a.Y); list.Add(a.Z);
            list.Add(b.X); list.Add(b.Y); list.Add(b.Z);
            list.Add(c.X); list.Add(c.Y); list.Add(c.Z);
        }

        private static void EmitPrism(List<float> list, Vector3 p0, Vector3 p1, Vector3 u, Vector3 v, float t)
        {
            Vector3 a0 = p0 - u * t - v * t, b0 = p0 + u * t - v * t, c0 = p0 + u * t + v * t, d0 = p0 - u * t + v * t;
            Vector3 a1 = p1 - u * t - v * t, b1 = p1 + u * t - v * t, c1 = p1 + u * t + v * t, d1 = p1 - u * t + v * t;
            EmitTri(list, a0, a1, b1); EmitTri(list, a0, b1, b0);
            EmitTri(list, b0, b1, c1); EmitTri(list, b0, c1, c0);
            EmitTri(list, c0, c1, d1); EmitTri(list, c0, d1, d0);
            EmitTri(list, d0, d1, a1); EmitTri(list, d0, a1, a0);
            EmitTri(list, a0, c0, b0); EmitTri(list, a0, d0, c0);
            EmitTri(list, a1, b1, c1); EmitTri(list, a1, c1, d1);
        }

        private static void EmitPyramid(List<float> list, Vector3[] ring, Vector3 apex)
        {
            for (int i = 0; i < 4; i++)
            {
                EmitTri(list, ring[i], ring[(i + 1) & 3], apex);
            }
            EmitTri(list, ring[0], ring[2], ring[1]);
            EmitTri(list, ring[0], ring[3], ring[2]);
        }

        private static void EmitArrow(List<float> list, Vector3 origin, Vector3 dir,
            float len, float shaftW, float headLen, float headW)
        {
            Vector3 front = dir.Normalized();
            Vector3 helper = Math.Abs(Vector3.Dot(front, Vector3.UnitY)) > 0.95f ? Vector3.UnitZ : Vector3.UnitY;
            Vector3 u = Vector3.Cross(front, helper).Normalized();
            Vector3 v = Vector3.Cross(u, front);

            float shaftLen = len - headLen;
            Vector3 ringC = origin + front * shaftLen;
            Vector3 apex = origin + front * len;

            EmitPrism(list, origin, ringC, u, v, shaftW);

            Vector3[] ring = new Vector3[]
            {
                ringC - u * headW - v * headW,
                ringC + u * headW - v * headW,
                ringC + u * headW + v * headW,
                ringC - u * headW + v * headW,
            };
            EmitPyramid(list, ring, apex);
        }

        /// <summary>
        /// Long thin reference line along the dragged axis so the movement
        /// direction stays visible even when the gizmo itself scrolls away.
        /// </summary>
        private static void EmitGuideLine(List<float> list, Vector3 origin, Vector3 dir, float len)
        {
            Vector3 front = dir.Normalized();
            Vector3 helper = Math.Abs(Vector3.Dot(front, Vector3.UnitY)) > 0.95f ? Vector3.UnitZ : Vector3.UnitY;
            Vector3 u = Vector3.Cross(front, helper).Normalized();
            Vector3 v = Vector3.Cross(u, front);
            EmitPrism(list,
                origin - front * len * 2.5f,
                origin + front * len * 4.0f,
                u, v, len * 0.006f);
        }

        private static void EmitQuad(List<float> list, Vector3 center, Vector3 u, Vector3 v, float half)
        {
            Vector3 a = center - u * half - v * half;
            Vector3 b = center + u * half - v * half;
            Vector3 c = center + u * half + v * half;
            Vector3 d = center - u * half + v * half;
            EmitTri(list, a, b, c);
            EmitTri(list, a, c, d);
        }

        private static Vector4 ColorForPart(int part)
        {
            Vector4 col;
            if (part == PartX) col = ColX;
            else if (part == PartY) col = ColY;
            else if (part == PartZ) col = ColZ;
            else col = ColCenter;

            float g = partGlow[part];
            if (g > 0.001f)
            {
                Vector4 hot = Highlight(col);
                col = new Vector4(
                    col.X + (hot.X - col.X) * g,
                    col.Y + (hot.Y - col.Y) * g,
                    col.Z + (hot.Z - col.Z) * g,
                    col.W);
            }
            return col;
        }

        public static void Render(Matrix4 view, Matrix4 proj, Vector3 camFront, Vector3 camRight, Vector3 camUp)
        {
            if (!Enabled || !HasSelection()) return;

            EnsureGizmoGlObjects();

            Vector3 pivot = GetPivot();
            float len = L(0, 0, view, proj, pivot);

            float shaftW = len * 0.022f;
            float headLen = len * 0.18f;
            float headW = len * 0.055f;
            float sqHalf = len * 0.085f;

            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.UseProgram(gizmoProgram);
            Matrix4 mvp = view * proj;
            // OpenTK stores matrices row-major; the rest of this engine uploads
            // with transpose=true (see Shader.SetMatrix4) - match that here.
            GL.UniformMatrix4(uMvpLocation, true, ref mvp);

            allVerts.Clear();
            for (int p = 0; p < 4; p++)
            {
                partVerts.Clear();
                switch (p)
                {
                    case 0:
                        EmitArrow(partVerts, pivot, Vector3.UnitX, len, shaftW, headLen, headW);
                        if (dragging && dragPart == PartX) EmitGuideLine(partVerts, pivot, Vector3.UnitX, len);
                        break;
                    case 1:
                        EmitArrow(partVerts, pivot, Vector3.UnitY, len, shaftW, headLen, headW);
                        if (dragging && dragPart == PartY) EmitGuideLine(partVerts, pivot, Vector3.UnitY, len);
                        break;
                    case 2:
                        EmitArrow(partVerts, pivot, Vector3.UnitZ, len, shaftW, headLen, headW);
                        if (dragging && dragPart == PartZ) EmitGuideLine(partVerts, pivot, Vector3.UnitZ, len);
                        break;
                    default:
                        EmitQuad(partVerts, pivot,
                            camRight.LengthSquared > 1e-6f ? camRight.Normalized() : Vector3.UnitX,
                            camUp.LengthSquared > 1e-6f ? camUp.Normalized() : Vector3.UnitY,
                            sqHalf);
                        break;
                }
                rangeStart[p] = allVerts.Count / 3;
                rangeCount[p] = partVerts.Count / 3;
                allVerts.AddRange(partVerts);
            }

            GL.BindVertexArray(gizmoVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, gizmoVbo);
            if (uploadArr == null || uploadArr.Length < allVerts.Count)
            {
                uploadArr = new float[Math.Max(8192, allVerts.Count * 2)];
            }
            allVerts.CopyTo(uploadArr);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(allVerts.Count * sizeof(float)), uploadArr, BufferUsageHint.DynamicDraw);

            for (int p = 0; p < 4; p++)
            {
                if (rangeCount[p] <= 0) continue;
                GL.Uniform4(uColorLocation, ColorForPart(p));
                GL.DrawArrays(PrimitiveType.Triangles, rangeStart[p], rangeCount[p]);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.UseProgram(0);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        private static bool HasSelection()
        {
            var sel = Re4QuadExtremeEditor.src.DataBase.SelectedNodes;
            if (sel == null) return false;
            foreach (KeyValuePair<int, TreeNode> kv in sel)
            {
                if (kv.Value is Object3D) return true;
            }
            return false;
        }
    }
}
