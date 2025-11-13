using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Newtonsoft.Json;

using UI;

namespace ThemeManager
{
    public partial class MainForm : Form
    {
        string FilePath = "";
        Manifest_V1 _Manifest;
        List<Manifest_V1.Element> Elements;

        TextBox FilePathBox;
        TextBox ThemeNameBox;
        NumericUpDown Version;
        FlowLayoutPanel ElementsPanel;
        Panel MainPanel;

        public MainForm()
        {
            InitializeMainForm();
            CreateNewManifest();

            void InitializeMainForm()
            {
                Text = "Theme Manifest Editor";
                Size = new Size(1200, 800);
                StartPosition = FormStartPosition.CenterScreen;
                MinimumSize = new Size(800, 600);

                CreateMainLayout();

                void CreateMainLayout()
                {
                    var main = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 1,
                        RowCount = 4,
                        Padding = new Padding(10),
                        CellBorderStyle = TableLayoutPanelCellBorderStyle.None
                    };

                    main.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
                    main.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
                    main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                    main.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

                    var jsonPanel = CreateJsonSection();
                    var themePanel = CreateThemeSection();
                    var elementsPanel = CreateElementsSection();
                    var buttonsPanel = CreateButtonsSection();

                    main.Controls.Add(jsonPanel, 0, 0);
                    main.Controls.Add(themePanel, 0, 1);
                    main.Controls.Add(elementsPanel, 0, 2);
                    main.Controls.Add(buttonsPanel, 0, 3);

                    MainPanel = main;

                    Controls.Add(main);

                    Panel CreateJsonSection()
                    {
                        var panel = new Panel
                        {
                            Dock = DockStyle.Fill,
                            BorderStyle = BorderStyle.FixedSingle,
                            Padding = new Padding(10)
                        };

                        var titleLabel = new Label
                        {
                            Text = "JSON File Configuration",
                            Font = new Font(DefaultFont, FontStyle.Bold),
                            Location = new Point(10, 10),
                            AutoSize = true
                        };

                        var fileLabel = new Label
                        {
                            Text = "JSON File:",
                            Location = new Point(10, 40),
                            AutoSize = true
                        };

                        FilePathBox = new TextBox
                        {
                            Location = new Point(80, 37),
                            Width = 500,
                            ReadOnly = true,
                            BackColor = SystemColors.Control
                        };

                        var loadButton = new Button
                        {
                            Text = "Load JSON",
                            Location = new Point(590, 35),
                            Width = 90,
                            Height = 25
                        };
                        var newButton = new Button
                        {
                            Text = "New JSON",
                            Location = new Point(690, 35),
                            Width = 90,
                            Height = 25
                        };
                        var saveButton = new Button
                        {
                            Text = "Save JSON",
                            Location = new Point(790, 35),
                            Width = 90,
                            Height = 25
                        };
                        var saveAsButton = new Button
                        {
                            Text = "Save As...",
                            Location = new Point(890, 35),
                            Width = 90,
                            Height = 25
                        };

                        loadButton.Click += (s, e) => LoadJsonFromFile();
                        newButton.Click += (s, e) => CreateNewManifest();
                        saveButton.Click += (s, e) => SaveJson();
                        saveAsButton.Click += (s, e) => SaveJsonAs();

                        panel.Controls.AddRange(new Control[]
                        {
                            titleLabel,
                            fileLabel,
                            FilePathBox,
                            loadButton,
                            newButton,
                            saveButton,
                            saveAsButton
                        });

                        return panel;

                        void LoadJsonFromFile()
                        {
                            var filePath = ShowOpenFileDialog("JSON files|*.json");
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                FilePath = filePath;
                                FilePathBox.Text = filePath;

                                if (string.IsNullOrEmpty(FilePath) ||
                                    !File.Exists(FilePath))
                                    return;

                                try
                                {
                                    var json = File.ReadAllText(FilePath);

                                    _Manifest = Manifest.Cast(json);
                                    Elements = new List<Manifest_V1.Element>(_Manifest.elements ?? new Manifest_V1.Element[0]);
                                    ThemeNameBox.Text = _Manifest.name;
                                    Version.Value = _Manifest.version;

                                    RefreshElementsView();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Error loading JSON: {ex.Message}",
                                        "Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    Panel CreateThemeSection()
                    {
                        var panel = new Panel
                        {
                            Dock = DockStyle.Fill,
                            BorderStyle = BorderStyle.FixedSingle,
                            Padding = new Padding(10)
                        };

                        var titleLabel = new Label
                        {
                            Text = "Theme Settings",
                            Font = new Font(DefaultFont, FontStyle.Bold),
                            Location = new Point(10, 10),
                            AutoSize = true
                        };

                        var nameLabel = new Label
                        {
                            Text = "Theme Name:",
                            Location = new Point(10, 40),
                            AutoSize = true
                        };

                        ThemeNameBox = new TextBox
                        {
                            Location = new Point(100, 37),
                            Width = 250,
                            Height = 25
                        };

                        var versionLabel = new Label
                        {
                            Text = "Version:",
                            Location = new Point(370, 40),
                            AutoSize = true
                        };

                        Version = new NumericUpDown
                        {
                            Location = new Point(430, 37),
                            Width = 80,
                            Height = 25,
                            Minimum = 1,
                            Maximum = 100
                        };

                        panel.Controls.AddRange(new Control[]
                        {
                            titleLabel,
                            nameLabel,
                            ThemeNameBox,
                            versionLabel,
                            Version
                        });

                        return panel;
                    }
                    Panel CreateElementsSection()
                    {
                        var panel = new Panel
                        {
                            Dock = DockStyle.Fill,
                            BorderStyle = BorderStyle.FixedSingle,
                            Padding = new Padding(10)
                        };

                        var titleLabel = new Label
                        {
                            Text = "Elements",
                            Font = new Font(DefaultFont, FontStyle.Bold),
                            Location = new Point(10, 10),
                            AutoSize = true
                        };

                        ElementsPanel = new FlowLayoutPanel
                        {
                            Location = new Point(10, 40),
                            Size = new Size(panel.Width - 40, panel.Height - 60),
                            AutoScroll = true,
                            FlowDirection = FlowDirection.TopDown,
                            WrapContents = false,
                            BackColor = Color.White,
                            BorderStyle = BorderStyle.FixedSingle,
                            Padding = new Padding(5),
                            AutoSize = false
                        };

                        panel.SizeChanged += (s, e) =>
                        {
                            ElementsPanel.Size = new Size(panel.Width - 40, panel.Height - 60);

                            RefreshElementsWidth();
                        };

                        panel.Controls.Add(titleLabel);
                        panel.Controls.Add(ElementsPanel);

                        return panel;
                    }
                    Panel CreateButtonsSection()
                    {
                        var panel = new Panel
                        {
                            Dock = DockStyle.Fill,
                            Padding = new Padding(10)
                        };

                        var addButton = new Button
                        {
                            Text = "Add Element",
                            Location = new Point(10, 10),
                            Width = 120,
                            Height = 30
                        };
                        var removeButton = new Button
                        {
                            Text = "Remove Last",
                            Location = new Point(140, 10),
                            Width = 120, Height = 30
                        };
                        var clearButton = new Button
                        {
                            Text = "Clear All",
                            Location = new Point(270, 10),
                            Width = 120,
                            Height = 30
                        };

                        addButton.Click += (s, e) => AddElement();
                        removeButton.Click += (s, e) => RemoveLastElement();
                        clearButton.Click += (s, e) => ClearAllElements();

                        panel.Controls.AddRange(new Control[]
                        {
                            addButton,
                            removeButton,
                            clearButton
                        });

                        return panel;

                        void AddElement()
                        {
                            Elements.Add(new Manifest_V1.Element
                            {
                                key = ElementType.Null.ToString(),
                                @base = CreateDefaultSprite(),
                                mask = CreateDefaultSprite(),
                                overlay = CreateDefaultSprite()
                            });

                            RefreshElementsView();
                        }
                        void RemoveLastElement()
                        {
                            if (Elements.Count > 0)
                            {
                                Elements.RemoveAt(Elements.Count - 1);

                                RefreshElementsView();
                            }
                        }
                        void ClearAllElements()
                        {
                            if (MessageBox.Show("Are you sure you want to remove all elements?", "Clear All",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                Elements.Clear();

                                RefreshElementsView();
                            }
                        }
                    }
                }
            }
        }

        void CreateNewManifest()
        {
            _Manifest = UI.Manifest.CreateNew();
            FilePath = "";
            FilePathBox.Text = "";
            ThemeNameBox.Text = _Manifest.name;
            Version.Value = _Manifest.version;
            Elements = new List<Manifest_V1.Element>();

            RefreshElementsView();
        }
        void SaveJson()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                SaveJsonAs();

                return;
            }

            try
            {
                _Manifest.name = ThemeNameBox.Text;
                _Manifest.version = (int)Version.Value;
                _Manifest.elements = Elements.ToArray();

                var json = JsonConvert.SerializeObject(_Manifest, Formatting.Indented);

                File.WriteAllText(FilePath, json);
                MessageBox.Show("JSON file saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void SaveJsonAs()
        {
            var filePath = ShowSaveFileDialog("JSON files|*.json");
            if (!string.IsNullOrEmpty(filePath))
            {
                FilePath = filePath;
                FilePathBox.Text = filePath;

                SaveJson();
            }

            string ShowSaveFileDialog(string filter, string title = "Save File")
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = filter;
                    dialog.Title = title;

                    return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
                }
            }
        }
        void RefreshElementsView()
        {
            if (ElementsPanel == null)
                return;

            ElementsPanel.Controls.Clear();

            for (int i = 0; i < Elements.Count; i++)
                ElementsPanel
                    .Controls
                    .Add(CreateElementPanel(i, Elements[i]));

            Panel CreateElementPanel(int index, Manifest_V1.Element element)
            {
                var panelL = new Panel
                {
                    Width = ElementsPanel.ClientSize.Width - 10,
                    Height = 40,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5),
                    BackColor = Color.LightGray
                };

                var Llayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 1
                };
                Llayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
                Llayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                Llayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

                var expandButton = new Button
                {
                    Text = "▶",
                    Dock = DockStyle.Fill,
                    Tag = index,
                    Font = new Font("Arial", 8)
                };
                expandButton.Click += (s, e) => ToggleElementExpansion();

                var titleLabelL = new Label
                {
                    Text = $"Element {index + 1}: {element.key}",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(5, 0, 0, 0),
                    Font = new Font(DefaultFont, FontStyle.Regular)
                };

                var removeButton = new Button
                {
                    Text = "Remove",
                    Dock = DockStyle.Fill,
                    Tag = index,
                    Height = 25
                };
                removeButton.Click += (s, e) => RemoveElement();

                Llayout.Controls.Add(expandButton, 0, 0);
                Llayout.Controls.Add(titleLabelL, 1, 0);
                Llayout.Controls.Add(removeButton, 2, 0);

                panelL.Controls.Add(Llayout);

                return panelL;

                void ToggleElementExpansion()
                {
                    var elementPanel = ElementsPanel.Controls[index] as Panel;
                    if (elementPanel == null)
                        return;

                    var mainLayout = elementPanel.Controls[0] as TableLayoutPanel;
                    if (mainLayout == null)
                        return;

                    if (elementPanel.Height == 40)
                    {
                        elementPanel.Height = 350;

                        AddElementContent();

                        (mainLayout.GetControlFromPosition(0, 0) as Button).Text = "▼";
                    }
                    else
                    {
                        elementPanel.Height = 40;
                        if (mainLayout.RowCount > 1)
                        {
                            mainLayout.RowCount = 1;
                            mainLayout.RowStyles.Clear();
                            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

                            var contentControl = mainLayout.GetControlFromPosition(0, 1);
                            if (contentControl != null)
                                mainLayout.Controls.Remove(contentControl);
                        }

                        (mainLayout.GetControlFromPosition(0, 0) as Button).Text = "▶";
                    }

                    void AddElementContent()
                    {
                        mainLayout.RowCount = 2;
                        mainLayout.RowStyles.Clear();
                        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
                        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                        var contentPanel = new Panel
                        {
                            Dock = DockStyle.Fill,
                            BackColor = Color.White,
                            AutoSize = true,
                            MinimumSize = new Size(0, 400)
                        };

                        var tabControl = new TabControl
                        {
                            Dock = DockStyle.Fill,
                            Padding = new Point(10, 10)
                        };

                        var baseTab = new TabPage("Base Sprite") { Padding = new Padding(10) };
                        baseTab.Controls.Add(CreateSpritePanel(element.@base, "Base"));
                        tabControl.TabPages.Add(baseTab);

                        var maskTab = new TabPage("Mask Sprite") { Padding = new Padding(10) };
                        maskTab.Controls.Add(CreateSpritePanel(element.mask, "Mask"));
                        tabControl.TabPages.Add(maskTab);

                        var overlayTab = new TabPage("Overlay Sprite") { Padding = new Padding(10) };
                        overlayTab.Controls.Add(CreateSpritePanel(element.overlay, "Overlay"));
                        tabControl.TabPages.Add(overlayTab);

                        var generalTab = new TabPage("Element Settings") { Padding = new Padding(10) };
                        generalTab.Controls.Add(CreateGeneralPanel());
                        tabControl.TabPages.Add(generalTab);

                        contentPanel.Controls.Add(tabControl);
                        mainLayout.Controls.Add(contentPanel, 0, 1);
                        mainLayout.SetColumnSpan(contentPanel, 3);

                        Panel CreateGeneralPanel()
                        {
                            var panel = new Panel
                            {
                                Dock = DockStyle.Fill,
                                AutoSize = true
                            };

                            var layout = new TableLayoutPanel
                            {
                                Dock = DockStyle.Fill,
                                ColumnCount = 2,
                                RowCount = 3,
                                Padding = new Padding(10),
                                AutoSize = true
                            };

                            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
                            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
                            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

                            var typeLabel = new Label
                            {
                                Text = "UI Element Type:",
                                TextAlign = ContentAlignment.MiddleLeft,
                                Dock = DockStyle.Fill,
                                Font = new Font(DefaultFont, FontStyle.Bold)
                            };

                            var typeCombo = new ComboBox
                            {
                                Dock = DockStyle.Fill,
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                Height = 28
                            };

                            foreach (ElementType type in Enum.GetValues(typeof(ElementType)))
                                typeCombo.Items.Add(type);

                            try
                            {
                                typeCombo.SelectedItem = Enum.Parse(typeof(ElementType), element.key);
                            }
                            catch
                            {
                                typeCombo.SelectedIndex = 0;
                            }

                            typeCombo.SelectedIndexChanged += (s, e) =>
                            {
                                if (typeCombo.SelectedItem != null)
                                {
                                    element.key = typeCombo.SelectedItem.ToString();
                                    var titleLabel = mainLayout.GetControlFromPosition(1, 0) as Label;
                                    if (titleLabel != null)
                                        titleLabel.Text = $"Element {index + 1}: {element.key}";
                                }
                            };

                            layout.Controls.Add(typeLabel, 0, 0);
                            layout.Controls.Add(typeCombo, 1, 0);

                            panel.Controls.Add(layout);

                            return panel;
                        }
                    }
                }
                void RemoveElement()
                {
                    if (MessageBox.Show($"Are you sure you want to remove element {Elements[index].key}?",
                        "Remove Element", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Elements.RemoveAt(index);

                        RefreshElementsView();
                    }
                }
            }
        }
        void RefreshElementsWidth()
        {
            if (ElementsPanel != null)
                foreach (Control control in ElementsPanel.Controls)
                    if (control is Panel panel)
                        panel.Width = ElementsPanel.ClientSize.Width - 10;
        }

        string ShowOpenFileDialog(string filter, string title = "Open File")
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = filter;
                dialog.Title = title;

                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
            }
        }
        Panel CreateSpritePanel(Manifest_V1.Element.Sprite sprite, string spriteType)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(10),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 7; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            var fileNameLabel = new Label
            {
                Text = "File Name:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var fileNameTextBox = new TextBox
            {
                Text = sprite?.fileName ?? "",
                Dock = DockStyle.Fill,
                Height = 25
            };
            fileNameTextBox.TextChanged += (s, e) =>
            {
                if (sprite != null)
                    sprite.fileName = fileNameTextBox.Text;
            };

            var browseButton = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill,
                Height = 25
            };
            browseButton.Click += (s, e) =>
            {
                var filePath = ShowOpenFileDialog("Image files|*.png;*.jpg;*.jpeg", $"Select {spriteType} sprite");
                if (!string.IsNullOrEmpty(filePath))
                {
                    fileNameTextBox.Text = Path.GetFileName(filePath);
                }
            };

