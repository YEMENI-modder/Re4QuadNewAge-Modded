using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Re4QuadExtremeEditor.src.JSON;
using Re4QuadExtremeEditor.src.Class;
using Re4QuadExtremeEditor.src.Class.Enums;
using System.IO;
using NewAgeTheRender;

namespace Re4QuadExtremeEditor.src.Forms
{
    public partial class SelectRoomForm : Form
    {
        // ------------------------------------------------------------------
        //  LoadJsonFiles stays as a FALLBACK (called only if the startup
        //  cache is not ready yet).  Normally the cache is always ready.
        // ------------------------------------------------------------------
        private List<RoomInfo> LoadJsonFiles() 
        {
            return Utils.LoadRoomInfoList();
        }

        /// <summary>
        /// evendo que acontece depois de clicar em load;
        /// </summary>
        public event EventHandler onLoadButtonClick;

        public SelectRoomForm()
        {
            InitializeComponent();
            KeyPreview = true;

            comboBoxMainList.Items.Add(Lang.GetText(eLang.NoneRoom));
            comboBoxMainList.SelectedIndex = 0;

            // ---------------------------------------------------------------
            //  Use the preloaded cache whenever possible so the form opens
            //  instantly.  Falls back to disk I/O only if cache is not ready.
            // ---------------------------------------------------------------
            var list1 = DataBase.CachedRoomInfoList ?? LoadJsonFiles();
            comboBoxMainList.Items.AddRange(list1.ToArray());

            if (DataBase.SelectedRoom != null)
            {
                //aqui deve selecionar o que ja esta carregado

                var list = comboBoxMainList.Items.OfType<RoomInfo>();
                var obj = list.Where(x => x.RoomListObj != null && DataBase.SelectedRoom.GetRoomListObj() != null 
                && x.RoomListObj.JsonFileName == DataBase.SelectedRoom.GetRoomListObj().JsonFileName).FirstOrDefault();
                if (obj != null)
                {
                    int index = comboBoxMainList.Items.IndexOf(obj);
                    if (index > -1)
                    {
                        comboBoxMainList.SelectedIndex = index;

                        if (comboBoxRoomList.Items.Contains(DataBase.SelectedRoom.GetRoomModel()))
                        {
                            comboBoxRoomList.SelectedIndex = comboBoxRoomList.Items.IndexOf(DataBase.SelectedRoom.GetRoomModel());
                        }
                    }
                }
            }

            if (Lang.LoadedTranslation)
            {
                StartUpdateTranslation();
            }
        }

        object lastSelected = null;

        private void comboBoxMainList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMainList.SelectedItem is RoomInfo r)
            {
                comboBoxRoomList.Items.Clear();
                comboBoxRoomList.Items.Add(Lang.GetText(eLang.NoneRoom));
                comboBoxRoomList.Items.AddRange(r.RoomModelDict.Values.ToArray());
            }
            else 
            {
                comboBoxRoomList.Items.Clear();
                comboBoxRoomList.Items.Add(Lang.GetText(eLang.NoneRoom));
            }

            bool foundIt = false;

            if (lastSelected is RoomModel rm)
            {
                 var list = comboBoxRoomList.Items.OfType<RoomModel>();
                 var obj = list.Where(x => x.JsonFileName == rm.JsonFileName || x.HexID == rm.HexID).FirstOrDefault();
                if (obj != null)
                {
                    int index = comboBoxRoomList.Items.IndexOf(obj);
                    if (index > -1)
                    {
                        comboBoxRoomList.SelectedIndex = index;
                        foundIt = true;
                    }
                }
            }

            if (!foundIt)
            {
                comboBoxRoomList.SelectedIndex = 0;
            }
        }

        private bool Enable_ComboBoxRoomList_SelectedIndexChanged = true;

        private void comboBoxRoomList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enable_ComboBoxRoomList_SelectedIndexChanged)
            {
                lastSelected = comboBoxRoomList.SelectedItem;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            buttonLoad.Enabled = false;
            buttonCancel.Enabled = false;
            comboBoxMainList.Enabled = false;
            comboBoxRoomList.Enabled = false;

            // ---------------------------------------------------------------
            //  Show a "Loading..." indicator so the user knows something is
            //  happening while the 3-D model files are being read from disk.
            //  (GL calls must run on the UI thread so we can't background
            //  the full load without refactoring the loaders – see notes.)
            // ---------------------------------------------------------------
            string originalBtnText = buttonLoad.Text;
            buttonLoad.Text = "Loading...";
            this.UseWaitCursor = true;
            this.Refresh();          // force the form to repaint before blocking

            try
            {
                // remove a antiga
                if (DataBase.SelectedRoom != null)
                {
                    DataBase.SelectedRoom.ClearGL();
                    DataBase.SelectedRoom = null;

                }

                // cria uma nova
                if (comboBoxMainList.SelectedItem is RoomInfo r && comboBoxRoomList.SelectedItem is RoomModel rm)
                {
                    try
                    {
                        DataBase.SelectedRoom = new RoomSelectedObj(rm, r.RoomListObj);
                    }
                    catch (OperationCanceledException) { return; }

                }
            }
            finally
            {
                this.UseWaitCursor = false;
                buttonLoad.Text = originalBtnText;
            }

            onLoadButtonClick?.Invoke(comboBoxRoomList.SelectedItem, new EventArgs());
            Close();
        }

        private void SelectRoomForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void StartUpdateTranslation() 
        {
            this.Text = Lang.GetText(eLang.SelectRoomForm);
            labelInfo.Text = Lang.GetText(eLang.labelInfo);
            labelText1.Text = Lang.GetText(eLang.labelSelectAList);
            labelText2.Text = Lang.GetText(eLang.labelSelectARoom);
            buttonLoad.Text = Lang.GetText(eLang.SelectRoomButtonLoad);
            buttonCancel.Text = Lang.GetText(eLang.SelectRoomButtonCancel);
        }
    }
}
