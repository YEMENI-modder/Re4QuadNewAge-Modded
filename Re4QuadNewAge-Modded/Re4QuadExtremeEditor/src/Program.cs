using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Re4QuadExtremeEditor
{
    static class Program
    {
        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        //raises the Windows timer resolution so Thread.Sleep(1) really sleeps
        //~1ms instead of up to 15.6ms - without this the render loop paces
        //irregularly and camera/object motion looks jittery
        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint period);

        private const int SHCNE_ASSOCCHANGED = 0x08000000;
        private const int SHCNF_IDLIST = 0x0000;

        /// <summary>
        /// .quad file passed on the command line (double-click "Open with"),
        /// opened automatically after the main form is shown.
        /// </summary>
        public static string StartupProjectFile { get; set; }

        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Configurar manipuladores globais de exceção
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            try
            {
                //when launched via a file association the working directory is the
                //file's folder - pin it to the exe dir so relative paths (configs,
                //room lists) keep working no matter how the editor was started
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                timeBeginPeriod(1);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                RegisterQuadFileAssociation();

                if (args != null)
                {
                    StartupProjectFile = args.FirstOrDefault(a =>
                        a.EndsWith(".quad", StringComparison.OrdinalIgnoreCase) && File.Exists(a));
                }

                Application.Run(new MainForm());
            }
            catch {}
            // Não tem como capturar a System.ObjectDisposedException
            // Essa Exception é gerada quando o openGL é de uma versão não suportada pelo programa
            // O try catch impede do windows gerar um "CrashDumps"
        }

        /// <summary>
        /// Registers the .quad extension for the current user so project files
        /// show the editor icon and open the editor on double-click.
        /// </summary>
        private static void RegisterQuadFileAssociation()
        {
            try
            {
                string exe = Application.ExecutablePath;

                using (var progId = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Re4QuadExtremeEditor.quad"))
                {
                    progId.SetValue("", "Quad Project");
                    using (var icon = progId.CreateSubKey("DefaultIcon"))
                    {
                        icon.SetValue("", "\"" + exe + "\",0");
                    }
                    using (var command = progId.CreateSubKey(@"shell\open\command"))
                    {
                        command.SetValue("", "\"" + exe + "\" \"%1\"");
                        command.SetValue("WorkingDirectory", Path.GetDirectoryName(exe));
                    }
                }

                using (var extension = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.quad"))
                {
                    extension.SetValue("", "Re4QuadExtremeEditor.quad");
                }

                //tell the shell the association changed so file icons refresh
                //immediately instead of showing a generic white icon
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            catch
            {
                //file association is best-effort only
            }
        }

        // Manipulador para exceções de thread UI
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception, "Error in graphical interface");
        }

        // Manipulador para exceções não tratadas de threads não-UI
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, "General error");
            }
        }

        // Método para lidar com exceções
        private static void HandleException(Exception ex, string context)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Re4Quad_error_log.txt");
                File.AppendAllText(logPath,
                    "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + context + "\r\n"
                    + ex.ToString() + "\r\n\r\n");
            }
            catch
            {
            }
            MessageBox.Show($"{context}: {ex.Message}\nAn unexpected error occurred, the program may not work correctly from now on.", "Error:", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