            var ppuLabel = new Label
            {
                Text = "Pixel Per Unit:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var ppuNumeric = new NumericUpDown
            {
                Value = sprite?.pixelPerUnit ?? 100,
                Minimum = 1,
                Maximum = 1000,
                Dock = DockStyle.Fill,
                Height = 25
            };
            ppuNumeric.ValueChanged += (s, e) =>
            {
                if (sprite != null)
                    sprite.pixelPerUnit = (int)ppuNumeric.Value;
            };

            var filterLabel = new Label
            {
                Text = "Filter Mode:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var filterCombo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 25
            };
            filterCombo.Items.AddRange(new object[] { "Point", "Bilinear", "Trilinear" });
            filterCombo.SelectedIndex = sprite?.filterMode ?? 1;
            filterCombo.SelectedIndexChanged += (s, e) =>
            {
                if (sprite != null)
                    sprite.filterMode = filterCombo.SelectedIndex;
            };

            var bordersButton = new Button
            {
                Text = "Edit Borders...",
                Dock = DockStyle.Fill,
                Height = 30
            };
            bordersButton.Click += (s, e) => ShowBordersDialog($"{spriteType} Sprite Borders");

            layout.Controls.Add(fileNameLabel, 0, 0);

            var fileNameLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Height = 25
            };
            fileNameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            fileNameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            fileNameLayout.Controls.Add(fileNameTextBox, 0, 0);
            fileNameLayout.Controls.Add(browseButton, 1, 0);

