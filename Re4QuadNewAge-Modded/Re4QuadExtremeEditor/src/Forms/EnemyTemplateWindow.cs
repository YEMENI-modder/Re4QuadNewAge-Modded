using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Re4QuadExtremeEditor.src.Class.EnemyTemplates;
using Re4QuadExtremeEditor.src.Class.TreeNodeObj;
using Re4QuadExtremeEditor.src.Class.Enums;

namespace Re4QuadExtremeEditor.src.Forms
{
    public class EnemyTemplateWindow : Window
    {
        private UiTheme.Palette P;
        private ListBox cardList;
        private TextBox txtSearch, txtEditName, txtEditDesc;
        private TextBlock txtDetailId, txtDetailLife;
        private ComboBox cmbCategory;
        private List<EnemyTemplate> _filtered;
        private EnemyTemplate _selected;
        private string _activeFilter = "All";

        public EnemyTemplateWindow()
        {
            P = UiTheme.CreatePalette();

            WindowStyle = WindowStyle.None;
            Topmost = true;
            Width = 740;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = P.BWindow;
            Foreground = P.BText;
            FontFamily = new FontFamily("Segoe UI");
            FontSize = 13;

            BuildUI();
            RefreshFilter();
        }

        private void BuildUI()
        {
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // title bar
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // search+filter
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // content
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });  // buttons

