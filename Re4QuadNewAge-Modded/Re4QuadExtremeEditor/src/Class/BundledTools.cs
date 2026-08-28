using System;
using System.IO;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// Resolves the on-disk path for each external command-line tool
    /// (UDAS, LFS, PACK, GCA).
    ///
    /// The tools ship bundled with the editor (see the "ExternalTools" folder
    /// next to the .exe), so the user normally doesn't need to download them
    /// separately or configure paths by hand in Options.
    ///
    /// If the bundled copy of a tool is missing (deleted by hand, antivirus,
    /// broken install...), this falls back to whatever custom path the user
    /// set manually in the Options menu - so that flexibility is not lost.
    /// </summary>
    public static class BundledTools
    {
        private static string Resolve(string bundledRelativePath, string userConfiguredFallback)
        {
            string bundledPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, bundledRelativePath);

            if (File.Exists(bundledPath))
            {
                return bundledPath;
            }

            return userConfiguredFallback;
        }

        /// <summary>
        /// True when the tool that ships with the program is present on disk
        /// (i.e. the user does not need to have configured anything manually).
        /// </summary>
        public static bool IsToolBundled(string bundledRelativePath)
        {
            return File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, bundledRelativePath));
        }

        public static string LFS => Resolve(Path.Combine("ExternalTools", "LFS", "re4lfs.exe"), Globals.ToolPathLFS);
        public static string UdasExtract => Resolve(Path.Combine("ExternalTools", "UDAS", "data", "JADERLINK_DATUDAS_EXTRACT.exe"), Globals.ToolPathUDAS);
        public static string UdasRepack => Resolve(Path.Combine("ExternalTools", "UDAS", "data", "JADERLINK_DATUDAS_REPACK.exe"), Globals.ToolPathUDAS);
        public static string PACK => Resolve(Path.Combine("ExternalTools", "PACK", "data", "RE4_UHD_PACK_TOOL.exe"), Globals.ToolPathPACK);
        public static string GCA => Resolve(Path.Combine("ExternalTools", "GCA", "RE4_2007_GCA_TOOL.exe"), Globals.ToolPathGCA);
    }
}