            layout.Controls.Add(fileNameLayout, 1, 0);
            layout.Controls.Add(ppuLabel, 0, 1);
            layout.Controls.Add(ppuNumeric, 1, 1);
            layout.Controls.Add(filterLabel, 0, 2);
            layout.Controls.Add(filterCombo, 1, 2);

            layout.Controls.Add(new Label { Text = "" }, 0, 3);
            layout.SetColumnSpan(new Label { Text = "" }, 2);

            layout.Controls.Add(bordersButton, 0, 4);
            layout.SetColumnSpan(bordersButton, 2);

            panel.Controls.Add(layout);

            return panel;

            void ShowBordersDialog(string title)
            {
                if (sprite == null)
                    return;

                var form = new Form
                {
                    Text = title,
                    Size = new Size(250, 280),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false
                };

                var groupBox = new GroupBox
                {
                    Text = "Borders Settings",
                    Location = new Point(12, 12),
                    Size = new Size(210, 180),
                    Font = new Font(DefaultFont, FontStyle.Bold)
                };

                var leftLabel = new Label { Text = "Left:", Location = new Point(20, 30), Size = new Size(60, 20) };
                var leftNumeric = new NumericUpDown
                {
                    Location = new Point(80, 27),
                    Size = new Size(80, 20),
                    Minimum = 0,
                    Maximum = 1000,
                    Value = sprite.borders.left
                };

                var rightLabel = new Label { Text = "Right:", Location = new Point(20, 60), Size = new Size(60, 20) };
                var rightNumeric = new NumericUpDown
                {
                    Location = new Point(80, 57),
                    Size = new Size(80, 20),
                    Minimum = 0,
                    Maximum = 1000,
                    Value = sprite.borders.right
                };

                var topLabel = new Label { Text = "Top:", Location = new Point(20, 90), Size = new Size(60, 20) };
                var topNumeric = new NumericUpDown
                {
                    Location = new Point(80, 87),
                    Size = new Size(80, 20),
                    Minimum = 0,
                    Maximum = 1000,
                    Value = sprite.borders.top
                };

                var bottomLabel = new Label { Text = "Bottom:", Location = new Point(20, 120), Size = new Size(60, 20) };
                var bottomNumeric = new NumericUpDown
                {
                    Location = new Point(80, 117),
                    Size = new Size(80, 20),
                    Minimum = 0,
                    Maximum = 1000,
                    Value = sprite.borders.bottom
                };

                var okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(75, 210),
                    Size = new Size(80, 30)
                };

                // Обработчики изменений
                leftNumeric.ValueChanged += (s, e) => sprite.borders.left = (int)leftNumeric.Value;
                rightNumeric.ValueChanged += (s, e) => sprite.borders.right = (int)rightNumeric.Value;
                topNumeric.ValueChanged += (s, e) => sprite.borders.top = (int)topNumeric.Value;
                bottomNumeric.ValueChanged += (s, e) => sprite.borders.bottom = (int)bottomNumeric.Value;

                // Добавляем элементы в GroupBox
                groupBox.Controls.AddRange(new Control[]
                {
                    leftLabel,
                    leftNumeric,
                    rightLabel,
                    rightNumeric,
                    topLabel,
                    topNumeric,
                    bottomLabel,
                    bottomNumeric
                });

                // Добавляем элементы на форму
                form.Controls.Add(groupBox);
                form.Controls.Add(okButton);

                form.AcceptButton = okButton;
                form.ShowDialog();
            }
        }
        Manifest_V1.Element.Sprite CreateDefaultSprite()
        {
            return new Manifest_V1.Element.Sprite
            {
                pixelPerUnit = 100,
                filterMode = 1,
                borders = new Manifest_V1.Borders()
            };
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RefreshElementsWidth();
        }
    }
}