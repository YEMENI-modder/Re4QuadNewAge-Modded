using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using OpenTK;
using Re4QuadExtremeEditor.src.Class;
using Re4QuadExtremeEditor.src.Forms;
using Re4QuadExtremeEditor.src.JSON;
using NewAgeTheRender;

namespace Re4QuadExtremeEditor.src
{
    /// <summary>
    /// Blender-style project system (.quad files): saves the whole session
    /// (room reference + every loaded object file + camera) into one small
    /// zip-based file, so the user can reopen it later and continue exactly
    /// where they left off.
    /// </summary>
    public static class ProjectManager
    {
        public const string FileFilter = "Quad Project (*.quad)|*.quad|All files (*.*)|*.*";
        private const string Extension = ".quad";

        /// <summary>Path of the project currently open in the editor (null = none).</summary>
        public static string CurrentProjectPath { get; private set; }

        // ------------------------------------------------------------------
        //  SAVE
        // ------------------------------------------------------------------

        public static void SaveProject(MainForm main, string path)
        {
            var room = DataBase.SelectedRoom;
            if (room == null)
            {
                EditorConsole.Error("Nothing is loaded - open a room before saving a project.");
                return;
            }

            var roomModel = room.GetRoomModel();
            var listObj = room.GetRoomListObj();
            if (roomModel == null || listObj == null)
            {
                EditorConsole.Error("The current room has no identification - cannot save the project.");
                return;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "Re4QuadProject_" + Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(tempDir);
                string objectsDir = Path.Combine(tempDir, "objects");
                Directory.CreateDirectory(objectsDir);

                int savedFiles = SaveObjectFiles(objectsDir);

                var meta = new JObject
                {
                    ["AppVersion"] = Application.ProductVersion,
                    ["SavedAt"] = DateTime.UtcNow.ToString("o"),
                    ["RoomListFile"] = listObj.JsonFileName,
                    ["RoomModelFile"] = roomModel.JsonFileName,
                    ["UseDataQingshengSource"] = Globals.useDataQingshengSource,
                    ["SavedFileCount"] = savedFiles,
                    ["Camera"] = new JObject
                    {
                        ["X"] = main.ActiveCamera.Position.X,
                        ["Y"] = main.ActiveCamera.Position.Y,
                        ["Z"] = main.ActiveCamera.Position.Z,
                        ["LookAtX"] = main.ActiveCamera.LookAt.X,
                        ["LookAtY"] = main.ActiveCamera.LookAt.Y,
                        ["LookAtZ"] = main.ActiveCamera.LookAt.Z,
                        ["YawDegrees"] = main.ActiveCamera.YawDegrees,
                        ["PitchDegrees"] = main.ActiveCamera.PitchDegrees,
                        ["Mode"] = (int)main.ActiveCamera.CamMode,
                    },
                    ["OriginalPaths"] = JObject.FromObject(new
                    {
                        ETS = Globals.FilePathETS ?? "",
                        ITA = Globals.FilePathITA ?? "",
                        AEV = Globals.FilePathAEV ?? "",
                        DSE = Globals.FilePathDSE ?? "",
                        AVL = Globals.FilePathAVL ?? "",
                        FSE = Globals.FilePathFSE ?? "",
                        SAR = Globals.FilePathSAR ?? "",
                        EAR = Globals.FilePathEAR ?? "",
                        EMI = Globals.FilePathEMI ?? "",
                        ESE = Globals.FilePathESE ?? "",
                        LIT = Globals.FilePathLIT ?? "",
                    })
                };
                File.WriteAllText(Path.Combine(tempDir, "project.json"), meta.ToString(Newtonsoft.Json.Formatting.Indented));

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                ZipFile.CreateFromDirectory(tempDir, path, CompressionLevel.Optimal, false);

                CurrentProjectPath = path;
                main.SetProjectTitle(path);
                EditorConsole.Log($"Project saved: {path} ({savedFiles} object file(s), {new FileInfo(path).Length / 1024} KB)");
            }
            catch (Exception ex)
            {
                EditorConsole.Error("Failed to save the project: " + ex.Message);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch (Exception) { }
            }
        }

