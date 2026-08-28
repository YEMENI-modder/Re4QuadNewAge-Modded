using System;
using System.Diagnostics;
using NsCamera;
using OpenTK;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// Per-frame viewport helpers: selection highlight breathing pulse,
    /// toast-message timing and the instant focus-on-selection jump.
    /// Tick() is called once per rendered frame from GlControl_Paint.
    /// </summary>
    internal static class ViewAnim
    {
        private static readonly Stopwatch clock = Stopwatch.StartNew();
        private static double lastTickMs = 0;
        private static double toastUntilMs = -1.0;

        private static bool pulseBaseCaptured = false;
        private static Vector4 pulseBase = new Vector4(1f, 1f, 0f, 1f);

        public static Camera Cam { get; set; }
        public static float DeltaSeconds { get; private set; }

        public static string ToastText { get; private set; } = "";
        public static bool ToastVisible { get { return clock.Elapsed.TotalMilliseconds < toastUntilMs; } }

        public static void ShowToast(string message)
        {
            ToastText = message ?? "";
            toastUntilMs = clock.Elapsed.TotalMilliseconds + 2200.0;
        }

        /// <summary>
        /// Instantly move the camera so the given world point is nicely framed.
        /// FLY mode teleports the camera position along its view direction;
        /// ORBIT mode snaps the orbit distance closer.
        /// </summary>
        public static void FocusNow(Vector3 pivot)
        {
            if (Cam == null) return;

            if (Cam.isOrbitCamera())
            {
                Cam.SetOrbitDistance(MathHelper.Clamp(Cam.OrbitDistance * 0.35f, 30f, 250f));
                return;
            }

            Vector3 front = Cam.Front;
            if (front.LengthSquared < 1e-6f) front = -Vector3.UnitZ;
            front.Normalize();

            float dist = (Cam.Position - pivot).Length;
            float targetDist = MathHelper.Clamp(dist * 0.35f, 8f, 60f);
            Cam.Position = pivot - front * targetDist;
        }

        public static void Tick()
        {
            double now = Now();

            float dt = (float)(now - lastTickMs) / 1000f;
            lastTickMs = now;
            if (dt <= 0f) dt = 1e-4f;
            else if (dt > 0.1f) dt = 0.1f;
            DeltaSeconds = dt;

            TickSelectionPulse(now);
        }

        private static void TickSelectionPulse(double now)
        {
            Vector4 current = Globals.GL_ColorSelected;
            if (!pulseBaseCaptured)
            {
                if (current == default(Vector4)) return;
                pulseBase = current;
                pulseBaseCaptured = true;
            }
            // gentle breathing (~0.9 Hz) between 90% and 100% brightness
            float pulse = 0.90f + 0.10f * (float)Math.Sin(now * 0.0056);
            Globals.GL_ColorSelected = new Vector4(
                pulseBase.X * pulse,
                pulseBase.Y * pulse,
                pulseBase.Z * pulse,
                pulseBase.W);
        }

        private static double Now() { return clock.Elapsed.TotalMilliseconds; }
    }
}
