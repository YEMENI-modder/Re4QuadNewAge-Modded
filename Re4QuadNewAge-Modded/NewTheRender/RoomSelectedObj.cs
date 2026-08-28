using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using Re4QuadExtremeEditor.src.JSON;
using ViewerBase;
using Re4QuadExtremeEditor;
using Re4QuadExtremeEditor.src;
using Re4QuadExtremeEditor.src.Class;
using Re4QuadExtremeEditor.src.Class.Enums;
using Re4QuadExtremeEditor.src.Class.Shaders;
using System.IO;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using JADERLINK_MODEL_VIEWER.src.Nodes;
using Re4ViewerRender;

namespace NewAgeTheRender
{
    public class RoomSelectedObj
    {
        public static bool RenderTextures = true;
        public static bool RenderWireframe = false;
        public static bool RenderNormals = false;
        public static bool RenderOnlyFrontFace = false;
        public static bool RenderVertexColor = true;
        public static bool RenderAlphaChannel = true;
        public static bool LoadTextureLinear = true;

        private RoomModel roomModel;
        private RoomListObj mainListobj;

        public RoomListObj GetRoomListObj() 
        {
            return mainListobj;
        }

        public RoomModel GetRoomModel() 
        {
            return roomModel;
        }

        private ModelGroup modelGroup;
        private ModelNodeGroup mng;
        private TexturePackNodeGroup tpng;
        private ScenarioNodeGroup sng;
        private RenderOrder order;
        private ModelNodeOrder modelNodeOrder;
        private ExtraTreatedModelContainer extraContainer;

        public RoomSelectedObj(RoomModel roomModel, RoomListObj mainListobj)
            : this(roomModel, mainListobj, null)
        {
        }

        //when not null, the room model (smd/smx) is read from this folder
        //using the flat "data Qingsheng DLL" names: 0000.smd / 0000.smx
        private readonly string dataQingshengDirectory;

        private LoadingForm _loadingForm;

        public RoomSelectedObj(RoomModel roomModel, RoomListObj mainListobj, string dataQingshengDirectory)
        {
            modelGroup = new ModelGroup();
            mng = new ModelNodeGroup();
            tpng = new TexturePackNodeGroup();
            sng = new ScenarioNodeGroup();
            order = new RenderOrder();
            modelNodeOrder = new ModelNodeOrder(mng);
            extraContainer = new ExtraTreatedModelContainer(modelGroup);

            this.roomModel = roomModel;
            this.mainListobj = mainListobj;
            this.dataQingshengDirectory = dataQingshengDirectory;
            Load(roomModel);
        }

        private void PumpLoading()
        {
            try
            {
                if (_loadingForm != null && !_loadingForm.IsDisposed && _loadingForm.IsHandleCreated)
                    _loadingForm.Invoke(new Action(() => Application.DoEvents()));
            }
            catch { }
        }

        private void SetLoadingText(string text)
        {
            try
            {
                if (_loadingForm != null && !_loadingForm.IsDisposed && _loadingForm.IsHandleCreated)
                    _loadingForm.BeginInvoke(new Action(() => _loadingForm.UpdateText(text)));
            }
            catch { }
        }

        private void Load(RoomModel roomModel) 
        {
            TextureRef.LoadTextureLinear = LoadTextureLinear;

            string roomName = "Loading Room...";
            if (roomModel is RoomModel2007 r2007) roomName = "Loading " + Path.GetFileName(r2007.SmdFile);
            else if (roomModel is RoomModelPs2 rps2) roomName = "Loading " + Path.GetFileName(rps2.SmdFile);
            else if (roomModel is RoomModelR100Uhd rr100) roomName = "Loading " + Path.GetFileName(rr100.SmdFile);
            else if (roomModel is RoomModelUhd ruhd) roomName = "Loading " + Path.GetFileName(ruhd.SmdFile);

            _loadingForm = new LoadingForm(roomName);
            var ready = new System.Threading.ManualResetEvent(false);
            System.Exception loadException = null;

            var uiThread = new System.Threading.Thread(() =>
            {
                try
                {
                    _loadingForm.FormClosed += (s, e) =>
                    {
                        try { Application.ExitThread(); } catch { }
                    };
                    ready.Set();
                    Application.Run(_loadingForm);
                }
                catch { }
            });
            uiThread.SetApartmentState(System.Threading.ApartmentState.STA);
            uiThread.IsBackground = true;
            uiThread.Start();
            ready.WaitOne();

            System.Threading.Thread.Sleep(100);

            try
            {
                if (roomModel is RoomModel2007 roomModel2007)
                {
                    Load2007(roomModel2007);
                }
                else if (roomModel is RoomModelPs2 roomModelPS2)
                {
                    LoadPS2(roomModelPS2);
                }
                else if (roomModel is RoomModelR100Uhd roomModelR100Uhd) 
                {
                    LoadR100UHD(roomModelR100Uhd);
                }
                else if (roomModel is RoomModelUhd roomModelUhd)
                {
                    LoadUHD(roomModelUhd);
                }
            }
            catch (Exception ex)
            {
                loadException = ex;
            }
            finally
            {
                try
                {
                    if (_loadingForm != null && !_loadingForm.IsDisposed && _loadingForm.IsHandleCreated)
                        _loadingForm.BeginInvoke(new Action(() => { try { _loadingForm.Close(); } catch { } }));
                }
                catch { }
                uiThread.Join(3000);
                _loadingForm = null;
            }

            if (loadException != null)
                throw loadException;
        }

