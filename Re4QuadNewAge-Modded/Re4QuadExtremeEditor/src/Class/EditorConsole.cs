using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// Editor action log (ported concept from Re4QuadX). Static logger bound
    /// to a RichTextBox inside the main window. Thread-safe via BeginInvoke,
    /// and buffers early messages fired before the control exists.
    /// </summary>
    public static class EditorConsole
    {
        private enum LogLevel { Info, Warning, Error }

        private static RichTextBox outputControl;
        private static readonly List<KeyValuePair<string, LogLevel>> earlyLogCache =
            new List<KeyValuePair<string, LogLevel>>();

        private static readonly Color InfoColor = Color.FromArgb(200, 206, 214);
        private static readonly Color WarningColor = Color.FromArgb(235, 195, 110);
        private static readonly Color ErrorColor = Color.FromArgb(238, 130, 130);
        private static readonly Color BackColor = Color.FromArgb(17, 20, 24);

        public static void RegisterOutputControl(RichTextBox richTextBox)
        {
            outputControl = richTextBox;
            outputControl.BackColor = BackColor;
            outputControl.BorderStyle = BorderStyle.None;
            outputControl.ReadOnly = true;
            outputControl.Font = new Font("Consolas", 9.5F);
            outputControl.Clear();

            FlushEarlyLogs();
        }

        private static void FlushEarlyLogs()
        {
            if (outputControl == null || earlyLogCache.Count == 0) return;
            foreach (var entry in earlyLogCache)
            {
                LogInternal(entry.Key, entry.Value);
            }
            earlyLogCache.Clear();
        }

        public static void Clear()
        {
            if (outputControl == null) return;
            outputControl.Clear();
        }

        public static void Log(string message) { LogInternal(message, LogLevel.Info); }
        public static void Warning(string message) { LogInternal(message, LogLevel.Warning); }
        public static void Error(string message) { LogInternal(message, LogLevel.Error); }

        private static void LogInternal(string message, LogLevel level)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (outputControl == null)
            {
                earlyLogCache.Add(new KeyValuePair<string, LogLevel>(message, level));
                if (earlyLogCache.Count > 500) earlyLogCache.RemoveAt(0);
                return;
            }

            if (outputControl.InvokeRequired)
            {
                // BeginInvoke instead of Invoke: never deadlocks against a
                // busy UI thread.
                outputControl.BeginInvoke(new Action(() => LogInternal(message, level)));
                return;
            }

            Color color;
            switch (level)
            {
                case LogLevel.Warning: color = WarningColor; break;
                case LogLevel.Error: color = ErrorColor; break;
                default: color = InfoColor; break;
            }

            outputControl.SelectionStart = outputControl.TextLength;
            outputControl.SelectionLength = 0;
            outputControl.SelectionColor = color;
            outputControl.SelectedText = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message + "\n";
            outputControl.SelectionColor = InfoColor;
            outputControl.ScrollToCaret();
        }
    }
}