        private static int SaveObjectFiles(string objectsDir)
        {
            //files are stored under their REAL extension so the standard
            //"*.ext" wildcard search finds them again on open
            int saved = 0;
            saved += TrySaveOne(objectsDir, "0000.ets", Globals.FilePathETS, FileManager.SaveFileETS);
            saved += TrySaveOne(objectsDir, "0000.ita", Globals.FilePathITA, FileManager.SaveFileITA);
            saved += TrySaveOne(objectsDir, "0000.aev", Globals.FilePathAEV, FileManager.SaveFileAEV);
            saved += TrySaveOne(objectsDir, "0000.dse", Globals.FilePathDSE, FileManager.SaveFileDSE);
            saved += TrySaveOne(objectsDir, "0000.avl", Globals.FilePathAVL, FileManager.SaveFileAVL);
            saved += TrySaveOne(objectsDir, "0000.fse", Globals.FilePathFSE, FileManager.SaveFileFSE);
            saved += TrySaveOne(objectsDir, "0000.sar", Globals.FilePathSAR, FileManager.SaveFileSAR);
            saved += TrySaveOne(objectsDir, "0000.ear", Globals.FilePathEAR, FileManager.SaveFileEAR);
            saved += TrySaveOne(objectsDir, "0000.emi", Globals.FilePathEMI, FileManager.SaveFileEMI);
            saved += TrySaveOne(objectsDir, "0000.ese", Globals.FilePathESE, FileManager.SaveFileESE);
            saved += TrySaveOne(objectsDir, "0000.lit", Globals.FilePathLIT, FileManager.SaveFileLIT);
            return saved;
        }

        private static int TrySaveOne(string dir, string fileName, string sourcePath, Action<FileStream> saver)
        {
            //a type is only considered loaded when its path was recorded
            if (string.IsNullOrEmpty(sourcePath))
            {
                return 0;
            }

            try
            {
                using (var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create))
                {
                    saver(stream);
                }
                return 1;
            }
            catch (Exception ex)
            {
                EditorConsole.Error($"Failed serializing {fileName}: {ex.Message}");
                return 0;
            }
        }

        // ------------------------------------------------------------------
        //  OPEN
        // ------------------------------------------------------------------