        /// <summary>
        /// Resolves a room file path (SMD/SMX). If the expected path exists, returns it.
        /// Otherwise searches the room folder for files with the same extension.
        /// Shows a selection dialog if multiple candidates are found.
        /// </summary>
        private static string ResolveRoomFile(string expectedPath, string fileLabel)
        {
            string dir = null;
            if (!string.IsNullOrEmpty(expectedPath))
                dir = Path.GetDirectoryName(expectedPath);

            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                return expectedPath;

            string ext = Path.GetExtension(expectedPath);
            if (string.IsNullOrEmpty(ext))
                return expectedPath;

            string[] candidates = Directory.GetFiles(dir, "*" + ext, SearchOption.TopDirectoryOnly);

            if (candidates.Length == 0)
                return expectedPath;

            if (candidates.Length == 1)
            {
                EditorConsole.Log($"Resolved {fileLabel}: {Path.GetFileName(candidates[0])}");
                return candidates[0];
            }

            string chosen = ShowFileSelectionDialog(candidates, fileLabel);
            if (!string.IsNullOrEmpty(chosen))
                return chosen;

            throw new OperationCanceledException($"User cancelled {fileLabel} selection.");
        }

        private static string ShowFileSelectionDialog(string[] files, string fileLabel)
        {
            string title = $"Select {fileLabel} file";

            using (var form = new Form())
            {
                form.Text = title;
                form.Size = new System.Drawing.Size(420, 300);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.TopMost = true;

                DarkTheme.Apply(form);

                var listBox = new ListBox();
                listBox.Dock = DockStyle.Fill;
                listBox.IntegralHeight = false;
                foreach (string f in files)
                {
                    string display = Path.GetFileName(f);
                    listBox.Items.Add(display);
                }
                if (files.Length > 0)
                    listBox.SelectedIndex = 0;

                listBox.SelectedIndexChanged += (s, e) =>
                {
                    var okBtn = form.Controls["btnOK"] as Button;
                    if (okBtn != null) okBtn.Enabled = listBox.SelectedIndex >= 0;
                };

                listBox.DoubleClick += (s, e) =>
                {
                    if (listBox.SelectedIndex >= 0)
                    {
                        form.DialogResult = DialogResult.OK;
                        form.Close();
                    }
                };

                var lbl = new Label();
                lbl.Text = $"Multiple {fileLabel} files found. Select one:";
                lbl.Dock = DockStyle.Top;
                lbl.Height = 30;
                lbl.Padding = new Padding(8, 6, 0, 0);

                var okButton = new Button();
                okButton.Name = "btnOK";
                okButton.Text = "OK";
                okButton.Dock = DockStyle.Bottom;
                okButton.Height = 35;
                okButton.Enabled = false;
                okButton.Click += (s, e) =>
                {
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                var cancelButton = new Button();
                cancelButton.Text = "Cancel";
                cancelButton.Dock = DockStyle.Bottom;
                cancelButton.Height = 35;
                cancelButton.Click += (s, e) =>
                {
                    form.DialogResult = DialogResult.Cancel;
                    form.Close();
                };

                // WinForms docks controls in reverse addition order:
                // add Fill first, then Top, then Bottom so that
                // Top and Bottom are processed first and Fill gets remaining space.
                form.Controls.Add(listBox);
                form.Controls.Add(lbl);
                form.Controls.Add(okButton);
                form.Controls.Add(cancelButton);

                DarkTheme.ApplyToControl(listBox);

                string chosen = null;
                if (form.ShowDialog() == DialogResult.OK && listBox.SelectedIndex >= 0)
                {
                    chosen = files[listBox.SelectedIndex];
                }
                return chosen;
            }
        }

        private void Load2007(RoomModel2007 roomModel2007) 
        {
            string BaseDirectory = "";
            if (DataBase.DirectoryDic.ContainsKey(roomModel2007.PathKey))
            {
                BaseDirectory = DataBase.DirectoryDic[roomModel2007.PathKey];
            }

            string smdPath = BaseDirectory + roomModel2007.SmdFile;
            string smxPath = BaseDirectory + roomModel2007.SmxFile;

            if (!string.IsNullOrEmpty(dataQingshengDirectory))
            {
                string qSmd = Path.Combine(dataQingshengDirectory, "0000.smd");
                string qSmx = Path.Combine(dataQingshengDirectory, "0000.smx");
                if (File.Exists(qSmd)) smdPath = qSmd;
                if (File.Exists(qSmx)) smxPath = qSmx;
            }

            smdPath = ResolveRoomFile(smdPath, "SMD");
            smxPath = ResolveRoomFile(smxPath, "SMX");

            SetLoadingText("Loading " + Path.GetFileName(smdPath));

            RE4_2007_MODEL_VIEWER.src.LoadScenarioSMD loadScenarioSMD = new RE4_2007_MODEL_VIEWER.src.LoadScenarioSMD(modelGroup, sng);
            loadScenarioSMD.LoadScenario(smdPath);

            SetLoadingText("Loading SMX...");

            JADERLINK_MODEL_VIEWER.src.LoadSMX loadSMX = new JADERLINK_MODEL_VIEWER.src.LoadSMX(modelGroup, sng);
            loadSMX.LoadSmx(smxPath, true);

            // parte que carrega os pmds

            int SmdAmount = modelGroup.SmdGroup?.SmdEntries?.Count ?? 0;

            RE4_2007_MODEL_VIEWER.src.LoadPmdModel loadPmdModel = new RE4_2007_MODEL_VIEWER.src.LoadPmdModel(modelGroup, mng, tpng);
            loadPmdModel.ExternalAddTreatedModel = extraContainer.AddTreatedModel;

            for (int i = 0; i < SmdAmount; i++)
            {
                string PmdPath = BaseDirectory + roomModel2007.PmdFolder + "\\" + roomModel2007.PmdBaseName + "_" + i.ToString("D3") + ".pmd";
                if (File.Exists(PmdPath))
                {
                    loadPmdModel.LoadPMD(PmdPath);
                }
            }

            //ordem
            modelNodeOrder.GetNodeOrder();
            order.ToOrder(ref modelGroup, modelNodeOrder.NodeOrder);
        }

        private void LoadPS2(RoomModelPs2 roomModelPS2)
        {
            string BaseDirectory = "";
            if (DataBase.DirectoryDic.ContainsKey(roomModelPS2.PathKey))
            {
                BaseDirectory = DataBase.DirectoryDic[roomModelPS2.PathKey];
            }

            string smdPath = BaseDirectory + roomModelPS2.SmdFile;
            string smxPath = BaseDirectory + roomModelPS2.SmxFile;

            if (!string.IsNullOrEmpty(dataQingshengDirectory))
            {
                string qSmd = Path.Combine(dataQingshengDirectory, "0000.smd");
                string qSmx = Path.Combine(dataQingshengDirectory, "0000.smx");
                if (File.Exists(qSmd)) smdPath = qSmd;
                if (File.Exists(qSmx)) smxPath = qSmx;
            }

            smdPath = ResolveRoomFile(smdPath, "SMD");
            smxPath = ResolveRoomFile(smxPath, "SMX");

            SetLoadingText("Loading " + Path.GetFileName(smdPath));

            RE4_PS2_MODEL_VIEWER.src.LoadPs2ScenarioSMD loadScenarioSMD = new RE4_PS2_MODEL_VIEWER.src.LoadPs2ScenarioSMD(modelGroup, sng);
            loadScenarioSMD.ExternalAddTreatedModel = extraContainer.AddTreatedModel;
            loadScenarioSMD.LoadScenario(smdPath);

            SetLoadingText("Loading SMX...");

            JADERLINK_MODEL_VIEWER.src.LoadSMX loadSMX = new JADERLINK_MODEL_VIEWER.src.LoadSMX(modelGroup, sng);
            loadSMX.LoadSmx(smxPath, true);

            //ordem
            modelNodeOrder.GetNodeOrder();
            order.ToOrder(ref modelGroup, modelNodeOrder.NodeOrder);
        }

        private void LoadUHD(RoomModelUhd roomModelUhd)
        {
            bool IsPS4NS = roomModelUhd.Type == RoomModel.EType.PS4NS;

            string BaseDirectory = "";
            if (DataBase.DirectoryDic.ContainsKey(roomModelUhd.PathKey))
            {
                BaseDirectory = DataBase.DirectoryDic[roomModelUhd.PathKey];
            }

            string smdPath = BaseDirectory + roomModelUhd.SmdFile;
            string smxPath = BaseDirectory + roomModelUhd.SmxFile;

            if (!string.IsNullOrEmpty(dataQingshengDirectory))
            {
                string qSmd = Path.Combine(dataQingshengDirectory, "0000.smd");
                string qSmx = Path.Combine(dataQingshengDirectory, "0000.smx");
                if (File.Exists(qSmd)) smdPath = qSmd;
                if (File.Exists(qSmx)) smxPath = qSmx;
            }

            if (!File.Exists(smdPath))
            {
                try
                {
                    string roomFolder = Path.GetDirectoryName(smdPath);
                    if (!string.IsNullOrEmpty(roomFolder))
                    {
                        string roomName = Path.GetFileName(roomFolder);
                        string stagePath = Path.GetDirectoryName(roomFolder);
                        EditorRe4Ver gameVersion = IsPS4NS ? EditorRe4Ver.PS4NS : EditorRe4Ver.UHD;

                        ExternalToolManager.EnsureRoomExtractedSync(stagePath, roomName, gameVersion, 0);
                    }
                }
                catch (Exception ex)
                {
                    EditorConsole.Error("Automatic room extraction failed: " + ex.Message);
                }
            }

            smdPath = ResolveRoomFile(smdPath, "SMD");
            smxPath = ResolveRoomFile(smxPath, "SMX");

            SetLoadingText("Loading " + Path.GetFileName(smdPath));

            LoadUhdPs4Ns.src.LoadUhdScenarioSMD loadScenarioSMD = new LoadUhdPs4Ns.src.LoadUhdScenarioSMD(modelGroup, sng, IsPS4NS);
            loadScenarioSMD.ExternalAddTreatedModel = extraContainer.AddTreatedModel;
            loadScenarioSMD.LoadScenario(smdPath);

            SetLoadingText("Loading SMX...");

            JADERLINK_MODEL_VIEWER.src.LoadSMX loadSMX = new JADERLINK_MODEL_VIEWER.src.LoadSMX(modelGroup, sng);
            loadSMX.LoadSmx(smxPath, false);

            SetLoadingText("Loading textures...");

            //texturas
            HashSet<string> packIds = new HashSet<string>();
            Dictionary<string, List<int>> packId_texId = new Dictionary<string, List<int>>();

            foreach (var i1 in modelGroup.MatTexGroupDic.Values)
            {
                foreach (var i2 in i1.MatTexDic.Values)
                {
                    var split = i2.TextureName.Split('/');
                    string packID = split[0];
                    packIds.Add(packID);

                    int texId = int.Parse(split[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                    if (packId_texId.ContainsKey(packID))
                    {
                        packId_texId[packID].Add(texId);
                    }
                    else
                    {
                        packId_texId.Add(packID, new List<int> { texId });
                    }
                }
            }

            if (IsPS4NS == false) // UHD
            {
                RE4_UHD_MODEL_VIEWER.src.LoadUhdPackCustomQuad loadUhdPack = new RE4_UHD_MODEL_VIEWER.src.LoadUhdPackCustomQuad(modelGroup, tpng);
                string packFolderPath = Path.Combine(BaseDirectory, roomModelUhd.PackFolder ?? "");
                foreach (var item in packIds)
                {
                    if (!string.IsNullOrEmpty(roomModelUhd.PackFolder))
                    {
                        try
                        {
                            ExternalToolManager.EnsurePackExtracted(packFolderPath, item);
                        }
                        catch (Exception ex)
                        {
                            EditorConsole.Error("Automatic pack extraction failed: " + ex.Message);
                        }
                    }

                    var Packyz2Filepath = Path.Combine(BaseDirectory, roomModelUhd.PackFolder ?? "", item + ".pack.yz2");
                    var PackFilepath = Path.Combine(BaseDirectory, roomModelUhd.PackFolder ?? "", item + ".pack");

                    if (File.Exists(Packyz2Filepath))
                    {
                        loadUhdPack.LoadPack(Packyz2Filepath, packId_texId[item].ToArray());
                    }
                    if (File.Exists(PackFilepath))
                    {
                        loadUhdPack.LoadPack(PackFilepath, packId_texId[item].ToArray());
                    }
                }
            }
            else // PS4NS
            {
                RE4_PS4NS_MODEL_VIEWER.src.LoadPs4NsPackCustomQuad loadPack = new RE4_PS4NS_MODEL_VIEWER.src.LoadPs4NsPackCustomQuad(modelGroup, tpng);
                foreach (var item in packIds)
                {
                    var PackFilepath = Path.Combine(BaseDirectory, roomModelUhd.PackFolder, item + ".pack");
                    if (File.Exists(PackFilepath))
                    {
                        loadPack.LoadPack(PackFilepath, packId_texId[item].ToArray());
                    }
                }
            }


            //ordem
            modelNodeOrder.GetNodeOrder();
            order.ToOrder(ref modelGroup, modelNodeOrder.NodeOrder);
        }

        private void LoadR100UHD(RoomModelR100Uhd roomModelR100Uhd)
        {
            bool IsPS4NS = roomModelR100Uhd.Type == RoomModel.EType.R100PS4NS;

            string BaseDirectory = "";
            if (DataBase.DirectoryDic.ContainsKey(roomModelR100Uhd.PathKey))
            {
                BaseDirectory = DataBase.DirectoryDic[roomModelR100Uhd.PathKey];
            }

            string smdPath = BaseDirectory + roomModelR100Uhd.SmdFile;
            string SharedSmd = BaseDirectory + roomModelR100Uhd.SharedSmd;
            string smxPath = BaseDirectory + roomModelR100Uhd.SmxFile;

            if (!string.IsNullOrEmpty(dataQingshengDirectory))
            {
                string qSmd = Path.Combine(dataQingshengDirectory, "0000.smd");
                string qSmx = Path.Combine(dataQingshengDirectory, "0000.smx");
                if (File.Exists(qSmd)) smdPath = qSmd;
                if (File.Exists(qSmx)) smxPath = qSmx;
            }

            if (!File.Exists(smdPath))
            {
                try
                {
                    string roomFolder = Path.GetDirectoryName(smdPath);
                    if (!string.IsNullOrEmpty(roomFolder))
                    {
                        string roomName = Path.GetFileName(roomFolder);
                        string stagePath = Path.GetDirectoryName(roomFolder);
                        EditorRe4Ver gameVersion = IsPS4NS ? EditorRe4Ver.PS4NS : EditorRe4Ver.UHD;

                        ExternalToolManager.EnsureRoomExtractedSync(stagePath, roomName, gameVersion, 0);
                    }
                }
                catch (Exception ex)
                {
                    EditorConsole.Error("Automatic room extraction failed: " + ex.Message);
                }
            }

            smdPath = ResolveRoomFile(smdPath, "SMD");
            SharedSmd = ResolveRoomFile(SharedSmd, "SharedSMD");
            smxPath = ResolveRoomFile(smxPath, "SMX");

            SetLoadingText("Loading " + Path.GetFileName(smdPath));

            int binCount = 0;
            int smdCount = 0;
            ModelLoaderNewAge.LoadUhdR100Custom loadScenarioSMD = new ModelLoaderNewAge.LoadUhdR100Custom(modelGroup, sng, IsPS4NS);
            loadScenarioSMD.ExternalAddTreatedModel = extraContainer.AddTreatedModel;
            loadScenarioSMD.LoadScenarioShared(SharedSmd, out binCount);


            int binPos = binCount;
            int SmdPos = smdCount;
            loadScenarioSMD.LoadScenarioParts(smdPath, -1, SmdPos, binPos, out smdCount, out binCount);

            int index = 0;
            foreach (var item in roomModelR100Uhd.DatSmd)
            {
                string datSmdPath = BaseDirectory + item;

                binPos += binCount;
                SmdPos += smdCount;
                loadScenarioSMD.LoadScenarioParts(datSmdPath, index, SmdPos, binPos, out smdCount, out binCount);

                index++;
            }

            SetLoadingText("Loading SMX...");

            JADERLINK_MODEL_VIEWER.src.LoadSMX loadSMX = new JADERLINK_MODEL_VIEWER.src.LoadSMX(modelGroup, sng);
            loadSMX.LoadSmx(smxPath, false);

            //texturas
            HashSet<string> packIds = new HashSet<string>();
            Dictionary<string, List<int>> packId_texId = new Dictionary<string, List<int>>();

            foreach (var i1 in modelGroup.MatTexGroupDic.Values)
            {
                foreach (var i2 in i1.MatTexDic.Values)
                {
                    var split = i2.TextureName.Split('/');
                    string packID = split[0];
                    packIds.Add(packID);

                    int texId = int.Parse(split[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                    if (packId_texId.ContainsKey(packID))
                    {
                        packId_texId[packID].Add(texId);
                    }
                    else
                    {
                        packId_texId.Add(packID, new List<int> { texId });
                    }
                }
            }

            if (IsPS4NS == false) // UHD
            {
                RE4_UHD_MODEL_VIEWER.src.LoadUhdPackCustomQuad loadUhdPack = new RE4_UHD_MODEL_VIEWER.src.LoadUhdPackCustomQuad(modelGroup, tpng);
                string packFolderPath = Path.Combine(BaseDirectory, roomModelR100Uhd.PackFolder ?? "");
                foreach (var item in packIds)
                {
                    if (!string.IsNullOrEmpty(roomModelR100Uhd.PackFolder))
                    {
                        try
                        {
                            ExternalToolManager.EnsurePackExtracted(packFolderPath, item);
                        }
                        catch (Exception ex)
                        {
                            EditorConsole.Error("Automatic pack extraction failed: " + ex.Message);
                        }
                    }

                    var Packyz2Filepath = Path.Combine(BaseDirectory, roomModelR100Uhd.PackFolder ?? "", item + ".pack.yz2");
                    var PackFilepath = Path.Combine(BaseDirectory, roomModelR100Uhd.PackFolder ?? "", item + ".pack");

                    if (File.Exists(Packyz2Filepath))
                    {
                        loadUhdPack.LoadPack(Packyz2Filepath, packId_texId[item].ToArray());
                    }
                    if (File.Exists(PackFilepath))
                    {
                        loadUhdPack.LoadPack(PackFilepath, packId_texId[item].ToArray());
                    }
                }
            }
            else // PS4NS
            {
                RE4_PS4NS_MODEL_VIEWER.src.LoadPs4NsPackCustomQuad loadPack = new RE4_PS4NS_MODEL_VIEWER.src.LoadPs4NsPackCustomQuad(modelGroup, tpng);
                foreach (var item in packIds)
                {
                    var PackFilepath = Path.Combine(BaseDirectory, roomModelR100Uhd.PackFolder, item + ".pack");
                    if (File.Exists(PackFilepath))
                    {
                        loadPack.LoadPack(PackFilepath, packId_texId[item].ToArray());
                    }
                }
            }


            //ordem
            modelNodeOrder.GetNodeOrder();
            order.ToOrder(ref modelGroup, modelNodeOrder.NodeOrder);
        }


        public void ClearGL() 
        {
            foreach (NodeItem i in mng.Nodes.List)
            {
                i.Responsibility.ReleaseResponsibilities();
            }

            foreach (NodeItem i in tpng.Nodes.List)
            {
                i.Responsibility.ReleaseResponsibilities();
            }

            foreach (NodeItem i in sng.Nodes.List)
            {
                i.Responsibility.ReleaseResponsibilities();
            }
        }

        public void Render_Solid() 
        {
            if (RenderWireframe)
            {
                GL.Disable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.FrontAndBack);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                if (RenderOnlyFrontFace)
                {
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Front);
                }
                else
                {
                    GL.CullFace(CullFaceMode.FrontAndBack);
                    GL.Disable(EnableCap.CullFace);
                }
            }

            GL.FrontFace(FrontFaceDirection.Cw);

            DataShader.ShaderRoomSelectMode.Use();
            DataShader.ShaderRoomSelectMode.SetVector4("mColor", new Vector4(0, 0, 0, 1));
            DataShader.ShaderRoomSelectMode.SetMatrix4("mRotation", Matrix4.Identity);
            DataShader.ShaderRoomSelectMode.SetVector3("mScale", Vector3.One);
            DataShader.ShaderRoomSelectMode.SetVector3("mPosition", Vector3.Zero);

            //lista de modelos
            foreach (var item in order.MeshOrder)
            {
                //chamadas da textura
                var mesh = modelGroup.MeshParts[item.mesh];

                if (RenderWireframe == false && RenderOnlyFrontFace == true)
                {
                    if (item.smxEntry.FaceCulling == SmxFaceCulling.OnlyBack)
                    {
                        GL.Enable(EnableCap.CullFace);
                        GL.CullFace(CullFaceMode.Back);
                    }
                    else if (item.smxEntry.FaceCulling == SmxFaceCulling.FrontAndBack)
                    {
                        GL.CullFace(CullFaceMode.FrontAndBack);
                        GL.Disable(EnableCap.CullFace);
                    }
                    else // only front
                    {
                        GL.Enable(EnableCap.CullFace);
                        GL.CullFace(CullFaceMode.Front);
                    }
                }

                //modelo
                DataShader.ShaderRoomSelectMode.SetMatrix4("mRotation", item.smdEntry.Fix.GetRotation());
                DataShader.ShaderRoomSelectMode.SetVector3("mScale", item.smdEntry.Fix.Scale);
                DataShader.ShaderRoomSelectMode.SetVector3("mPosition", item.smdEntry.Fix.Position);
                mesh.Render();
            }


        }

        public void Render()
        {
            if (RenderWireframe)
            {
                GL.Disable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.FrontAndBack);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

                if (RenderOnlyFrontFace)
                {
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Front);
                }
                else
                {
                    GL.CullFace(CullFaceMode.FrontAndBack);
                    GL.Disable(EnableCap.CullFace);
                }
            }

            GL.FrontFace(FrontFaceDirection.Cw);

            DataShader.ShaderRoom.Use();
            DataShader.ShaderRoom.SetMatrix4("mRotation", Matrix4.Identity);
            DataShader.ShaderRoom.SetVector3("mScale", Vector3.One);
            DataShader.ShaderRoom.SetVector3("mPosition", Vector3.Zero);
            DataShader.ShaderRoom.SetVector4("matColor", Vector4.One);

            if (RenderNormals)
            {
                DataShader.ShaderRoom.SetInt("EnableNormals", 1);
            }
            else
            {
                DataShader.ShaderRoom.SetInt("EnableNormals", 0);
            }

            if (RenderVertexColor)
            {
                DataShader.ShaderRoom.SetInt("EnableVertexColors", 1);
            }
            else
            {
                DataShader.ShaderRoom.SetInt("EnableVertexColors", 0);
            }

            if (RenderAlphaChannel)
            {
                DataShader.ShaderRoom.SetInt("EnableAlphaChannel", 1);
            }
            else
            {
                DataShader.ShaderRoom.SetInt("EnableAlphaChannel", 0);
            }

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.001f); //Gequal
            GL.Enable(EnableCap.Texture2D);

            //tex
            DataShader.WhiteTexture.Use(TextureUnit.Texture0);
            DataShader.WhiteTexture.Use(TextureUnit.Texture1);

            //lista de modelos
            foreach (var item in order.MeshOrder)
            {
                //chamadas da textura
                var mesh = modelGroup.MeshParts[item.mesh];
                var modelID = mesh.RefModelID;
                var material = modelGroup.MaterialGroupDic[modelID].MaterialsDic[mesh.MaterialRef];

                if (RenderTextures)
                {
                    DataShader.ShaderRoom.SetVector4("matColor", material.MatColor);

                    DataShader.WhiteTexture.Use(TextureUnit.Texture0);
                    DataShader.WhiteTexture.Use(TextureUnit.Texture1);
                    {
                        var matTexGroupName = modelGroup.MatLinkerDic[modelID].MatTexGroupName;
                        if (modelGroup.MatTexGroupDic.ContainsKey(matTexGroupName))
                        {
                            var MatTexGroup = modelGroup.MatTexGroupDic[matTexGroupName];

                            if (MatTexGroup.MatTexDic.ContainsKey(material.DiffuseMatTex))
                            {
                                var matTex = MatTexGroup.MatTexDic[material.DiffuseMatTex];
                                if (modelGroup.TextureRefDic.ContainsKey(matTex.TextureName))
                                {
                                    var texture = modelGroup.TextureRefDic[matTex.TextureName];
                                    texture.Use(TextureUnit.Texture0);
                                }
                            }

                            if (material.AsAlphaTex)
                            {
                                if (MatTexGroup.MatTexDic.ContainsKey(material.AlphaMatTex))
                                {
                                    var matTex = MatTexGroup.MatTexDic[material.AlphaMatTex];
                                    if (modelGroup.TextureRefDic.ContainsKey(matTex.TextureName))
                                    {
                                        var texture = modelGroup.TextureRefDic[matTex.TextureName];
                                        texture.Use(TextureUnit.Texture1);
                                    }
                                }
                            }

                        }
                    }

                }

                if (RenderWireframe == false && RenderOnlyFrontFace == true)
                {
                    if (item.smxEntry.FaceCulling == SmxFaceCulling.OnlyBack)
                    {
                        GL.Enable(EnableCap.CullFace);
                        GL.CullFace(CullFaceMode.Back);
                    }
                    else if (item.smxEntry.FaceCulling == SmxFaceCulling.FrontAndBack)
                    {
                        GL.CullFace(CullFaceMode.FrontAndBack);
                        GL.Disable(EnableCap.CullFace);
                    }
                    else // only front
                    {
                        GL.Enable(EnableCap.CullFace);
                        GL.CullFace(CullFaceMode.Front);
                    }
                }

                //modelo
                DataShader.ShaderRoom.SetMatrix4("mRotation", item.smdEntry.Fix.GetRotation());
                DataShader.ShaderRoom.SetVector3("mScale", item.smdEntry.Fix.Scale);
                DataShader.ShaderRoom.SetVector3("mPosition", item.smdEntry.Fix.Position);
                mesh.Render();
            }

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.Blend);
        }