            // === Row 0: Custom title bar ===
            Grid titleBar = new Grid { Height = 32, Background = P.BBar };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleBar.MouseDown += (s, e) => { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); };
            Grid.SetRow(titleBar, 0);

            TextBlock ttl = new TextBlock { Text = "   Enemy Templates", VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold, FontSize = 13, Foreground = P.BText };
            Grid.SetColumn(ttl, 0);
            titleBar.Children.Add(ttl);

            Button xb = new Button { Content = "\u2715", Width = 32, Height = 32, FontSize = 14, Foreground = P.BText, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand };
            xb.Click += (s, e) => Close();
            xb.MouseEnter += (s, e) => xb.Background = P.BPressSurface;
            xb.MouseLeave += (s, e) => xb.Background = Brushes.Transparent;
            Grid.SetColumn(xb, 1);
            titleBar.Children.Add(xb);
            root.Children.Add(titleBar);

            // === Row 1: Search + Filter ===
            Grid searchRow = new Grid { Margin = new Thickness(10, 8, 10, 6), Background = P.BSurface };
            searchRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            searchRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetRow(searchRow, 1);

            txtSearch = new TextBox
            {
                Height = 28,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = P.BInput,
                Foreground = P.BText,
                BorderBrush = P.BBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6, 0, 6, 0)
            };
            txtSearch.TextChanged += (s, e) => RefreshFilter();
            Grid.SetColumn(txtSearch, 0);
            searchRow.Children.Add(txtSearch);

            StackPanel filterBtns = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (string cat in new[] { "All", "Village", "Castle", "Island" })
            {
                Button b = new Button
                {
                    Content = cat,
                    Height = 28,
                    MinWidth = 65,
                    Margin = new Thickness(4, 0, 0, 0),
                    Padding = new Thickness(8, 0, 8, 0),
                    Tag = cat,
                    FontSize = 11
                };
                StyleFilterBtn(b);
                b.Click += (s, e) => { _activeFilter = (string)((Button)s).Tag; RefreshFilter(); };
                filterBtns.Children.Add(b);
            }
            Grid.SetColumn(filterBtns, 1);
            searchRow.Children.Add(filterBtns);

            root.Children.Add(searchRow);

            // === Row 2: Content (list + detail) ===
            Grid contentGrid = new Grid { Margin = new Thickness(10, 0, 10, 6) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
            Grid.SetRow(contentGrid, 2);

            cardList = new ListBox
            {
                BorderThickness = new Thickness(1),
                BorderBrush = P.BBorder,
                Background = P.BSurface,
                Foreground = P.BText,
                Padding = new Thickness(4)
            };
            cardList.SelectionChanged += CardList_SelectionChanged;
            Grid.SetColumn(cardList, 0);
            contentGrid.Children.Add(cardList);

            Border detailBorder = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = P.BBorder,
                Background = P.BSurface,
                Margin = new Thickness(8, 0, 0, 0),
                CornerRadius = new CornerRadius(3)
            };
            ScrollViewer detailScroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(10) };
            detailBorder.Child = detailScroll;
            StackPanel detail = new StackPanel();
            detailScroll.Content = detail;
            Grid.SetColumn(detailBorder, 1);
            contentGrid.Children.Add(detailBorder);

            detail.Children.Add(new TextBlock { Text = "SELECTED", FontWeight = FontWeights.Bold, FontSize = 11, Foreground = P.BSub, Margin = new Thickness(0, 0, 0, 8) });

            detail.Children.Add(MakeLabel("Name:"));
            txtEditName = new TextBox { Height = 26, Margin = new Thickness(0, 0, 0, 6), Background = P.BInput, Foreground = P.BText, BorderBrush = P.BBorder, BorderThickness = new Thickness(1), Padding = new Thickness(6, 0, 6, 0), VerticalContentAlignment = VerticalAlignment.Center };
            detail.Children.Add(txtEditName);

            detail.Children.Add(MakeLabel("Category:"));
            cmbCategory = new ComboBox { Height = 28, Margin = new Thickness(0, 0, 0, 6), Background = P.BInput, Foreground = P.BText, BorderBrush = P.BBorder, BorderThickness = new Thickness(1) };
            cmbCategory.Items.Add("Village");
            cmbCategory.Items.Add("Castle");
            cmbCategory.Items.Add("Island");
            cmbCategory.SelectedIndex = 0;
            StyleComboBox(cmbCategory);
            detail.Children.Add(cmbCategory);

            detail.Children.Add(MakeLabel("Enemy:"));
            txtDetailId = new TextBlock { Margin = new Thickness(0, 2, 0, 8), Foreground = P.BText, Text = "-" };
            detail.Children.Add(txtDetailId);

            detail.Children.Add(MakeLabel("Life:"));
            txtDetailLife = new TextBlock { Margin = new Thickness(0, 2, 0, 8), Foreground = P.BText, Text = "-" };
            detail.Children.Add(txtDetailLife);

            detail.Children.Add(MakeLabel("Description:"));
            txtEditDesc = new TextBox { Height = 50, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 0, 0, 0), Background = P.BInput, Foreground = P.BText, BorderBrush = P.BBorder, BorderThickness = new Thickness(1), Padding = new Thickness(6, 4, 6, 4) };
            detail.Children.Add(txtEditDesc);

            root.Children.Add(contentGrid);

            // === Row 3: Action buttons ===
            StackPanel btnBar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 0, 10, 10) };
            Grid.SetRow(btnBar, 3);

            btnBar.Children.Add(MakeActionBtn("Save from Enemy", P.MAccent, BtnSave_Click));
            btnBar.Children.Add(MakeActionBtn("Apply to Enemy", P.MAccent, BtnApply_Click));
            btnBar.Children.Add(MakeActionBtn("Delete", Color.FromRgb(160, 50, 50), BtnDelete_Click));

            root.Children.Add(btnBar);

            Content = root;
        }

        // --- Actions ---
        private void CardList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cardList.SelectedItem as ListBoxItem;
            _selected = item?.Tag as EnemyTemplate;
            if (_selected != null)
            {
                txtEditName.Text = _selected.Name;
                txtEditDesc.Text = _selected.Description;
                txtDetailId.Text = "0x" + _selected.EnemyId.ToString("X4") + " - " + _selected.EnemyName;
                txtDetailLife.Text = _selected.Life.ToString();
                for (int i = 0; i < cmbCategory.Items.Count; i++)
                    if ((string)cmbCategory.Items[i] == _selected.Category) { cmbCategory.SelectedIndex = i; break; }
            }
            else
            {
                txtEditName.Text = "";
                txtEditDesc.Text = "";
                txtDetailId.Text = "-";
                txtDetailLife.Text = "-";
                cmbCategory.SelectedIndex = 0;
            }
        }

        private void RefreshFilter()
        {
            string search = txtSearch != null ? txtSearch.Text : "";
            _filtered = EnemyTemplateLibrary.Search(_activeFilter, search);
            cardList.Items.Clear();
            foreach (var t in _filtered)
            {
                cardList.Items.Add(new ListBoxItem
                {
                    Content = t.Name,
                    Tag = t,
                    Padding = new Thickness(6, 5, 6, 5),
                    Margin = new Thickness(0, 0, 0, 2),
                    Background = P.BSurface,
                    Foreground = P.BText,
                    BorderThickness = new Thickness(1),
                    BorderBrush = P.BBorder
                });
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Object3D obj = DataBase.LastSelectNode as Object3D;
            if (obj == null || obj.Group != GroupType.ESL) return;
            if (DataBase.FileESL == null || !DataBase.FileESL.Lines.ContainsKey(obj.ObjLineRef)) return;

            ushort eid = obj.ObjLineRef;
            byte[] line = DataBase.FileESL.Lines[eid];
            string ename = "Unknown";
            if (DataBase.EnemiesIDs != null && DataBase.EnemiesIDs.List.ContainsKey(eid))
                ename = DataBase.EnemiesIDs.List[eid].Name;

            SaveDialog dlg = new SaveDialog(P, ename, "0x" + eid.ToString("X4"));
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                var t = EnemyTemplate.FromEnemy(eid, ename, line);
                t.Name = dlg.TemplateName;
                t.Description = dlg.TemplateDesc;
                t.Category = dlg.TemplateCategory;
                EnemyTemplateLibrary.Save(t);
                RefreshFilter();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;
            _selected.Name = txtEditName.Text.Trim();
            _selected.Description = txtEditDesc.Text.Trim();
            _selected.Category = (string)cmbCategory.SelectedItem;
            EnemyTemplateLibrary.Save(_selected);
            RefreshFilter();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;
            Object3D obj = DataBase.LastSelectNode as Object3D;
            if (obj == null || obj.Group != GroupType.ESL) return;
            if (DataBase.FileESL == null || !DataBase.FileESL.Lines.ContainsKey(obj.ObjLineRef)) return;
            _selected.ApplyToTarget(obj.ObjLineRef);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;
            EnemyTemplateLibrary.Delete(_selected);
            _selected = null;
            RefreshFilter();
        }

        // --- Helpers ---
        private TextBlock MakeLabel(string text)
        {
            return new TextBlock { Text = text, FontWeight = FontWeights.SemiBold, FontSize = 11, Foreground = P.BSub, Margin = new Thickness(0, 0, 0, 2) };
        }

        private void StyleFilterBtn(Button b)
        {
            b.Background = P.BSurface;
            b.Foreground = P.BText;
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = P.BBorder;
            b.Cursor = Cursors.Hand;
        }

        private void StyleComboBox(ComboBox c)
        {
            c.Foreground = P.BText;
            c.Background = P.BInput;
            c.BorderBrush = P.BBorder;

            Style itemStyle = new Style(typeof(ComboBoxItem));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, Brushes.Black));
            itemStyle.Setters.Add(new Setter(ComboBoxItem.PaddingProperty, new Thickness(6, 4, 6, 4)));

            Trigger hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            hover.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, Brushes.Black));
            itemStyle.Triggers.Add(hover);

            Trigger selected = new Trigger { Property = Selector.IsSelectedProperty, Value = true };
            selected.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, Brushes.Black));
            itemStyle.Triggers.Add(selected);

            c.ItemContainerStyle = itemStyle;
        }

        private Button MakeActionBtn(string text, Color bg, RoutedEventHandler click)
        {
            Button b = new Button
            {
                Content = text,
                Height = 30,
                MinWidth = 80,
                Margin = new Thickness(4, 0, 0, 0),
                Padding = new Thickness(10, 0, 10, 0),
                Background = new SolidColorBrush(bg),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontSize = 11
            };
            b.Click += click;
            return b;
        }

        // --- Save Dialog ---
        internal class SaveDialog : Window
        {
            public string TemplateName { get; private set; }
            public string TemplateDesc { get; private set; }
            public string TemplateCategory { get; private set; }
            private TextBox txtName, txtDesc;
            private ComboBox cmbCat;

            public SaveDialog(UiTheme.Palette p, string enemyName, string enemyId)
            {
                Topmost = true;
                Width = 340;
                Height = 340;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                Background = p.BWindow;
                Foreground = p.BText;
                FontFamily = new FontFamily("Segoe UI");

                Grid root = new Grid();
                root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // content
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // buttons

                // Content
                StackPanel sp = new StackPanel { Margin = new Thickness(14, 12, 14, 0) };
                sp.Children.Add(new TextBlock { Text = "Save Template", FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 0, 0, 10), Foreground = p.BText });
                sp.Children.Add(new TextBlock { Text = enemyName + " (" + enemyId + ")", Foreground = p.BSub, Margin = new Thickness(0, 0, 0, 10) });

                sp.Children.Add(new TextBlock { Text = "Name:", Foreground = p.BSub, FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
                txtName = new TextBox { Text = "New Template", Height = 26, Background = p.BInput, Foreground = p.BText, BorderBrush = p.BBorder, BorderThickness = new Thickness(1), Padding = new Thickness(6, 0, 6, 0), Margin = new Thickness(0, 0, 0, 6), VerticalContentAlignment = VerticalAlignment.Center };
                sp.Children.Add(txtName);

                sp.Children.Add(new TextBlock { Text = "Category:", Foreground = p.BSub, FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
                cmbCat = new ComboBox { Height = 26, Background = p.BInput, Foreground = p.BText, BorderBrush = p.BBorder, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 6) };
                cmbCat.Items.Add("Village");
                cmbCat.Items.Add("Castle");
                cmbCat.Items.Add("Island");
                cmbCat.SelectedIndex = 0;
                StyleCmb(p, cmbCat);
                sp.Children.Add(cmbCat);

                sp.Children.Add(new TextBlock { Text = "Description:", Foreground = p.BSub, FontSize = 11, Margin = new Thickness(0, 0, 0, 2) });
                txtDesc = new TextBox { Height = 40, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Background = p.BInput, Foreground = p.BText, BorderBrush = p.BBorder, BorderThickness = new Thickness(1), Padding = new Thickness(6, 4, 6, 4), Margin = new Thickness(0, 0, 0, 8) };
                sp.Children.Add(txtDesc);

                Grid.SetRow(sp, 0);
                root.Children.Add(sp);

                // Buttons
                StackPanel btns = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(14, 0, 14, 10) };
                Button ok = new Button { Content = "Save", Height = 28, MinWidth = 70, Background = p.BAccent, Foreground = Brushes.White, BorderThickness = new Thickness(0), Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 6, 0), FontSize = 11 };
                ok.Click += (s, ev) => { TemplateName = txtName.Text.Trim(); TemplateDesc = txtDesc.Text.Trim(); TemplateCategory = (string)cmbCat.SelectedItem; DialogResult = true; };
                btns.Children.Add(ok);
                Button cancel = new Button { Content = "Cancel", Height = 28, MinWidth = 70, Background = p.BSurface, Foreground = p.BText, BorderThickness = new Thickness(1), BorderBrush = p.BBorder, Cursor = Cursors.Hand, FontSize = 11 };
                cancel.Click += (s, ev) => { DialogResult = false; };
                btns.Children.Add(cancel);
                Grid.SetRow(btns, 1);
                root.Children.Add(btns);

                Content = root;
            }

            private void StyleCmb(UiTheme.Palette p, ComboBox c)
            {
                c.Foreground = p.BText;
                c.Background = p.BInput;
                c.BorderBrush = p.BBorder;

                Style itemStyle = new Style(typeof(ComboBoxItem));
                itemStyle.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, Brushes.Black));
                itemStyle.Setters.Add(new Setter(ComboBoxItem.PaddingProperty, new Thickness(6, 4, 6, 4)));
                Trigger hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
                hover.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, Brushes.Black));
                itemStyle.Triggers.Add(hover);
                Trigger selected = new Trigger { Property = Selector.IsSelectedProperty, Value = true };
                selected.Setters.Add(new Setter(ComboBoxItem.ForegroundProperty, Brushes.Black));
                itemStyle.Triggers.Add(selected);
                c.ItemContainerStyle = itemStyle;
            }
        }
    }
}