        public static void OpenProject(MainForm main, string path)
        {
            EditorConsole.Log($"Opening project: {path}");
            main.ShowConsoleTab();

            string extractDir = Path.Combine(Path.GetTempPath(), "Re4QuadProjects",
                Path.GetFileNameWithoutExtension(path));
            try
            {
                if (!File.Exists(path))
                {
                    EditorConsole.Error("Project file not found.");
                    return;
                }

                if (Directory.Exists(extractDir))
                {
                    Directory.Delete(extractDir, true);
                }
                Directory.CreateDirectory(extractDir);
                ZipFile.ExtractToDirectory(path, extractDir);

                var meta = JObject.Parse(File.ReadAllText(Path.Combine(extractDir, "project.json")));

                //resolve the room list and room model by their json names
                var lists = DataBase.CachedRoomInfoList ?? Utils.LoadRoomInfoList();
                RoomInfo roomInfo = lists.FirstOrDefault(r => r.RoomListObj != null &&
                    r.RoomListObj.JsonFileName == (string)meta["RoomListFile"]);
                if (roomInfo == null)
                {
                    EditorConsole.Error($"Room list '{meta["RoomListFile"]}' not found. Check that the JSON lists still exist.");
                    return;
                }

                RoomModel roomModel = roomInfo.RoomModelDict?.Values.FirstOrDefault(m =>
                    m.JsonFileName == (string)meta["RoomModelFile"]);
                if (roomModel == null)
                {
                    EditorConsole.Error($"Room '{meta["RoomModelFile"]}' not found inside '{meta["RoomListFile"]}'.");
                    return;
                }

                Globals.useDataQingshengSource = meta.Value<bool?>("UseDataQingshengSource") ?? false;

                ClearLoadedObjects(main);

                //load archived object files (only the extensions present)
                string objectsDir = Path.Combine(extractDir, "objects");
                var available = new HashSet<string>();
                if (Directory.Exists(objectsDir))
                {
                    foreach (string f in Directory.GetFiles(objectsDir))
                    {
                        available.Add(Path.GetExtension(f).ToLowerInvariant());
                    }
                }

                RoomObjectFileLoader.LoadAllFilesForRoom(objectsDir, roomModel, delegate (string extension)
                {
                    return available.Contains(extension.ToLowerInvariant());
                });

                //recreate the room model itself (same core as LoadRoomModel)
                DataBase.SelectedRoom?.ClearGL();
                DataBase.SelectedRoom = null;


                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    DataBase.SelectedRoom = new RoomSelectedObj(roomModel, roomInfo.RoomListObj, GetDataQingshengDirectory(roomModel));
                }
                catch (OperationCanceledException)
                {
                    EditorConsole.Log("Room loading cancelled by user.");
                    Cursor.Current = Cursors.Default;
                    return;
                }

                //restore the camera
                var cam = meta["Camera"];
                if (cam != null && main.ActiveCamera != null)
                {
                    try
                    {
                        var pos = new Vector3(
                            cam.Value<float>("X"), cam.Value<float>("Y"), cam.Value<float>("Z"));

                        //new projects store the look-at target too; legacy ones don't,
                        //but it is only used by the ORBIT branch so a fallback is fine
                        float? lookX = cam.Value<float?>("LookAtX");
                        var look = lookX.HasValue
                            ? new Vector3(
                                lookX.Value,
                                cam.Value<float?>("LookAtY") ?? 0f,
                                cam.Value<float?>("LookAtZ") ?? 0f)
                            : pos;

                        main.ActiveCamera.ApplyExternalState(
                            pos,
                            cam.Value<float>("YawDegrees"),
                            cam.Value<float>("PitchDegrees"),
                            look,
                            cam.Value<int?>("Mode") ?? 0);
                    }
                    catch (Exception ex)
                    {
                        EditorConsole.Warning("Could not restore the camera: " + ex.Message);
                    }
                }

                CurrentProjectPath = path;
                main.SetProjectTitle(path);

                EditorConsole.Log($"Project loaded: {Path.GetFileName(path)} - continue where you left off.");
            }
            catch (Exception ex)
            {
                EditorConsole.Error("Failed to open the project: " + ex);
            }
        }

        private static void ClearLoadedObjects(MainForm main)
        {
            UndoSystem.Clear();
            EditorConsole.Log("Clearing previously loaded object files..");

            try
            {
                main.TreeViewUpdateSelectedsClear();
            }
            catch (Exception ex)
            {
                EditorConsole.Error("Failed to clear the current selection: " + ex);
            }

            FileManager.ClearETS();
            FileManager.ClearITA();
            FileManager.ClearAEV();
            FileManager.ClearDSE();
            FileManager.ClearFSE();
            FileManager.ClearSAR();
            FileManager.ClearEAR();
            FileManager.ClearEMI();
            FileManager.ClearESE();
            FileManager.ClearLIT();
        }

        // ------------------------------------------------------------------
        //  Shared helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns the "data Qingsheng DLL" directory for this room
        /// (<base>\data\<room>) when the mode is selected and the folder
        /// exists; otherwise null (standard St-folder loading is used).
        /// </summary>
        public static string GetDataQingshengDirectory(RoomModel rm)
        {
            if (rm == null || !Globals.useDataQingshengSource)
            {
                return null;
            }

            string rootGamePath;
            bool uhdFamily;
            switch (rm.Type)
            {
                case RoomModel.EType.UHD:
                case RoomModel.EType.R100UHD:
                    rootGamePath = Globals.DirectoryUHDRE4; uhdFamily = true; break;
                case RoomModel.EType.PS4NS:
                case RoomModel.EType.R100PS4NS:
                    rootGamePath = Globals.DirectoryPS4NSRE4; uhdFamily = true; break;
                case RoomModel.EType.V2007:
                    rootGamePath = Globals.Directory2007RE4; uhdFamily = false; break;
                case RoomModel.EType.PS2:
                    rootGamePath = Globals.DirectoryPS2RE4; uhdFamily = false; break;
                default:
                    return null;
            }

            if (string.IsNullOrEmpty(rootGamePath))
            {
                return null;
            }

            string basePath = uhdFamily ? Path.Combine(rootGamePath, "BIO4") : rootGamePath;
            string roomName = Path.GetFileNameWithoutExtension(rm.JsonFileName).Split('_')[0];
            string dir = Path.Combine(basePath, "data", roomName);

            return Directory.Exists(dir) ? dir : null;
        }
    }
}
