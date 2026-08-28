using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// Viewport overlay decoration rendered on top of the scene every frame:
    /// a miniature world-axis indicator in the bottom-right corner showing
    /// where X/Y/Z point relative to the current camera, with standard
    /// red/green/blue color coding and depth fading.
    ///
    /// Draws through one tiny position-only shader, mirroring the proven
    /// Gizmo rendering setup (row-vector matrices uploaded with transpose=true).
    /// </summary>
    public static class SelectionOverlay
    {
        private static int program = 0;
        private static int vao = 0;
        private static int vbo = 0;
        private static int uMvpLocation = -1;
        private static int uColorLocation = -1;
        private static readonly List<float> verts = new List<float>(1024);

        public static bool AxisWidgetEnabled = true;

        // ------------------------------------------------------------------
        // GL plumbing

        private static void EnsureOverlayGlObjects()
        {
            if (program != 0) return;

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

            program = GL.CreateProgram();
            GL.AttachShader(program, vs);
            GL.AttachShader(program, fs);
            GL.LinkProgram(program);
            GL.DetachShader(program, vs);
            GL.DetachShader(program, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            uMvpLocation = GL.GetUniformLocation(program, "uMVP");
            uColorLocation = GL.GetUniformLocation(program, "uColor");

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private static void BeginOverlayState()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.UseProgram(program);
        }

        private static void EndOverlayState(int width, int height)
        {
            GL.UseProgram(0);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Viewport(0, 0, width, height);
        }

        // ------------------------------------------------------------------
        // geometry emitters

        private static void EmitTri(Vector3 a, Vector3 b, Vector3 c)
        {
            verts.Add(a.X); verts.Add(a.Y); verts.Add(a.Z);
            verts.Add(b.X); verts.Add(b.Y); verts.Add(b.Z);
            verts.Add(c.X); verts.Add(c.Y); verts.Add(c.Z);
        }

        private static void EmitArrow3D(Vector3 origin, Vector3 dir, float len, float shaftW, float headLen, float headW)
        {
            Vector3 front = dir.Normalized();
            Vector3 helper = Math.Abs(Vector3.Dot(front, Vector3.UnitY)) > 0.95f ? Vector3.UnitZ : Vector3.UnitY;
            Vector3 u = Vector3.Cross(front, helper).Normalized();
            Vector3 v = Vector3.Cross(u, front);

            float shaftLen = len - headLen;
            Vector3 ringC = origin + front * shaftLen;
            Vector3 apex = origin + front * len;

            // shaft prism
            Vector3 a0 = origin - u * shaftW - v * shaftW, b0 = origin + u * shaftW - v * shaftW;
            Vector3 c0 = origin + u * shaftW + v * shaftW, d0 = origin - u * shaftW + v * shaftW;
            Vector3 a1 = ringC - u * shaftW - v * shaftW, b1 = ringC + u * shaftW - v * shaftW;
            Vector3 c1 = ringC + u * shaftW + v * shaftW, d1 = ringC - u * shaftW + v * shaftW;
            EmitTri(a0, a1, b1); EmitTri(a0, b1, b0);
            EmitTri(b0, b1, c1); EmitTri(b0, c1, c0);
            EmitTri(c0, c1, d1); EmitTri(c0, d1, d0);
            EmitTri(d0, d1, a1); EmitTri(d0, a1, a0);
            EmitTri(a0, c0, b0); EmitTri(a0, d0, c0);
            EmitTri(a1, b1, c1); EmitTri(a1, c1, d1);

            // head pyramid
            Vector3 r0 = ringC - u * headW - v * headW;
            Vector3 r1 = ringC + u * headW - v * headW;
            Vector3 r2 = ringC + u * headW + v * headW;
            Vector3 r3 = ringC - u * headW + v * headW;
            EmitTri(r0, r1, apex); EmitTri(r1, r2, apex);
            EmitTri(r2, r3, apex); EmitTri(r3, r0, apex);
            EmitTri(r0, r2, r1); EmitTri(r0, r3, r2);
        }

        // ------------------------------------------------------------------
        // mini axis widget

        public static void RenderAxisWidget(Matrix4 camMtx, int width, int height)
        {
            if (!AxisWidgetEnabled || width < 140 || height < 140) return;

            const int size = 92;
            const int margin = 10;

            EnsureOverlayGlObjects();
            BeginOverlayState();

            GL.Viewport(width - size - margin, margin, size, size);

            Matrix4 ortho = Matrix4.CreateOrthographic(3.0f, 3.0f, -10f, 10f);
            GL.UniformMatrix4(uMvpLocation, true, ref ortho);

            // world axis directions expressed in view space (row-vector math)
            Vector3 dx = TransformDir(camMtx, Vector3.UnitX);
            Vector3 dy = TransformDir(camMtx, Vector3.UnitY);
            Vector3 dz = TransformDir(camMtx, Vector3.UnitZ);

            // painter order: axes pointing away from the viewer first
            int[] order = SortAxesByDepth(dx, dy, dz);
            Vector3[] dirs = { dx, dy, dz };
            Vector4[] cols =
            {
                new Vector4(0.95f, 0.28f, 0.28f, 1f),   // X red
                new Vector4(0.42f, 0.90f, 0.32f, 1f),   // Y green
                new Vector4(0.32f, 0.52f, 0.98f, 1f),   // Z blue
            };

            verts.Clear();

            for (int k = 0; k < 3; k++)
            {
                int i = order[k];
                Vector3 dir = dirs[i];
                float lenSq = dir.LengthSquared;
                if (lenSq < 1e-8f) continue;
                dir.Normalize();

                bool towardViewer = dir.Z > 0f;         // camera looks down -Z
                Vector4 col = cols[i];
                if (!towardViewer)
                {
                    col = new Vector4(col.X * 0.50f, col.Y * 0.50f, col.Z * 0.55f, 0.85f);
                }
                GL.Uniform4(uColorLocation, col);

                int start = verts.Count / 3;
                EmitArrow3D(Vector3.Zero, dir, 0.86f, 0.052f, 0.22f, 0.13f);
                int count = verts.Count / 3 - start;

                // per-axis color needs its own draw call
                FlushRange(start, count);
            }

            // center hub
            GL.Uniform4(uColorLocation, new Vector4(0.92f, 0.94f, 0.97f, 1f));
            int hs = verts.Count / 3;
            EmitArrow3D(Vector3.Zero, Vector3.UnitY, 0.001f, 0.085f, 0.0005f, 0.085f);
            FlushRange(hs, verts.Count / 3 - hs);

            EndOverlayState(width, height);
        }

        private static Vector3 TransformDir(Matrix4 m, Vector3 v)
        {
            Vector4 r = new Vector4(v, 0f) * m;
            return r.Xyz;
        }

        private static int[] SortAxesByDepth(Vector3 dx, Vector3 dy, Vector3 dz)
        {
            // ascending Z: most negative (farthest) first
            int[] order = { 0, 1, 2 };
            float[] z = { dx.Z, dy.Z, dz.Z };
            for (int i = 1; i < 3; i++)
            {
                int key = order[i];
                float kz = z[key];
                int j = i - 1;
                while (j >= 0 && z[order[j]] > kz)
                {
                    order[j + 1] = order[j];
                    j--;
                }
                order[j + 1] = key;
            }
            return order;
        }

        //reused GPU upload buffer - avoids per-frame allocations (GC stutter)
        private static float[] uploadArr = null;

        private static void FlushRange(int startVertex, int vertexCount)
        {
            if (vertexCount <= 0) return;
            if (uploadArr == null || uploadArr.Length < verts.Count)
            {
                uploadArr = new float[Math.Max(1024, verts.Count * 2)];
            }
            verts.CopyTo(uploadArr);
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Count * sizeof(float)), uploadArr, BufferUsageHint.DynamicDraw);
            GL.DrawArrays(PrimitiveType.Triangles, startVertex, vertexCount);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
