using Re4QuadExtremeEditor.src.Class.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor.src.Class
{
    /// <summary>
    /// One-click room automation (full port from Re4QuadX): wraps the bundled
    /// JADERLINK command-line tools so rooms and texture packs are extracted /
    /// repacked automatically, both on demand (Misc menu) and transparently
    /// while loading a room. Every step is reported to the console log.
    ///
    ///   St{n}\{room}.udas.lfs -> (LFS tool)     -> St{n}\{room}.udas
    ///   St{n}\{room}.udas     -> (UDAS extract) -> St{n}\{room}\...
    ///   St{n}\{room}\...      -> (UDAS repack, via .idxJ/.idx)
    ///   {pack}.pack.yz2(.lfs) -> (LFS + PACK)   -> {pack}.pack
    /// </summary>
    public static class ExternalToolManager
    {
        private static async Task<bool> RunTool(string toolPath, string targetPath, string argsPrefix = null)
        {
            if (string.IsNullOrEmpty(toolPath) || !File.Exists(toolPath))
            {
                EditorConsole.Error("Tool not found at path: " + toolPath + ". Please configure the tool path in the options.");
                return false;
            }
            if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
            {
                EditorConsole.Error("Target path not found: " + targetPath);
                return false;
            }

            try
            {
                string arguments = string.IsNullOrEmpty(argsPrefix)
                    ? "\"" + targetPath + "\""
                    : argsPrefix + " \"" + targetPath + "\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = toolPath,
                    Arguments = arguments,

                    // UseShellExecute=false runs the tool directly (CreateProcess)
                    // instead of going through a shell/Explorer - which is exactly
                    // what triggers the "Open File - Security Warning" window for
                    // anything downloaded from the internet.
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Path.GetDirectoryName(toolPath)
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    await Task.Run(() => process.WaitForExit());

                    if (process.ExitCode == 0)
                    {
                        EditorConsole.Log("Tool '" + Path.GetFileName(toolPath) + "' successfully processed '" + Path.GetFileName(targetPath) + "'.");
                        return true;
                    }
                    EditorConsole.Warning("Tool '" + Path.GetFileName(toolPath) + "' finished with a non-zero exit code (" + process.ExitCode + "). There may have been an error.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                EditorConsole.Error("An exception occurred while running the external tool: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> RepackFolderAsync(string toolPath, string folderPath, string argsPrefix = null)
        {
            EditorConsole.Log("Attempting to repack folder: " + folderPath);
            return await RunTool(toolPath, folderPath, argsPrefix);
        }

        public static async Task<bool> UnpackFileAsync(string toolPath, string filePath, string argsPrefix = null)
        {
            EditorConsole.Log("Attempting to unpack file: " + filePath);
            return await RunTool(toolPath, filePath, argsPrefix);
        }

        #region PACK Tool

        public static async Task<bool> UnpackPack(string filePath)
        {
            EditorConsole.Log("Attempting to unpack PACK file: " + Path.GetFileName(filePath));
            if (string.IsNullOrEmpty(BundledTools.PACK))
            {
                EditorConsole.Error("PACK Tool could not be found (bundled copy missing and no custom path set in the options menu).");
                return false;
            }
            return await RunTool(BundledTools.PACK, filePath, "-bat");
        }

        public static async Task<bool> RepackPackFolderAsync(string folderPath)
        {
            EditorConsole.Log("Attempting to repack PACK folder: " + Path.GetFileName(folderPath));
            if (string.IsNullOrEmpty(BundledTools.PACK))
            {
                EditorConsole.Error("PACK Tool could not be found (bundled copy missing and no custom path set in the options menu).");
                return false;
            }
            return await RunTool(BundledTools.PACK, folderPath, "-bat");
        }

        public static async Task UnpackAllPacks(string targetImagePack)
        {
            if (string.IsNullOrEmpty(BundledTools.PACK)){
                EditorConsole.Error("PACK Tool could not be found (bundled copy missing and no custom path set in the options menu).");
                return;
            }

            string targetDirectory = Path.Combine(Globals.DirectoryUHDRE4, "BIO4", targetImagePack);
            if (!Directory.Exists(targetDirectory)){
                EditorConsole.Error($"Target directory not found: '{targetDirectory}'. Mass unpack aborted.");
                return;
            }

            EditorConsole.Log($"Starting mass unpack of all files in '{targetDirectory}'...");

            var allFiles = Directory.GetFiles(targetDirectory, "*.*", SearchOption.TopDirectoryOnly);
            var filesToUnpackWithPackTool = new List<string>();

            //.LFS pack list
            EditorConsole.Log("Scanning for .lfs files to decompress first...");
            foreach (string file in allFiles.Where(f => f.EndsWith(".lfs", StringComparison.OrdinalIgnoreCase))){
                string unpackedLfsPath = await UnpackLfs(file);
                if (unpackedLfsPath != null)
                {
                    filesToUnpackWithPackTool.Add(unpackedLfsPath);
                }
            }

            //raw pack list
            var unpackedLfsBaseNames = new HashSet<string>(filesToUnpackWithPackTool.Select(f => Path.GetFileName(f)));
            foreach (string file in allFiles){
                if (!file.EndsWith(".lfs", StringComparison.OrdinalIgnoreCase) && !unpackedLfsBaseNames.Contains(Path.GetFileName(file)))
                {
                    filesToUnpackWithPackTool.Add(file);
                }
            }

            //unpack
            if (filesToUnpackWithPackTool.Count > 0){
                EditorConsole.Log($"Found {filesToUnpackWithPackTool.Count} file(s) to process with PACK tool. Unpacking...");
                foreach (string filePath in filesToUnpackWithPackTool)
                {
                    await UnpackPack(filePath);
                }
            }else{
                EditorConsole.Log("No files found to unpack in the target directory.");
            }

            EditorConsole.Log("Mass PACK unpack process finished.");
        }

        #endregion

        #region LFS Tool

        public static async Task<string> UnpackLfs(string lfsFilePath, bool deleteAfter = false)
        {
            EditorConsole.Log("Attempting to uncompress LFS file: " + Path.GetFileName(lfsFilePath));
            if (string.IsNullOrEmpty(BundledTools.LFS))
            {
                EditorConsole.Error("LFS Tool could not be found (bundled copy missing and no custom path set in the options menu).");
                return null;
            }

            bool success = await RunTool(BundledTools.LFS, lfsFilePath);
            if (success)
            {
                string unpackedPath = lfsFilePath.Substring(0, lfsFilePath.Length - ".lfs".Length);
                if (File.Exists(unpackedPath))
                {
                    if (deleteAfter)
                    {
                        try
                        {
                            File.Delete(lfsFilePath);
                            EditorConsole.Log("Deleted original file: " + Path.GetFileName(lfsFilePath));
                        }
                        catch (Exception ex)
                        {
                            EditorConsole.Warning("Failed to delete original LFS file: " + ex.Message);
                        }
                    }

                    return unpackedPath;
                }
            }
            return null;
        }

        public static async Task<string> RepackLfs(string filePath)
        {
            EditorConsole.Log("Attempting to compress file to LFS: " + Path.GetFileName(filePath));
            if (string.IsNullOrEmpty(BundledTools.LFS))
            {
                EditorConsole.Error("LFS Tool could not be found (bundled copy missing and no custom path set in the options menu).");
                return null;
            }

            bool success = await RunTool(BundledTools.LFS, filePath);
            if (success)
            {
                string repackedPath = filePath + ".lfs";
                if (File.Exists(repackedPath))
                {
                    return repackedPath;
                }
            }
            return null;
        }

        #endregion

        #region UDAS TOOL

        public static async Task RepackRoomUdas(bool repackCurrent = false)
        {
            if (string.IsNullOrEmpty(BundledTools.UdasRepack))
            {
                EditorConsole.Error("UDAS Repack Tool could not be found (bundled copy missing and no custom path set in the 'settings' menu).");
                return;
            }

            string roomDirectory;

            if (repackCurrent)
            {
                if (DataBase.SelectedRoom == null)
                {
                    EditorConsole.Warning("No room is currently loaded. Cannot perform repack.");
                    return;
                }

                roomDirectory = GetCurrentRoomDirectory();
                if (roomDirectory == null)
                {
                    EditorConsole.Error("Could not find the path for the current room. Repack aborted.");
                    return;
                }
            }
            else
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "Select the unpacked room folder to repack";
                    dialog.FileName = "Select Folder";
                    dialog.ValidateNames = false;
                    dialog.CheckFileExists = false;
                    dialog.CheckPathExists = true;

                    string initialDir = FindFirstGameBase();
                    if (initialDir != null)
                    {
                        dialog.InitialDirectory = initialDir;
                    }
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        roomDirectory = Path.GetDirectoryName(dialog.FileName);
                    }

                    else
                    {
                        EditorConsole.Log("Room repack operation was cancelled by the user.");
                        return;
                    }
                }
            }

            if (string.IsNullOrEmpty(roomDirectory) || !Directory.Exists(roomDirectory))
            {
                EditorConsole.Error("The specified room directory is invalid. Repack aborted.");
                return;
            }

            //get room unpacked folder
            string parentDirectory = Directory.GetParent(roomDirectory).FullName;
            //get room folder name/id
            string roomName = new DirectoryInfo(roomDirectory).Name;

            //build idx path
            string idxJPath = Path.Combine(parentDirectory, roomName + ".idxJ"); //for jader's tool
            string idxPath = Path.Combine(parentDirectory, roomName + ".idx");

            string targetIdxFile = null;

            //try to find idxJ or idx
            if (File.Exists(idxJPath))
            {
                targetIdxFile = idxJPath;
            }
            else if (File.Exists(idxPath))
            {
                targetIdxFile = idxPath;
            }

            if (targetIdxFile != null)
            {
                EditorConsole.Log("Found file: " + Path.GetFileName(targetIdxFile) + ". Attempting to repack...");
                await RepackFolderAsync(BundledTools.UdasRepack, targetIdxFile);
            }
            else
            {
                EditorConsole.Error("Could not find .idxJ or .idx file for repacking in '" + parentDirectory + "'. Repack cancelled.");
            }
        }

        public static async Task UnpackRoomUdas()
        {
            if (string.IsNullOrEmpty(BundledTools.UdasExtract))
            {
                EditorConsole.Error("UDAS Extract Tool could not be found (bundled copy missing and no custom path set in the options menu).");
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select .udas File to Unpack";
                dialog.Filter = "UDAS/DAT Room File (*.udas, *.dat)|*.udas;*.dat|All Files (*.*)|*.*";
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;

                string initialDir = FindFirstGameBase();
                if (!string.IsNullOrEmpty(initialDir))
                    dialog.InitialDirectory = initialDir;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string udasPath = dialog.FileName;

                    if (File.Exists(udasPath))
                    {
                        await UnpackFileAsync(BundledTools.UdasExtract, udasPath);
                    }
                    else
                    {
                        EditorConsole.Error("Could not find .udas file at '" + udasPath + "'. Unpack aborted.");
                    }
                }
                else
                {
                    EditorConsole.Log("Room unpack operation was cancelled by the user.");
                }
            }
        }

        public static async Task UnpackAllRoomsUdas(bool deleteLFS = true)
        {
            if (string.IsNullOrEmpty(BundledTools.UdasExtract))
            {
                EditorConsole.Error("DAT/UDAS Extract Tool could not be found (bundled copy missing and no custom path set in the options menu).");
                return;
            }

            //scan every configured game root (UHD/PS4NS keep rooms under BIO4)
            foreach (string basePath in ConfiguredGameBases())
            {
                EditorConsole.Log("Starting mass unpack of all DAT/UDAS files in '" + basePath + "'...");

                //loop through stage folders st0-7
                for (int i = 0; i <= 7; i++)
                {
                    string stagePath = Path.Combine(basePath, "St" + i);

                    if (!Directory.Exists(stagePath))
                    {
                        // 2007 stage folders are compressed as .dat
                        if (basePath.Contains("2007"))
                        {
                            if (i == 0 || i > 5) continue; //2007 st only go from 1 to 5

                            if (string.IsNullOrEmpty(BundledTools.GCA))
                            {
                                EditorConsole.Error("GCA Tool could not be found. Unpacked stage file '" + stagePath + "' will be skipped...");
                                continue;
                            }

                            //convert stagepath to stagefile (add ".dat" at the end)
                            string stageFile = stagePath + ".dat";
                            EditorConsole.Log("Unpack DAT stage file '" + stageFile + "'...");
                            await UnpackFileAsync(BundledTools.GCA, stageFile, "-bat");
                        }
                        else
                        {
                            continue;
                        }
                    }

                    string[] lfsFiles = Directory.GetFiles(stagePath, "*.udas.lfs", SearchOption.TopDirectoryOnly);
                    string[] udasFiles = Directory.GetFiles(stagePath, "*.udas", SearchOption.TopDirectoryOnly);

                    if (lfsFiles.Length == 0 && udasFiles.Length == 0) { continue; }

                    EditorConsole.Log("Found rooms in St" + i + ". Processing...");

                    //.LFS udas
                    foreach (string lfsPath in lfsFiles)
                    {
                        string unpackedUdasPath = await UnpackLfs(lfsPath, deleteLFS);
                        if (unpackedUdasPath != null)
                        {
                            await UnpackFileAsync(BundledTools.UdasExtract, unpackedUdasPath);
                        }
                    }

                    //raw udas
                    var lfsBaseNames = new HashSet<string>(lfsFiles.Select(f => Path.GetFileNameWithoutExtension(f)));
                    foreach (string udasPath in udasFiles)
                    {
                        if (!lfsBaseNames.Contains(Path.GetFileName(udasPath)))
                        {
                            await UnpackFileAsync(BundledTools.UdasExtract, udasPath);
                        }
                    }
                }
            }

            EditorConsole.Log("Mass room unpack process finished.");
        }

        /// <summary>
        /// Makes sure a single room's files are extracted and ready to be read from disk,
        /// automating exactly the manual steps a modder would otherwise do by hand:
        /// St{n}\{room}.udas.lfs -> (LFS tool) -> St{n}\{room}.udas -> (UDAS tool) -> St{n}\{room}\...
        ///
        /// If the room folder already has files in it, this does nothing (fast path).
        /// If the room simply isn't the one that belongs to this particular stage folder,
        /// this quietly returns false so the caller can keep looking in other stages -
        /// exactly like the previous "folder doesn't exist yet, skip" behaviour.
        /// </summary>
        public static async Task<bool> EnsureRoomExtracted(string stagePath, string roomName, EditorRe4Ver gameVersion, int stageIndex)
        {
            string roomFolder = Path.Combine(stagePath, roomName);
            Func<bool> RoomFolderReady = () => Directory.Exists(roomFolder) && Directory.GetFiles(roomFolder).Length > 0;

            if (RoomFolderReady()) return true;

            if (gameVersion == EditorRe4Ver.UHD || gameVersion == EditorRe4Ver.PS4NS)
            {
                string udasPath = Path.Combine(stagePath, roomName + ".udas");
                string lfsPath = udasPath + ".lfs";

                if (!File.Exists(udasPath))
                {
                    if (!File.Exists(lfsPath)) return false; //this room isn't in this stage folder at all

                    EditorConsole.Log("'" + roomName + "' is not extracted yet. Decompressing '" + roomName + ".udas.lfs'...");
                    string decompressed = await UnpackLfs(lfsPath);
                    if (decompressed == null)
                    {
                        EditorConsole.Error("Failed to decompress '" + roomName + ".udas.lfs'.");
                        return false;
                    }
                }

                EditorConsole.Log("Extracting '" + roomName + ".udas'...");
                await UnpackFileAsync(BundledTools.UdasExtract, udasPath);

                if (!RoomFolderReady())
                {
                    EditorConsole.Error("UDAS extraction did not produce the expected folder for '" + roomName + "'.");
                    return false;
                }
                return true;
            }

            if (gameVersion == EditorRe4Ver.SourceNext2007)
            {
                string stageDat = stagePath + ".dat";

                if (!Directory.Exists(stagePath))
                {
                    if (!File.Exists(stageDat)) return false; //stage doesn't exist at all

                    EditorConsole.Log("Stage 'St" + stageIndex + "' is not extracted yet. Unpacking 'St" + stageIndex + ".dat'...");
                    await UnpackFileAsync(BundledTools.GCA, stageDat, "-bat");
                }

                return RoomFolderReady();
            }

            //PS2 (and any other version) ships room folders already extracted - nothing to do
            return RoomFolderReady();
        }

        /// <summary>
        /// Same as EnsureRoomExtracted, but blocking - for use in places that can't be made
        /// async easily (like RoomSelectedObj's constructor chain, which reads the SMD/SMX
        /// directly without going through the room-picker's own extraction step).
        ///
        /// Runs on a background thread (Task.Run) before blocking, so it does NOT deadlock
        /// the UI thread - the tools awaited deeper in this chain resume on their own
        /// thread-pool context instead of trying to get back onto a UI thread that is
        /// itself sitting here waiting for them.
        /// </summary>
        public static bool EnsureRoomExtractedSync(string stagePath, string roomName, EditorRe4Ver gameVersion, int stageIndex)
        {
            return Task.Run(() => EnsureRoomExtracted(stagePath, roomName, gameVersion, stageIndex)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Makes sure a texture pack (e.g. "44000101") is available as a plain, readable
        /// ".pack" file inside packFolder, unwrapping whatever compression it currently has:
        ///
        ///   44000101.pack             -> already ready, nothing to do
        ///   44000101.pack.yz2         -> (PACK/YZ2 tool)          -> .pack
        ///   44000101.pack.lfs         -> (LFS tool)                -> .pack directly
        ///   44000101.pack.yz2.lfs     -> (LFS tool) -> .pack.yz2 -> (PACK/YZ2 tool) -> .pack
        ///
        /// Blocking on purpose - this is called from RoomSelectedObj's synchronous model
        /// loading code, once per texture pack referenced by the room (usually cached on
        /// disk after the first time, so this only actually runs the tools once per pack).
        /// Runs via Task.Run for the same UI-thread-deadlock reason as EnsureRoomExtractedSync.
        /// </summary>
        public static bool EnsurePackExtracted(string packFolder, string packId)
        {
            return Task.Run(() => EnsurePackExtractedAsync(packFolder, packId)).GetAwaiter().GetResult();
        }

        private static async Task<bool> EnsurePackExtractedAsync(string packFolder, string packId)
        {
            string basePath = Path.Combine(packFolder, packId);
            string packPath = basePath + ".pack";

            if (File.Exists(packPath)) return true;

            string yz2Path = basePath + ".pack.yz2";

            if (!File.Exists(yz2Path))
            {
                string yz2LfsPath = basePath + ".pack.yz2.lfs";
                string packLfsPath = basePath + ".pack.lfs";
                string lfsSource = File.Exists(yz2LfsPath) ? yz2LfsPath : (File.Exists(packLfsPath) ? packLfsPath : null);

                if (lfsSource == null) return false; //this pack doesn't exist in any known form

                EditorConsole.Log("'" + packId + "' texture pack is not extracted yet. Decompressing '" + Path.GetFileName(lfsSource) + "'...");
                string decompressed = await UnpackLfs(lfsSource);
                if (decompressed == null)
                {
                    EditorConsole.Error("Failed to decompress '" + Path.GetFileName(lfsSource) + "'.");
                    return false;
                }

                if (File.Exists(packPath)) return true; //it was ".pack.lfs" -> now ".pack" directly, done
            }

            if (File.Exists(yz2Path))
            {
                EditorConsole.Log("Extracting texture pack '" + packId + ".pack.yz2'...");
                await UnpackPack(yz2Path);
            }

            return File.Exists(packPath);
        }

        #endregion

        #region Room path discovery

        /// <summary>
        /// Derives the on-disk folder of the loaded room exactly like the
        /// model loader does (DataBase.DirectoryDic[PathKey] + SmdFile), with
        /// a name-based St0..St7 scan as fallback.
        /// </summary>
        public static string GetCurrentRoomDirectory()
        {
            if (DataBase.SelectedRoom == null || DataBase.SelectedRoom.GetRoomModel() == null) return null;

            var roomModel = DataBase.SelectedRoom.GetRoomModel();

            try
            {
                string baseDir;
                if (!string.IsNullOrEmpty(roomModel.PathKey)
                    && !string.IsNullOrEmpty(roomModel.SmdFile)
                    && DataBase.DirectoryDic != null
                    && DataBase.DirectoryDic.TryGetValue(roomModel.PathKey.ToLowerInvariant(), out baseDir))
                {
                    string smdPath = baseDir + roomModel.SmdFile;
                    string dir = Path.GetDirectoryName(smdPath);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) return dir;
                }
            }
            catch { }

            string rawFileName = Path.GetFileNameWithoutExtension(roomModel.JsonFileName);
            if (string.IsNullOrEmpty(rawFileName)) return null;
            string roomName = rawFileName.Split('_')[0];
            if (string.IsNullOrEmpty(roomName)) return null;

            foreach (string basePath in ConfiguredGameBases())
            {
                for (int i = 0; i <= 7; i++)
                {
                    string potentialPath = Path.Combine(basePath, "St" + i, roomName);
                    if (Directory.Exists(potentialPath)) return potentialPath;
                }
            }
            return null;
        }

        private static List<string> ConfiguredGameBases()
        {
            var bases = new List<string>();
            AddBase(bases, Globals.DirectoryUHDRE4, true);
            AddBase(bases, Globals.DirectoryPS4NSRE4, true);
            AddBase(bases, Globals.Directory2007RE4, false);
            AddBase(bases, Globals.DirectoryPS2RE4, false);
            return bases;
        }

        private static void AddBase(List<string> list, string root, bool underBio4)
        {
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) return;
            string basePath = underBio4 ? Path.Combine(root, "BIO4") : root;
            if (!list.Contains(basePath)) list.Add(basePath);
        }

        private static string FindFirstGameBase()
        {
            var bases = ConfiguredGameBases();
            return bases.Count > 0 ? bases[0] : null;
        }

        #endregion
    }
}