        public float DropToGround(Vector3 pos) 
        {
            return extraContainer.DropToGround(pos);
        }

        public ushort GetRoomId() 
        {
            return roomModel.HexID;
        }

        public void ChangeTextureType()
        {
            if (modelGroup?.TextureRefDic?.Values != null)
            {
                if (LoadTextureLinear)
                {
                    foreach (var item in modelGroup?.TextureRefDic?.Values)
                    {
                        item.SetLinear();
                    }
                }
                else
                {
                    foreach (var item in modelGroup?.TextureRefDic?.Values)
                    {
                        item.SetNearest();
                    }
                }
            }  
        }


        private class ExtraTreatedModelContainer 
        {
            private ModelGroup modelGroup = null;
            private Dictionary<string, TreatedModel> treatedModels = null;
            
            public ExtraTreatedModelContainer(ModelGroup modelGroup) 
            {
                this.modelGroup = modelGroup;
                treatedModels = new Dictionary<string, TreatedModel>();
            }

            public void AddTreatedModel(TreatedModel treatedModel) 
            {
                if (!treatedModels.ContainsKey(treatedModel.ModelID))
                {
                    treatedModels.Add(treatedModel.ModelID, treatedModel);
                }
            }

            #region DropToGround
            public float DropToGround(Vector3 pos)
            {
                List<float> found = new List<float>();

                for (int i = 0; i < (modelGroup?.SmdGroup?.SmdEntries?.Count ?? 0); i++)
                {
                    if (modelGroup?.SmdGroup?.SmdEntries?.ContainsKey(i) ?? false)
                    {
                        var Fix = modelGroup.SmdGroup.SmdEntries[i].Fix;
                        int BinID = modelGroup.SmdGroup.SmdEntries[i].BIN_ID;

                        if (modelGroup.ScenarioBinList.ContainsKey(BinID) 
                            && treatedModels.ContainsKey(modelGroup.ScenarioBinList[BinID]))
                        {
                            var treated = treatedModels[modelGroup.ScenarioBinList[BinID]];
                           
                            var min = ApplyTransformations(treated.MinBoundary, Fix); 
                            var max = ApplyTransformations(treated.MaxBoundary, Fix);
                            if (pos.X > min.X && pos.Z > min.Z
                             && pos.X < max.X && pos.Z < max.Z)
                            {
                                for (int im = 0; im < treated.Meshes.Count; im++)
                                {
                                    var mmin = ApplyTransformations(treated.Meshes[im].MinBoundary, Fix);
                                    var mmax = ApplyTransformations(treated.Meshes[im].MaxBoundary, Fix);

                                    if (pos.X > mmin.X && pos.Z > mmin.Z
                                     && pos.X < mmax.X && pos.Z < mmax.Z)
                                    {
                                        for (int j = 0; j < treated.Meshes[im].IndexesLength; j += 3)
                                        {
                                            tempTriangle temp;
                                            uint index1 = treated.Meshes[im].Indexes[j];
                                            uint index2 = treated.Meshes[im].Indexes[j + 1];
                                            uint index3 = treated.Meshes[im].Indexes[j + 2];
                                            int numVertices = treated.Meshes[im].Vertex.Length / 12;
                                            if (index1 >= numVertices || index2 >= numVertices || index3 >= numVertices) { continue; }

                                            temp.a = ApplyTransformations(treated.Meshes[im].Vertex[index1 * 12], treated.Meshes[im].Vertex[(index1 * 12) + 1], treated.Meshes[im].Vertex[(index1 * 12) + 2], Fix);
                                            temp.b = ApplyTransformations(treated.Meshes[im].Vertex[index2 * 12], treated.Meshes[im].Vertex[(index2 * 12) + 1], treated.Meshes[im].Vertex[(index2 * 12) + 2], Fix);
                                            temp.c = ApplyTransformations(treated.Meshes[im].Vertex[index3 * 12], treated.Meshes[im].Vertex[(index3 * 12) + 1], treated.Meshes[im].Vertex[(index3 * 12) + 2], Fix);
                                            if (PointInTriangle(pos.Xz, temp.a.Xz, temp.b.Xz, temp.c.Xz))
                                            {
                                                found.Add(barryCentric(temp.a, temp.b, temp.c, pos));
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

               
                if (found.Count == 0)
                { return pos.Y; }

                int closest_index = 0;
                float closest_abs = 9999999.0f;
                // Console.WriteLine("Found " + found.Count + " triangles under position");
                for (int i = 0; i < found.Count; i++)
                {
                    float abs = Math.Abs(pos.Y - found[i]);
                    if (abs < closest_abs)
                    {
                        closest_abs = abs;
                        closest_index = i;
                    }
                }
                return found[closest_index];
            }

            private static Vector3 ApplyTransformations(float x, float y, float z, PreFix fix) 
            {
                Vector4 temp = new Vector4(x * 100f, y * 100f, z * 100f, 1);
                temp *= fix.GetRotation();
                temp.X *= fix.Scale.X;
                temp.Y *= fix.Scale.Y;
                temp.Z *= fix.Scale.Z;
                temp.X += (fix.Position.X * 100f);
                temp.Y += (fix.Position.Y * 100f);
                temp.Z += (fix.Position.Z * 100f);
                return new Vector3(temp.X, temp.Y, temp.Z);
            }

            private static Vector3 ApplyTransformations(Vector3 pos, PreFix fix) 
            {
                return ApplyTransformations(pos.X, pos.Y, pos.Z, fix);
            }

            private struct tempTriangle
            {
                public Vector3 a, b, c;
            }

            private static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
            {
                var s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
                var t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

                if ((s < 0) != (t < 0))
                    return false;

                var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
                if (A < 0.0)
                {
                    s = -s;
                    t = -t;
                    A = -A;
                }
                return s > 0 && t > 0 && (s + t) <= A;
            }

            private static float barryCentric(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 pos)
            {
                float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
                float l1 = ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Z - p3.Z)) / det;
                float l2 = ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Z - p3.Z)) / det;
                float l3 = 1.0f - l1 - l2;
                return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
            }

            #endregion
        }
    }
}
