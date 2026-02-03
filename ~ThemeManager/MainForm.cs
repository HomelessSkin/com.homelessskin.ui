using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

using Newtonsoft.Json;

using UI;

namespace ThemeManager
{
    public partial class MainForm : Form
    {
        string FilePath = "";
        Manifest_V2 _Manifest;
        List<ElementData> Elements;
        List<CustomSprite> Icons;

        TabControl MainTabControl;
        Panel FontColorPanel;

        TabPage ThemeTab;
        TabPage ElementsTab;
        TabPage IconsTab;

        TextBox FilePathBox;
        TextBox ThemeNameBox;
        TextBox FontNameBox;
        TextBox LanguageKeyBox;

        NumericUpDown Major;
        NumericUpDown Minor;
        NumericUpDown Patch;
        NumericUpDown FontColorR;
        NumericUpDown FontColorG;
        NumericUpDown FontColorB;
        NumericUpDown FontColorA;
        NumericUpDown CharacterSpacing;
        NumericUpDown WordSpacing;

        Button FontBrowseButton;
        Button FontColorPickerButton;

        FlowLayoutPanel ElementsPanel;
        FlowLayoutPanel IconsPanel;

        public MainForm()
        {
            InitializeMainForm();
            CreateNewManifest();
        }

        void InitializeMainForm()
        {
            Text = "Theme Manifest Editor";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 600);

            CreateMainLayout();
        }
        void CreateMainLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var jsonPanel = CreateJsonSection();
            MainTabControl = CreateTabControl();

            mainLayout.Controls.Add(jsonPanel, 0, 0);
            mainLayout.Controls.Add(MainTabControl, 0, 1);

            Controls.Add(mainLayout);
        }
        TabControl CreateTabControl()
        {
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(10, 10)
            };

            ThemeTab = CreateThemeTab();
            ElementsTab = CreateElementsTab();
            IconsTab = CreateIconsTab();

            tabControl.TabPages.Add(ThemeTab);
            tabControl.TabPages.Add(ElementsTab);
            tabControl.TabPages.Add(IconsTab);

            return tabControl;
        }
        TabPage CreateThemeTab()
        {
            var tab = new TabPage("Theme Settings")
            {
                Padding = new Padding(10),
                BackColor = Color.White
            };

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var contentPanel = CreateThemeContent();
            scrollPanel.Controls.Add(contentPanel);

            tab.Controls.Add(scrollPanel);
            return tab;
        }
        TabPage CreateElementsTab()
        {
            var tab = new TabPage("Elements")
            {
                Padding = new Padding(10),
                BackColor = Color.White
            };

            var elementsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            elementsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            elementsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            ElementsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
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
                Width = 120,
                Height = 30
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

            buttonsPanel.Controls.AddRange(new Control[] { addButton, removeButton, clearButton });

            elementsLayout.Controls.Add(ElementsPanel, 0, 0);
            elementsLayout.Controls.Add(buttonsPanel, 0, 1);

            tab.Controls.Add(elementsLayout);
            return tab;
        }
        TabPage CreateIconsTab()
        {
            var tab = new TabPage("Icons")
            {
                Padding = new Padding(10),
                BackColor = Color.White
            };

            var iconsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            iconsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            iconsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            IconsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            var buttonsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            var addButton = new Button
            {
                Text = "Add Icon",
                Location = new Point(10, 10),
                Width = 120,
                Height = 30
            };
            var removeButton = new Button
            {
                Text = "Remove Last",
                Location = new Point(140, 10),
                Width = 120,
                Height = 30
            };
            var clearButton = new Button
            {
                Text = "Clear All",
                Location = new Point(270, 10),
                Width = 120,
                Height = 30
            };

            addButton.Click += (s, e) => AddIcon();
            removeButton.Click += (s, e) => RemoveLastIcon();
            clearButton.Click += (s, e) => ClearAllIcons();

            buttonsPanel.Controls.AddRange(new Control[] { addButton, removeButton, clearButton });

            iconsLayout.Controls.Add(IconsPanel, 0, 0);
            iconsLayout.Controls.Add(buttonsPanel, 0, 1);

            tab.Controls.Add(iconsLayout);
            return tab;
        }
        Panel CreateThemeContent()
        {
            var panel = new Panel
            {
                AutoSize = true,
                MinimumSize = new Size(0, 600),
                Width = 800
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Width = 780
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

            var titleLabel = new Label
            {
                Text = "Theme Configuration",
                Font = new Font(DefaultFont, FontStyle.Bold),
                AutoSize = true,
                Height = 30
            };
            layout.Controls.Add(titleLabel, 0, 0);
            layout.SetColumnSpan(titleLabel, 2);

            var nameLabel = new Label
            {
                Text = "Theme Name:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            ThemeNameBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Height = 25
            };

            layout.Controls.Add(nameLabel, 0, 1);
            layout.Controls.Add(ThemeNameBox, 1, 1);

            var languageKeyLabel = new Label
            {
                Text = "Language Key:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            LanguageKeyBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Height = 25,
                Text = _Manifest?.languageKey ?? ""
            };

            layout.Controls.Add(languageKeyLabel, 0, 2);
            layout.Controls.Add(LanguageKeyBox, 1, 2);

            var versionLabel = new Label
            {
                Text = "Version:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var versionPanel = new Panel
            {
                Height = 25,
                Dock = DockStyle.Fill
            };

            Major = new NumericUpDown
            {
                Location = new Point(0, 0),
                Width = 50,
                Height = 25,
                Minimum = 0,
                Maximum = 10000
            };

            Minor = new NumericUpDown
            {
                Location = new Point(55, 0),
                Width = 50,
                Height = 25,
                Minimum = 0,
                Maximum = 99
            };

            Patch = new NumericUpDown
            {
                Location = new Point(110, 0),
                Width = 50,
                Height = 25,
                Minimum = 0,
                Maximum = 99,
            };

            versionPanel.Controls.AddRange(new Control[] { Major, Minor, Patch });

            layout.Controls.Add(versionLabel, 0, 3);
            layout.Controls.Add(versionPanel, 1, 3);

            var fontLabel = new Label
            {
                Text = "Font Asset:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var fontPanel = new Panel
            {
                Height = 25,
                Dock = DockStyle.Fill
            };

            FontNameBox = new TextBox
            {
                Location = new Point(0, 0),
                Width = 300,
                Height = 25
            };

            FontBrowseButton = new Button
            {
                Text = "Browse...",
                Location = new Point(310, 0),
                Width = 80,
                Height = 25
            };
            FontBrowseButton.Click += (s, e) =>
            {
                var filePath = ShowOpenFileDialog("Asset Files|*.asset|Manifest Files|*.manifest", "Select Font Asset");
                if (!string.IsNullOrEmpty(filePath))
                {
                    FontNameBox.Text = Path.GetFileName(filePath);
                }
            };

            fontPanel.Controls.Add(FontNameBox);
            fontPanel.Controls.Add(FontBrowseButton);

            layout.Controls.Add(fontLabel, 0, 4);
            layout.Controls.Add(fontPanel, 1, 4);

            var fontColorLabel = new Label
            {
                Text = "Font Color:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            var fontColorPanel = new Panel
            {
                Height = 70,
                Dock = DockStyle.Fill
            };

            FontColorPanel = new Panel
            {
                Location = new Point(0, 5),
                Size = new Size(40, 40),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            FontColorR = new NumericUpDown
            {
                Location = new Point(50, 5),
                Width = 50,
                Height = 25,
                Minimum = 0,
                Maximum = 255,
                Value = 255
            };

            var fontColorRLabel = new Label
            {
                Text = "R:",
                Location = new Point(50, 30),
                AutoSize = true
            };

            FontColorG = new NumericUpDown
            {
                Location = new Point(110, 5),
                Width = 50,
                Height = 25,
                Minimum = 0,
                Maximum = 255,
                Value = 255
            };

            var fontColorGLabel = new Label
            {
                Text = "G:",
                Location = new Point(110, 30),
                AutoSize = true
            };

            FontColorB = new NumericUpDown
            {
                Location = new Point(170, 5),
                Width = 50,
                Height = 25,
                Minimum = 0,
                Maximum = 255,
                Value = 255
            };

            var fontColorBLabel = new Label
            {
                Text = "B:",
                Location = new Point(170, 30),
                AutoSize = true
            };

            FontColorA = new NumericUpDown
            {
                Location = new Point(230, 5),
                Width = 50,
                Height = 25,
                Minimum = 0,
                Maximum = 255,
                Value = 255
            };

            var fontColorALabel = new Label
            {
                Text = "A:",
                Location = new Point(230, 30),
                AutoSize = true
            };

            FontColorPickerButton = new Button
            {
                Text = "Pick Color...",
                Location = new Point(290, 5),
                Width = 80,
                Height = 25
            };
            FontColorPickerButton.Click += (s, e) => ShowColorPickerDialog();

            fontColorPanel.Controls.AddRange(new Control[]
            {
                FontColorPanel,
                FontColorR,
                fontColorRLabel,
                FontColorG,
                fontColorGLabel,
                FontColorB,
                fontColorBLabel,
                FontColorA,
                fontColorALabel,
                FontColorPickerButton
            });

            layout.Controls.Add(fontColorLabel, 0, 5);
            layout.Controls.Add(fontColorPanel, 1, 5);

            var characterSpacingLabel = new Label
            {
                Text = "Character Spacing:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            CharacterSpacing = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Height = 25,
                Minimum = -50,
                Maximum = 50,
                Value = 0
            };

            layout.Controls.Add(characterSpacingLabel, 0, 6);
            layout.Controls.Add(CharacterSpacing, 1, 6);

            var wordSpacingLabel = new Label
            {
                Text = "Word Spacing:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            WordSpacing = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Height = 25,
                Minimum = -50,
                Maximum = 50,
                Value = 0
            };

            layout.Controls.Add(wordSpacingLabel, 0, 7);
            layout.Controls.Add(WordSpacing, 1, 7);

            FontColorR.ValueChanged += (s, e) => UpdateFontColor();
            FontColorG.ValueChanged += (s, e) => UpdateFontColor();
            FontColorB.ValueChanged += (s, e) => UpdateFontColor();
            FontColorA.ValueChanged += (s, e) => UpdateFontColor();
            CharacterSpacing.ValueChanged += (s, e) => UpdateFontSpacing();
            WordSpacing.ValueChanged += (s, e) => UpdateFontSpacing();

            panel.Controls.Add(layout);
            return panel;

            void ShowColorPickerDialog()
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.Color = FontColorPanel.BackColor;
                    colorDialog.FullOpen = true;

                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        FontColorR.Value = colorDialog.Color.R;
                        FontColorG.Value = colorDialog.Color.G;
                        FontColorB.Value = colorDialog.Color.B;
                        FontColorA.Value = colorDialog.Color.A;

                        UpdateFontColor();
                    }
                }
            }
        }
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
        }
        void CreateNewManifest()
        {
            _Manifest = Manifest.CreateNew();
            FilePath = "";
            FilePathBox.Text = "";
            ThemeNameBox.Text = _Manifest.name;
            LanguageKeyBox.Text = _Manifest.languageKey ?? "default";
            Major.Value = _Manifest.v.major;
            Minor.Value = _Manifest.v.minor;
            Patch.Value = _Manifest.v.patch;
            Elements = new List<ElementData>();
            Icons = new List<CustomSprite>();

            FontNameBox.Text = _Manifest.font?.assetName ?? "";

            FontColorR.Value = 255;
            FontColorG.Value = 255;
            FontColorB.Value = 255;
            FontColorA.Value = 255;
            CharacterSpacing.Value = 0;
            WordSpacing.Value = 0;

            UpdateFontColorPreview();

            RefreshElementsView();
            RefreshIconsView();
        }
        void LoadJsonFromFile()
        {
            var filePath = ShowOpenFileDialog("JSON files|*.json");
            if (!string.IsNullOrEmpty(filePath))
            {
                FilePath = filePath;
                FilePathBox.Text = filePath;

                if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
                    return;

                try
                {
                    var json = File.ReadAllText(FilePath);
                    _Manifest = Manifest.Cast(json);
                    Elements = new List<ElementData>(_Manifest.elements ?? new ElementData[0]);
                    Icons = new List<CustomSprite>(_Manifest.icons ?? new CustomSprite[0]);

                    foreach (var element in Elements)
                    {
                        if (element.overlay == null)
                            element.overlay = CreateDefaultSprite();
                        else if (element.overlay.borders == null)
                            element.overlay.borders = new Borders();

                        if (element.mask == null)
                            element.mask = CreateDefaultSprite();

                        if (element.@base.borders == null)
                            element.@base.borders = new Borders();

                        if (element.text == null)
                        {
                            element.text = new TextData
                            {
                                fontSize = 32,
                                xOffset = 0,
                                yOffset = 0
                            };
                        }

                        if (element.selectable == null)
                        {
                            element.selectable = new SelectableData
                            {
                                transition = 0,
                                normalColor = new Vector4Data(255f, 255f, 255f, 255f),
                                highlightedColor = new Vector4Data(255f, 255f, 255f, 255f),
                                pressedColor = new Vector4Data(255f, 255f, 255f, 255f),
                                selectedColor = new Vector4Data(255f, 255f, 255f, 255f),
                                disabledColor = new Vector4Data(255f, 255f, 255f, 255f)
                            };
                        }
                    }

                    foreach (var icon in Icons)
                    {
                        if (icon.borders == null)
                            icon.borders = new Borders();
                    }

                    ThemeNameBox.Text = _Manifest.name;
                    LanguageKeyBox.Text = _Manifest.languageKey ?? "";
                    Major.Value = _Manifest.v.major;
                    Minor.Value = _Manifest.v.minor;
                    Patch.Value = _Manifest.v.patch;
                    FontNameBox.Text = _Manifest.font?.assetName ?? "";

                    if (_Manifest.font != null)
                    {
                        var color = _Manifest.font.color;
                        var cSpacing = _Manifest.font.characterSpacing;
                        var wSpacing = _Manifest.font.wordSpacing;

                        FontColorR.Value = (decimal)(color.X);
                        FontColorG.Value = (decimal)(color.Y);
                        FontColorB.Value = (decimal)(color.Z);
                        FontColorA.Value = (decimal)(color.W);

                        CharacterSpacing.Value = cSpacing;
                        WordSpacing.Value = wSpacing;

                        UpdateFontColorPreview();
                    }
                    else
                    {
                        FontColorR.Value = 255;
                        FontColorG.Value = 255;
                        FontColorB.Value = 255;
                        FontColorA.Value = 255;
                        CharacterSpacing.Value = 0;
                        WordSpacing.Value = 0;

                        UpdateFontColorPreview();
                    }

                    RefreshElementsView();
                    RefreshIconsView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
                _Manifest.languageKey = LanguageKeyBox.Text;
                _Manifest.v = new VersionData((int)Major.Value, (int)Minor.Value, (int)Patch.Value);
                _Manifest.elements = Elements.ToArray();
                _Manifest.icons = Icons.ToArray();

                if (_Manifest.font == null)
                    _Manifest.font = new FontData();

                _Manifest.font.assetName = FontNameBox.Text;

                _Manifest.font.color = new Vector4Data(
                    (float)FontColorR.Value,
                    (float)FontColorG.Value,
                    (float)FontColorB.Value,
                    (float)FontColorA.Value
                );

                _Manifest.font.characterSpacing = (int)CharacterSpacing.Value;
                _Manifest.font.wordSpacing = (int)WordSpacing.Value;

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
        }
        void UpdateFontSpacing()
        {
            if (_Manifest != null && _Manifest.font != null)
            {
                _Manifest.font.characterSpacing = (int)CharacterSpacing.Value;
                _Manifest.font.wordSpacing = (int)WordSpacing.Value;
            }
        }
        void AddElement()
        {
            Elements.Add(new ElementData
            {
                key = ElementType.Null.ToString(),
                @base = CreateDefaultSprite(),
                mask = CreateDefaultSprite(),
                overlay = CreateDefaultSprite(),
                text = new TextData
                {
                    fontSize = 32,
                    xOffset = 0,
                    yOffset = 0
                },
                selectable = new SelectableData
                {
                    transition = 0,
                    normalColor = new Vector4Data(255f, 255f, 255f, 255f),
                    highlightedColor = new Vector4Data(255f, 255f, 255f, 255f),
                    pressedColor = new Vector4Data(255f, 255f, 255f, 255f),
                    selectedColor = new Vector4Data(255f, 255f, 255f, 255f),
                    disabledColor = new Vector4Data(255f, 255f, 255f, 255f)
                }
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
            if (MessageBox.Show("Are you sure you want to remove all elements?", "Clear All Elements",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Elements.Clear();
                RefreshElementsView();
            }
        }
        void AddIcon()
        {
            Icons.Add(CreateDefaultSprite());
            RefreshIconsView();
        }
        void RemoveLastIcon()
        {
            if (Icons.Count > 0)
            {
                Icons.RemoveAt(Icons.Count - 1);
                RefreshIconsView();
            }
        }
        void ClearAllIcons()
        {
            if (MessageBox.Show("Are you sure you want to remove all icons?", "Clear All Icons",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Icons.Clear();
                RefreshIconsView();
            }
        }
        void RefreshElementsView()
        {
            if (ElementsPanel == null)
                return;

            ElementsPanel.Controls.Clear();

            for (int i = 0; i < Elements.Count; i++)
                ElementsPanel.Controls.Add(CreateElementPanel(i, Elements[i]));

            RefreshElementsWidth();
        }
        void RefreshIconsView()
        {
            if (IconsPanel == null)
                return;

            IconsPanel.Controls.Clear();

            for (int i = 0; i < Icons.Count; i++)
                IconsPanel.Controls.Add(CreateIconPanel(i, Icons[i]));

            RefreshIconsWidth();
        }
        Panel CreateElementPanel(int index, ElementData element)
        {
            var panel = new Panel
            {
                Width = ElementsPanel.ClientSize.Width - 10,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                BackColor = Color.LightGray
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            var expandButton = new Button
            {
                Text = "▶",
                Dock = DockStyle.Fill,
                Tag = index,
                Font = new Font("Arial", 8)
            };
            expandButton.Click += (s, e) => ToggleElementExpansion();

            var titleLabel = new Label
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

            layout.Controls.Add(expandButton, 0, 0);
            layout.Controls.Add(titleLabel, 1, 0);
            layout.Controls.Add(removeButton, 2, 0);

            panel.Controls.Add(layout);

            return panel;

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
                    elementPanel.Height = 500;
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
                        MinimumSize = new Size(0, 500)
                    };

                    var tabControl = new TabControl
                    {
                        Dock = DockStyle.Fill,
                        Padding = new Point(10, 10)
                    };

                    var baseTab = new TabPage("Base Sprite") { Padding = new Padding(10) };
                    baseTab.Controls.Add(CreateSpritePanel(element.@base, "Base", element.mask));
                    tabControl.TabPages.Add(baseTab);

                    var overlayTab = new TabPage("Overlay Sprite") { Padding = new Padding(10) };
                    overlayTab.Controls.Add(CreateSpritePanel(element.overlay, "Overlay"));
                    tabControl.TabPages.Add(overlayTab);

                    var textTab = new TabPage("Text Settings") { Padding = new Padding(10) };
                    textTab.Controls.Add(CreateTextPanel(element.text));
                    tabControl.TabPages.Add(textTab);

                    var selectableTab = new TabPage("Selectable Transitions") { Padding = new Padding(10) };
                    selectableTab.Controls.Add(CreateSelectablePanel(element.selectable));
                    tabControl.TabPages.Add(selectableTab);

                    var generalTab = new TabPage("Element Settings") { Padding = new Padding(10) };
                    generalTab.Controls.Add(CreateGeneralPanel(element, index));
                    tabControl.TabPages.Add(generalTab);

                    contentPanel.Controls.Add(tabControl);
                    mainLayout.Controls.Add(contentPanel, 0, 1);
                    mainLayout.SetColumnSpan(contentPanel, 3);
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
        Panel CreateIconPanel(int index, CustomSprite icon)
        {
            var panel = new Panel
            {
                Width = IconsPanel.ClientSize.Width - 10,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                BackColor = Color.LightSteelBlue
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            var expandButton = new Button
            {
                Text = "▶",
                Dock = DockStyle.Fill,
                Tag = index,
                Font = new Font("Arial", 8)
            };
            expandButton.Click += (s, e) => ToggleIconExpansion();

            var titleLabel = new Label
            {
                Text = $"Icon {index + 1}: {icon.fileName}",
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
            removeButton.Click += (s, e) => RemoveIcon();

            layout.Controls.Add(expandButton, 0, 0);
            layout.Controls.Add(titleLabel, 1, 0);
            layout.Controls.Add(removeButton, 2, 0);

            panel.Controls.Add(layout);

            return panel;

            void ToggleIconExpansion()
            {
                var iconPanel = IconsPanel.Controls[index] as Panel;
                if (iconPanel == null)
                    return;

                var mainLayout = iconPanel.Controls[0] as TableLayoutPanel;
                if (mainLayout == null)
                    return;

                if (iconPanel.Height == 40)
                {
                    iconPanel.Height = 300;
                    AddIconContent();
                    (mainLayout.GetControlFromPosition(0, 0) as Button).Text = "▼";
                }
                else
                {
                    iconPanel.Height = 40;
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

                void AddIconContent()
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
                        MinimumSize = new Size(0, 300)
                    };

                    var spritePanel = CreateSpritePanel(icon, "Icon");
                    spritePanel.Dock = DockStyle.Fill;
                    contentPanel.Controls.Add(spritePanel);

                    mainLayout.Controls.Add(contentPanel, 0, 1);
                    mainLayout.SetColumnSpan(contentPanel, 3);
                }
            }

            void RemoveIcon()
            {
                if (MessageBox.Show($"Are you sure you want to remove icon {index + 1}?",
                    "Remove Icon", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Icons.RemoveAt(index);
                    RefreshIconsView();
                }
            }
        }
        Panel CreateGeneralPanel(ElementData element, int index)
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
                    RefreshElementsView();
                }
            };

            layout.Controls.Add(typeLabel, 0, 0);
            layout.Controls.Add(typeCombo, 1, 0);

            panel.Controls.Add(layout);

            return panel;
        }
        Panel CreateTextPanel(TextData textData)
        {
            if (textData == null)
            {
                textData = new TextData
                {
                    fontSize = 14,
                    xOffset = 0,
                    yOffset = 0
                };
            }

            var panel = new Panel
            {
                Dock = DockStyle.Fill
            };

            var fontSizeLabel = new Label
            {
                Text = "Font Size:",
                Location = new Point(20, 20),
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var fontSizeNumeric = new NumericUpDown
            {
                Value = textData.fontSize,
                Minimum = 1,
                Maximum = 100,
                Location = new Point(150, 17),
                Size = new Size(80, 20)
            };
            fontSizeNumeric.ValueChanged += (s, e) =>
            {
                textData.fontSize = (int)fontSizeNumeric.Value;
            };

            var xOffsetLabel = new Label
            {
                Text = "X Offset:",
                Location = new Point(20, 60),
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var xOffsetNumeric = new NumericUpDown
            {
                Value = textData.xOffset,
                Minimum = -1000,
                Maximum = 1000,
                Location = new Point(150, 57),
                Size = new Size(80, 20)
            };
            xOffsetNumeric.ValueChanged += (s, e) =>
            {
                textData.xOffset = (int)xOffsetNumeric.Value;
            };

            var yOffsetLabel = new Label
            {
                Text = "Y Offset:",
                Location = new Point(20, 100),
                Size = new Size(120, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var yOffsetNumeric = new NumericUpDown
            {
                Value = textData.yOffset,
                Minimum = -1000,
                Maximum = 1000,
                Location = new Point(150, 97),
                Size = new Size(80, 20)
            };
            yOffsetNumeric.ValueChanged += (s, e) =>
            {
                textData.yOffset = (int)yOffsetNumeric.Value;
            };

            panel.Controls.AddRange(new Control[]
            {
                fontSizeLabel, fontSizeNumeric,
                xOffsetLabel, xOffsetNumeric,
                yOffsetLabel, yOffsetNumeric,
            });

            return panel;
        }
        Panel CreateSelectablePanel(SelectableData selectable)
        {
            if (selectable == null)
                selectable = new SelectableData
                {
                    transition = 0,
                    normalColor = new Vector4Data(255f, 255f, 255f, 255f),
                    highlightedColor = new Vector4Data(255f, 255f, 255f, 255f),
                    pressedColor = new Vector4Data(255f, 255f, 255f, 255f),
                    selectedColor = new Vector4Data(255f, 255f, 255f, 255f),
                    disabledColor = new Vector4Data(255f, 255f, 255f, 255f)
                };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(10),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var transitionLabel = new Label
            {
                Text = "Transition Type:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font(DefaultFont, FontStyle.Bold)
            };

            var transitionCombo = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 25
            };
            transitionCombo.Items.AddRange(new object[] { "Color Tint", "Sprite Swap" });
            transitionCombo.SelectedIndex = selectable.transition;

            layout.Controls.Add(transitionLabel, 0, 0);
            layout.Controls.Add(transitionCombo, 1, 0);

            var normalLabel = new Label
            {
                Text = "Normal State:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font(DefaultFont, FontStyle.Bold)
            };
            layout.Controls.Add(normalLabel, 0, 1);
            layout.SetColumnSpan(normalLabel, 2);

            var normalColorLabel = new Label { Text = "Normal Color:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var normalColorPanel = CreateColorControlWithPicker(selectable.normalColor, color => selectable.normalColor = new Vector4Data(color));
            layout.Controls.Add(normalColorLabel, 0, 2);
            layout.Controls.Add(normalColorPanel, 1, 2);

            var highlightedLabel = new Label
            {
                Text = "Highlighted State:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font(DefaultFont, FontStyle.Bold)
            };
            layout.Controls.Add(highlightedLabel, 0, 3);
            layout.SetColumnSpan(highlightedLabel, 2);

            var highlightedColorLabel = new Label { Text = "Highlighted Color:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var highlightedColorPanel = CreateColorControlWithPicker(selectable.highlightedColor, color => selectable.highlightedColor = new Vector4Data(color));
            layout.Controls.Add(highlightedColorLabel, 0, 4);
            layout.Controls.Add(highlightedColorPanel, 1, 4);

            var highlightedSpriteLabel = new Label { Text = "Highlighted Sprite:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var highlightedSpritePanel = CreateSpriteControl(selectable.highlightedSprite, sprite => selectable.highlightedSprite = sprite);
            layout.Controls.Add(highlightedSpriteLabel, 0, 4);
            layout.Controls.Add(highlightedSpritePanel, 1, 4);

            var pressedLabel = new Label
            {
                Text = "Pressed State:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font(DefaultFont, FontStyle.Bold)
            };
            layout.Controls.Add(pressedLabel, 0, 5);
            layout.SetColumnSpan(pressedLabel, 2);

            var pressedColorLabel = new Label { Text = "Pressed Color:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var pressedColorPanel = CreateColorControlWithPicker(selectable.pressedColor, color => selectable.pressedColor = new Vector4Data(color));
            layout.Controls.Add(pressedColorLabel, 0, 6);
            layout.Controls.Add(pressedColorPanel, 1, 6);

            var pressedSpriteLabel = new Label { Text = "Pressed Sprite:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var pressedSpritePanel = CreateSpriteControl(selectable.pressedSprite, sprite => selectable.pressedSprite = sprite);
            layout.Controls.Add(pressedSpriteLabel, 0, 6);
            layout.Controls.Add(pressedSpritePanel, 1, 6);

            var selectedLabel = new Label
            {
                Text = "Selected State:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font(DefaultFont, FontStyle.Bold)
            };
            layout.Controls.Add(selectedLabel, 0, 7);
            layout.SetColumnSpan(selectedLabel, 2);

            var selectedColorLabel = new Label { Text = "Selected Color:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var selectedColorPanel = CreateColorControlWithPicker(selectable.selectedColor, color => selectable.selectedColor = new Vector4Data(color));
            layout.Controls.Add(selectedColorLabel, 0, 8);
            layout.Controls.Add(selectedColorPanel, 1, 8);

            var selectedSpriteLabel = new Label { Text = "Selected Sprite:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var selectedSpritePanel = CreateSpriteControl(selectable.selectedSprite, sprite => selectable.selectedSprite = sprite);
            layout.Controls.Add(selectedSpriteLabel, 0, 8);
            layout.Controls.Add(selectedSpritePanel, 1, 8);

            var disabledLabel = new Label
            {
                Text = "Disabled State:",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Font = new Font(DefaultFont, FontStyle.Bold)
            };
            layout.Controls.Add(disabledLabel, 0, 9);
            layout.SetColumnSpan(disabledLabel, 2);

            var disabledColorLabel = new Label { Text = "Disabled Color:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var disabledColorPanel = CreateColorControlWithPicker(selectable.disabledColor, color => selectable.disabledColor = new Vector4Data(color));
            layout.Controls.Add(disabledColorLabel, 0, 10);
            layout.Controls.Add(disabledColorPanel, 1, 10);

            var disabledSpriteLabel = new Label { Text = "Disabled Sprite:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            var disabledSpritePanel = CreateSpriteControl(selectable.disabledSprite, sprite => selectable.disabledSprite = sprite);
            layout.Controls.Add(disabledSpriteLabel, 0, 10);
            layout.Controls.Add(disabledSpritePanel, 1, 10);

            panel.Controls.Add(layout);

            void UpdateTransitionVisibility()
            {
                bool isColorTint = transitionCombo.SelectedIndex == 0;

                normalColorLabel.Visible = isColorTint;
                normalColorPanel.Visible = isColorTint;
                highlightedColorLabel.Visible = isColorTint;
                highlightedColorPanel.Visible = isColorTint;
                pressedColorLabel.Visible = isColorTint;
                pressedColorPanel.Visible = isColorTint;
                selectedColorLabel.Visible = isColorTint;
                selectedColorPanel.Visible = isColorTint;
                disabledColorLabel.Visible = isColorTint;
                disabledColorPanel.Visible = isColorTint;

                highlightedSpriteLabel.Visible = !isColorTint;
                highlightedSpritePanel.Visible = !isColorTint;
                pressedSpriteLabel.Visible = !isColorTint;
                pressedSpritePanel.Visible = !isColorTint;
                selectedSpriteLabel.Visible = !isColorTint;
                selectedSpritePanel.Visible = !isColorTint;
                disabledSpriteLabel.Visible = !isColorTint;
                disabledSpritePanel.Visible = !isColorTint;
            }

            transitionCombo.SelectedIndexChanged += (s, e) =>
            {
                selectable.transition = (byte)transitionCombo.SelectedIndex;
                UpdateTransitionVisibility();
            };

            UpdateTransitionVisibility();

            return panel;
        }
        Panel CreateColorControlWithPicker(Vector4Data initialColor, Action<Vector4> onColorChanged)
        {
            var colorPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                MinimumSize = new Size(0, 40)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Height = 40,
                MinimumSize = new Size(0, 40)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

            var previewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(
                    (int)(initialColor.W),
                    (int)(initialColor.X),
                    (int)(initialColor.Y),
                    (int)(initialColor.Z)
                ),
                Margin = new Padding(2),
                Size = new Size(36, 36)
            };

            var colorInfoLabel = new Label
            {
                Text = $"R:{(int)(initialColor.X)} G:{(int)(initialColor.Y)} B:{(int)(initialColor.Z)} A:{(int)(initialColor.W)}",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(10, 0, 0, 0),
                AutoSize = false,
                Height = 40
            };

            var pickerButton = new Button
            {
                Text = "Pick",
                Dock = DockStyle.Fill,
                Height = 30,
                Margin = new Padding(2),
                Size = new Size(56, 30)
            };

            var clearButton = new Button
            {
                Text = "Clear",
                Dock = DockStyle.Fill,
                Height = 30,
                Margin = new Padding(2),
                Size = new Size(56, 30)
            };

            pickerButton.Click += (s, e) =>
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.Color = previewPanel.BackColor;
                    colorDialog.FullOpen = true;

                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        var color = colorDialog.Color;
                        var newColor = new Vector4(
                            color.R,
                            color.G,
                            color.B,
                            color.A
                        );

                        previewPanel.BackColor = color;
                        colorInfoLabel.Text = $"R:{color.R} G:{color.G} B:{color.B} A:{color.A}";
                        onColorChanged?.Invoke(newColor);
                    }
                }
            };

            clearButton.Click += (s, e) =>
            {
                var defaultColor = new Vector4(255f, 255f, 255f, 255f);
                previewPanel.BackColor = Color.White;
                colorInfoLabel.Text = "R:255 G:255 B:255 A:255";
                onColorChanged?.Invoke(defaultColor);
            };

            layout.Controls.Add(previewPanel, 0, 0);
            layout.Controls.Add(colorInfoLabel, 1, 0);
            layout.Controls.Add(pickerButton, 2, 0);
            layout.Controls.Add(clearButton, 3, 0);

            colorPanel.Controls.Add(layout);
            return colorPanel;
        }
        Panel CreateSpriteControl(SpriteData sprite, Action<SpriteData> onSpriteChanged)
        {
            var spritePanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                MinimumSize = new Size(0, 40)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Height = 40,
                MinimumSize = new Size(0, 40)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            var fileNameTextBox = new TextBox
            {
                Text = sprite?.fileName ?? "",
                Dock = DockStyle.Fill,
                Height = 30,
                Margin = new Padding(2),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            var browseButton = new Button
            {
                Text = "Browse...",
                Dock = DockStyle.Fill,
                Height = 30,
                Margin = new Padding(2),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            fileNameTextBox.TextChanged += (s, e) =>
            {
                if (sprite != null)
                    sprite.fileName = fileNameTextBox.Text;
                else
                {
                    sprite = CreateDefaultSprite();
                    sprite.fileName = fileNameTextBox.Text;
                    onSpriteChanged?.Invoke(sprite);
                }
            };

            browseButton.Click += (s, e) =>
            {
                var filePath = ShowOpenFileDialog("Image files|*.png;*.jpg;*.jpeg", "Select Sprite");
                if (!string.IsNullOrEmpty(filePath))
                {
                    fileNameTextBox.Text = Path.GetFileName(filePath);
                }
            };

            layout.Controls.Add(fileNameTextBox, 0, 0);
            layout.Controls.Add(browseButton, 1, 0);

            spritePanel.Controls.Add(layout);
            return spritePanel;
        }
        Panel CreateSpritePanel(CustomSprite sprite, string spriteType, SpriteData maskSprite = null)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            int rowCount = 7;
            if (spriteType == "Base")
                rowCount += 2;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = rowCount,
                Padding = new Padding(10),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < rowCount; i++)
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
                Height = 30,
                Tag = sprite
            };
            bordersButton.Click += (s, e) =>
            {
                var btn = s as Button;
                var targetSprite = btn?.Tag as CustomSprite;
                if (targetSprite != null)
                {
                    ShowBordersDialog($"{spriteType} Sprite Borders", targetSprite);
                }
            };

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

            if (spriteType == "Base" && maskSprite != null)
            {
                var maskSeparator = new Label
                {
                    Text = "Mask Settings:",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill,
                    Font = new Font(DefaultFont, FontStyle.Bold)
                };
                layout.Controls.Add(maskSeparator, 0, 3);
                layout.SetColumnSpan(maskSeparator, 2);

                var maskFileNameLabel = new Label
                {
                    Text = "Mask File:",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };

                var maskFileNameTextBox = new TextBox
                {
                    Text = maskSprite?.fileName ?? "",
                    Dock = DockStyle.Fill,
                    Height = 25
                };
                maskFileNameTextBox.TextChanged += (s, e) =>
                {
                    if (maskSprite != null)
                        maskSprite.fileName = maskFileNameTextBox.Text;
                };

                var maskBrowseButton = new Button
                {
                    Text = "Browse...",
                    Dock = DockStyle.Fill,
                    Height = 25
                };
                maskBrowseButton.Click += (s, e) =>
                {
                    var filePath = ShowOpenFileDialog("Image files|*.png;*.jpg;*.jpeg", "Select Mask sprite");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        maskFileNameTextBox.Text = Path.GetFileName(filePath);
                    }
                };

                var maskFileNameLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Height = 25
                };
                maskFileNameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
                maskFileNameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
                maskFileNameLayout.Controls.Add(maskFileNameTextBox, 0, 0);
                maskFileNameLayout.Controls.Add(maskBrowseButton, 1, 0);

                layout.Controls.Add(maskFileNameLabel, 0, 4);
                layout.Controls.Add(maskFileNameLayout, 1, 4);

                layout.Controls.Add(new Label { Text = "" }, 0, 5);
                layout.SetColumnSpan(new Label { Text = "" }, 2);

                layout.Controls.Add(bordersButton, 0, 6);
                layout.SetColumnSpan(bordersButton, 2);
            }
            else
            {
                layout.Controls.Add(new Label { Text = "" }, 0, 3);
                layout.SetColumnSpan(new Label { Text = "" }, 2);

                layout.Controls.Add(bordersButton, 0, 4);
                layout.SetColumnSpan(bordersButton, 2);
            }

            panel.Controls.Add(layout);

            return panel;
        }
        void ShowBordersDialog(string title, CustomSprite sprite)
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

            leftNumeric.ValueChanged += (s, e) => sprite.borders.left = (int)leftNumeric.Value;
            rightNumeric.ValueChanged += (s, e) => sprite.borders.right = (int)rightNumeric.Value;
            topNumeric.ValueChanged += (s, e) => sprite.borders.top = (int)topNumeric.Value;
            bottomNumeric.ValueChanged += (s, e) => sprite.borders.bottom = (int)bottomNumeric.Value;

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

            form.Controls.Add(groupBox);
            form.Controls.Add(okButton);

            form.AcceptButton = okButton;
            form.ShowDialog();
        }
        CustomSprite CreateDefaultSprite() => new CustomSprite
        {
            pixelPerUnit = 100,
            filterMode = 1,
            borders = new Borders()
        };
        void UpdateFontColor()
        {
            UpdateFontColorPreview();

            if (_Manifest != null && _Manifest.font != null)
            {
                _Manifest.font.color = new Vector4Data(
                    (float)FontColorR.Value,
                    (float)FontColorG.Value,
                    (float)FontColorB.Value,
                    (float)FontColorA.Value
                );
            }
        }
        void UpdateFontColorPreview()
        {
            FontColorPanel.BackColor = Color.FromArgb((int)FontColorA.Value,
                (int)FontColorR.Value,
                (int)FontColorG.Value,
                (int)FontColorB.Value);
        }
        void RefreshElementsWidth()
        {
            if (ElementsPanel != null)
                foreach (Control control in ElementsPanel.Controls)
                    if (control is Panel panel)
                        panel.Width = ElementsPanel.ClientSize.Width - 10;
        }
        void RefreshIconsWidth()
        {
            if (IconsPanel != null)
                foreach (Control control in IconsPanel.Controls)
                    if (control is Panel panel)
                        panel.Width = IconsPanel.ClientSize.Width - 10;
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
        string ShowSaveFileDialog(string filter, string title = "Save File")
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = filter;
                dialog.Title = title;
                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RefreshElementsWidth();
            RefreshIconsWidth();
        }
    }
}