using System.Text.Json;

namespace L_systems
{
    public partial class Form1 : Form
    {
        private Bitmap canvas;
        private Graphics g;
        private Turtle turtle;
        private string program = string.Empty;
        private int currentStep = 0;

        // UI Controls
        private PictureBox pictureBox;
        private TextBox txtAxiom, txtRules, txtVariables, txtConstants;
        private Button btnStart, btnClear, btnStepForward, btnStepBackward, btnGenerate, btnRunAll;
        private CheckBox chkPenDown;
        private NumericUpDown numStep, numAngle, numPenWidth, numIterations;
        private Button btnCanvasColor, btnPenColor;
        private Label lblCurrentStep, lblGenerated;
        private ComboBox cmbExamples;
        private Button btnSaveSettings, btnLoadSettings, btnSaveImage;

        // Settings
        private Color canvasColor = Color.DarkGreen;
        private Color penColor = Color.Yellow;
        private float stepSize = 10f;
        private float angleSize = 90f;
        private float penWidth = 1f;
        private bool penDown = true;

        // L-system
        private Dictionary<char, string> rules = new Dictionary<char, string>();
        private HashSet<char> variables = new HashSet<char>();
        private HashSet<char> constants = new HashSet<char>();
        private string axiom = string.Empty;

        // Panning
        private Point panOffset = new Point(0, 0);
        private Point lastMousePos;
        private bool isPanning = false;

        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 800);
            SetupUI();
        }

        private void SetupUI()
        {
            Text = "L-systems Turtle";

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray,
                SizeMode = PictureBoxSizeMode.Normal,
                Cursor = Cursors.Hand
            };
            pictureBox.SizeChanged += (s, e) => InitCanvas();
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;
            pictureBox.Paint += PictureBox_Paint;

            // L-system definition panel (right side)
            var lSystemPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                Padding = new Padding(10),
                AutoScroll = true,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            int yPos = 5;
            int controlWidth = 255;

            // Examples dropdown
            var lblExamples = new Label { Text = "Load Example:", Location = new Point(5, yPos), Width = controlWidth, AutoSize = false };
            lSystemPanel.Controls.Add(lblExamples);
            yPos += 20;

            cmbExamples = new ComboBox { Location = new Point(5, yPos), Width = controlWidth, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbExamples.Items.AddRange(new object[] {
                "Koch Curve",
                "Sierpiński Triangle",
                "Dragon Curve",
                "Plant",
                "Tree",
                "Hilbert Curve",
                "Fractal Plant 2"
            });
            cmbExamples.SelectedIndexChanged += (s, e) => LoadExample(cmbExamples.SelectedIndex);
            lSystemPanel.Controls.Add(cmbExamples);
            yPos += 35;

            var lblAxiom = new Label { Text = "Axiom:", Location = new Point(5, yPos), Width = controlWidth, AutoSize = false };
            txtAxiom = new TextBox { Location = new Point(5, yPos + 20), Width = controlWidth, PlaceholderText = "e.g., F" };
            lSystemPanel.Controls.Add(lblAxiom);
            lSystemPanel.Controls.Add(txtAxiom);
            yPos += 50;

            var lblRules = new Label { Text = "Rules:", Location = new Point(5, yPos), Width = controlWidth, AutoSize = false };
            txtRules = new TextBox { Location = new Point(5, yPos + 20), Width = controlWidth, Height = 60, Multiline = true, ScrollBars = ScrollBars.Vertical, PlaceholderText = "e.g., F=F+F-F-F+F" };
            lSystemPanel.Controls.Add(lblRules);
            lSystemPanel.Controls.Add(txtRules);
            yPos += 90;

            var lblVariables = new Label { Text = "Variables:", Location = new Point(5, yPos), Width = controlWidth, AutoSize = false };
            txtVariables = new TextBox { Location = new Point(5, yPos + 20), Width = controlWidth, PlaceholderText = "e.g., F,G,X,Y" };
            lSystemPanel.Controls.Add(lblVariables);
            lSystemPanel.Controls.Add(txtVariables);
            yPos += 50;

            var lblConstants = new Label { Text = "Constants:", Location = new Point(5, yPos), Width = controlWidth, AutoSize = false };
            txtConstants = new TextBox { Location = new Point(5, yPos + 20), Width = controlWidth, PlaceholderText = "e.g., +,-,[,]" };
            lSystemPanel.Controls.Add(lblConstants);
            lSystemPanel.Controls.Add(txtConstants);
            yPos += 50;

            var lblIterations = new Label { Text = "Iterations:", Location = new Point(5, yPos), Width = controlWidth, AutoSize = false };
            numIterations = new NumericUpDown { Location = new Point(5, yPos + 20), Width = controlWidth, Minimum = 0, Maximum = 10, Value = 3 };
            lSystemPanel.Controls.Add(lblIterations);
            lSystemPanel.Controls.Add(numIterations);
            yPos += 50;

            btnGenerate = new Button { Text = "Generate Program", Location = new Point(5, yPos), Width = controlWidth, Height = 30 };
            btnGenerate.Click += (s, e) => GenerateProgram();
            lSystemPanel.Controls.Add(btnGenerate);
            yPos += 40;

            lblGenerated = new Label { Text = "", Location = new Point(5, yPos), Width = controlWidth, Height = 60, AutoEllipsis = false };
            lSystemPanel.Controls.Add(lblGenerated);
            yPos += 70;

            // Save/Load buttons
            btnSaveSettings = new Button { Text = "Save Settings", Location = new Point(5, yPos), Width = 120, Height = 25 };
            btnSaveSettings.Click += (s, e) => SaveSettings();
            lSystemPanel.Controls.Add(btnSaveSettings);

            btnLoadSettings = new Button { Text = "Load Settings", Location = new Point(135, yPos), Width = 120, Height = 25 };
            btnLoadSettings.Click += (s, e) => LoadSettingsFromFile();
            lSystemPanel.Controls.Add(btnLoadSettings);
            yPos += 35;

            btnSaveImage = new Button { Text = "Save Image", Location = new Point(5, yPos), Width = controlWidth, Height = 30 };
            btnSaveImage.Click += (s, e) => SaveImage();
            lSystemPanel.Controls.Add(btnSaveImage);

            // Action buttons
            btnStart = new Button { Text = "Start", AutoSize = true, Margin = new Padding(3) };
            btnClear = new Button { Text = "Clear", AutoSize = true, Margin = new Padding(3) };
            btnStepForward = new Button { Text = "Step +", AutoSize = true, Margin = new Padding(3) };
            btnStepBackward = new Button { Text = "Step -", AutoSize = true, Margin = new Padding(3) };
            btnRunAll = new Button { Text = "Run All", AutoSize = true, Margin = new Padding(3) };

            btnStart.Click += (s, e) => StartProgram();
            btnClear.Click += (s, e) => ClearCanvas();
            btnStepForward.Click += (s, e) => StepForward();
            btnStepBackward.Click += (s, e) => StepBackward();
            btnRunAll.Click += (s, e) => RunAll();

            // Settings controls
            chkPenDown = new CheckBox
            {
                Text = "Pen Down",
                Checked = true,
                AutoSize = true,
                Padding = new Padding(5)
            };
            chkPenDown.CheckedChanged += (s, e) =>
            {
                penDown = chkPenDown.Checked;
                if (turtle != null)
                {
                    if (penDown) turtle.PenDown();
                    else turtle.PenUp();
                }
            };

            numStep = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 500,
                Value = 10,
                Width = 60,
                DecimalPlaces = 0
            };
            numStep.ValueChanged += (s, e) => stepSize = (float)numStep.Value;

            numAngle = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 180,
                Value = 90,
                Width = 60,
                DecimalPlaces = 0
            };
            numAngle.ValueChanged += (s, e) => angleSize = (float)numAngle.Value;

            numPenWidth = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 20,
                Value = 1,
                Width = 60,
                DecimalPlaces = 0
            };
            numPenWidth.ValueChanged += (s, e) =>
            {
                penWidth = (float)numPenWidth.Value;
                if (turtle != null)
                    turtle.SetWidth(penWidth);
            };

            btnCanvasColor = new Button
            {
                Text = "Canvas",
                AutoSize = true,
                BackColor = canvasColor
            };
            btnCanvasColor.Click += (s, e) =>
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.Color = canvasColor;
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        canvasColor = colorDialog.Color;
                        btnCanvasColor.BackColor = canvasColor;
                        ClearCanvas();
                    }
                }
            };

            btnPenColor = new Button
            {
                Text = "Pen",
                AutoSize = true,
                BackColor = penColor
            };
            btnPenColor.Click += (s, e) =>
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.Color = penColor;
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        penColor = colorDialog.Color;
                        btnPenColor.BackColor = penColor;
                        if (turtle != null)
                            turtle.SetColor(penColor);
                    }
                }
            };

            var btnResetPan = new Button { Text = "Reset View", AutoSize = true, Margin = new Padding(3) };
            btnResetPan.Click += (s, e) =>
            {
                panOffset = new Point(0, 0);
                pictureBox.Invalidate();
            };

            // Layout panels
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                WrapContents = false
            };
            buttonPanel.Controls.Add(btnStart);
            buttonPanel.Controls.Add(btnStepForward);
            buttonPanel.Controls.Add(btnStepBackward);
            buttonPanel.Controls.Add(btnRunAll);
            buttonPanel.Controls.Add(btnClear);

            lblCurrentStep = new Label
            {
                Text = "Step: 0/0",
                AutoSize = true,
                Padding = new Padding(10, 10, 5, 5),
                Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
            };
            buttonPanel.Controls.Add(lblCurrentStep);

            var settingsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                WrapContents = false,
            };
            settingsPanel.Controls.Add(chkPenDown);
            settingsPanel.Controls.Add(new Label { Text = "Step:", AutoSize = true, Padding = new Padding(5) });
            settingsPanel.Controls.Add(numStep);
            settingsPanel.Controls.Add(new Label { Text = "Angle:", AutoSize = true, Padding = new Padding(5) });
            settingsPanel.Controls.Add(numAngle);
            settingsPanel.Controls.Add(new Label { Text = "Width:", AutoSize = true, Padding = new Padding(5) });
            settingsPanel.Controls.Add(numPenWidth);
            settingsPanel.Controls.Add(btnCanvasColor);
            settingsPanel.Controls.Add(btnPenColor);
            settingsPanel.Controls.Add(btnResetPan);

            Controls.Add(pictureBox);
            Controls.Add(buttonPanel);
            Controls.Add(settingsPanel);
            Controls.Add(lSystemPanel);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitCanvas();
            cmbExamples.SelectedIndex = 0;
        }

        private void LoadExample(int index)
        {
            switch (index)
            {
                case 0: // Koch Curve
                    txtAxiom.Text = "F";
                    txtRules.Text = "F=F+F-F-F+F";
                    txtVariables.Text = "F";
                    txtConstants.Text = "+,-";
                    numIterations.Value = 2;
                    numAngle.Value = 90;
                    numStep.Value = 10;
                    break;

                case 1: // Sierpiński Triangle
                    txtAxiom.Text = "F-G-G";
                    txtRules.Text = "F=F-G+F+G-F\nG=GG";
                    txtVariables.Text = "F,G";
                    txtConstants.Text = "+,-";
                    numIterations.Value = 4;
                    numAngle.Value = 120;
                    numStep.Value = 5;
                    break;

                case 2: // Dragon Curve
                    txtAxiom.Text = "F";
                    txtRules.Text = "F=F+G\nG=F-G";
                    txtVariables.Text = "F,G";
                    txtConstants.Text = "+,-";
                    numIterations.Value = 8;
                    numAngle.Value = 90;
                    numStep.Value = 5;
                    break;

                case 3: // Plant
                    txtAxiom.Text = "X";
                    txtRules.Text = "X=F+[[X]-X]-F[-FX]+X\nF=FF";
                    txtVariables.Text = "X,F";
                    txtConstants.Text = "+,-,[,]";
                    numIterations.Value = 4;
                    numAngle.Value = 25;
                    numStep.Value = 5;
                    break;

                case 4: // Tree
                    txtAxiom.Text = "F";
                    txtRules.Text = "F=FF+[+F-F-F]-[-F+F+F]";
                    txtVariables.Text = "F";
                    txtConstants.Text = "+,-,[,]";
                    numIterations.Value = 3;
                    numAngle.Value = 22;
                    numStep.Value = 8;
                    break;

                case 5: // Hilbert Curve
                    txtAxiom.Text = "A";
                    txtRules.Text = "A=-BF+AFA+FB-\nB=+AF-BFB-FA+";
                    txtVariables.Text = "A,B";
                    txtConstants.Text = "F,+,-";
                    numIterations.Value = 4;
                    numAngle.Value = 90;
                    numStep.Value = 8;
                    break;

                case 6: // Fractal Plant 2
                    txtAxiom.Text = "F";
                    txtRules.Text = "F=F[+F]F[-F]F";
                    txtVariables.Text = "F";
                    txtConstants.Text = "+,-,[,]";
                    numIterations.Value = 4;
                    numAngle.Value = 25;
                    numStep.Value = 5;
                    break;
            }
        }

        private void GenerateProgram()
        {
            rules.Clear();
            var ruleLines = txtRules.Text.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in ruleLines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2 && parts[0].Trim().Length == 1)
                {
                    char variable = parts[0].Trim()[0];
                    string replacement = parts[1].Trim();
                    rules[variable] = replacement;
                }
            }

            variables.Clear();
            constants.Clear();

            foreach (char c in txtVariables.Text.Replace(",", "").Replace(" ", ""))
            {
                variables.Add(c);
            }

            foreach (char c in txtConstants.Text.Replace(",", "").Replace(" ", ""))
            {
                constants.Add(c);
            }

            axiom = txtAxiom.Text.Trim();
            string current = axiom;

            int iterations = (int)numIterations.Value;
            for (int i = 0; i < iterations; i++)
            {
                current = ApplyRules(current);
            }

            program = current;
            currentStep = 0;

            string preview = program.Length > 50 ? program.Substring(0, 50) + "..." : program;
            lblGenerated.Text = $"Generated:\n{program.Length} chars\n{preview}";

            UpdateStepLabel();
        }

        private string ApplyRules(string input)
        {
            var result = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (rules.ContainsKey(c))
                {
                    result.Append(rules[c]);
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        private void InitCanvas()
        {
            if (pictureBox.Width <= 0 || pictureBox.Height <= 0) return;

            g?.Dispose();
            canvas?.Dispose();

            int canvasSize = 2000;
            canvas = new Bitmap(canvasSize, canvasSize);
            g = Graphics.FromImage(canvas);
            g.Clear(canvasColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            turtle = new Turtle(g, canvasSize / 2, canvasSize / 2);
            turtle.SetWidth(penWidth);
            turtle.SetColor(penColor);
            if (penDown) turtle.PenDown();
            else turtle.PenUp();

            pictureBox.Invalidate();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (canvas != null)
            {
                e.Graphics.DrawImage(canvas, panOffset.X, panOffset.Y);
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPanning = true;
                lastMousePos = e.Location;
                pictureBox.Cursor = Cursors.SizeAll;
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;

                panOffset.X += dx;
                panOffset.Y += dy;

                lastMousePos = e.Location;
                pictureBox.Invalidate();
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPanning = false;
                pictureBox.Cursor = Cursors.Hand;
            }
        }

        private void StartProgram()
        {
            if (string.IsNullOrEmpty(program))
            {
                MessageBox.Show("Please generate a program first!", "No Program", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            currentStep = 0;
            UpdateStepLabel();
            ClearCanvas();
        }

        private void ClearCanvas()
        {
            if (g == null) return;

            g.Clear(canvasColor);

            int canvasSize = 2000;
            turtle = new Turtle(g, canvasSize / 2, canvasSize / 2);
            turtle.SetWidth(penWidth);
            turtle.SetColor(penColor);
            if (penDown) turtle.PenDown();
            else turtle.PenUp();

            pictureBox.Invalidate();
        }

        private void StepForward()
        {
            if (string.IsNullOrEmpty(program)) return;
            if (currentStep >= program.Length) return;

            char c = program[currentStep];
            InterpretChar(c);
            currentStep++;

            UpdateStepLabel();
            pictureBox.Invalidate();
        }

        private void StepBackward()
        {
            if (currentStep <= 0) return;

            currentStep--;
            g.Clear(canvasColor);

            int canvasSize = 2000;
            turtle = new Turtle(g, canvasSize / 2, canvasSize / 2);
            turtle.SetWidth(penWidth);
            turtle.SetColor(penColor);
            if (penDown) turtle.PenDown();
            else turtle.PenUp();

            for (int i = 0; i < currentStep; i++)
            {
                InterpretChar(program[i]);
            }

            UpdateStepLabel();
            pictureBox.Invalidate();
        }

        private void RunAll()
        {
            if (string.IsNullOrEmpty(program))
            {
                MessageBox.Show("Please generate a program first!", "No Program", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ClearCanvas();
            currentStep = 0;

            for (int i = 0; i < program.Length; i++)
            {
                InterpretChar(program[i]);
                currentStep++;
            }

            UpdateStepLabel();
            pictureBox.Invalidate();
        }

        private void UpdateStepLabel()
        {
            if (string.IsNullOrEmpty(program))
            {
                lblCurrentStep.Text = "Step: 0/0";
                return;
            }

            if (currentStep < program.Length)
            {
                char nextChar = program[currentStep];
                lblCurrentStep.Text = $"Step: {currentStep}/{program.Length} (Next: '{nextChar}')";
            }
            else
            {
                lblCurrentStep.Text = $"Step: {currentStep}/{program.Length} (Done)";
            }
        }

        private void InterpretChar(char c)
        {
            switch (c)
            {
                case 'F':
                case 'G':
                case 'A':
                case 'B':
                    turtle.Forward(stepSize);
                    break;
                case '+': turtle.Right(angleSize); break;
                case '-': turtle.Left(angleSize); break;
                case '[': turtle.PushState(); break;
                case ']': turtle.PopState(); break;
            }
        }

        private void SaveSettings()
        {
            var settings = new LSystemSettings
            {
                Axiom = txtAxiom.Text,
                Rules = txtRules.Text,
                Variables = txtVariables.Text,
                Constants = txtConstants.Text,
                Iterations = (int)numIterations.Value,
                Angle = angleSize,
                Step = stepSize,
                PenWidth = penWidth,
                CanvasColor = canvasColor.ToArgb(),
                PenColor = penColor.ToArgb()
            };

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "L-System Files (*.lsys)|*.lsys|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "lsys";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveDialog.FileName, json);
                    MessageBox.Show("Settings saved successfully!", "Save Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void LoadSettingsFromFile()
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "L-System Files (*.lsys)|*.lsys|All Files (*.*)|*.*";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var json = File.ReadAllText(openDialog.FileName);
                        var settings = JsonSerializer.Deserialize<LSystemSettings>(json);

                        txtAxiom.Text = settings.Axiom;
                        txtRules.Text = settings.Rules;
                        txtVariables.Text = settings.Variables;
                        txtConstants.Text = settings.Constants;
                        numIterations.Value = settings.Iterations;
                        numAngle.Value = (decimal)settings.Angle;
                        numStep.Value = (decimal)settings.Step;
                        numPenWidth.Value = (decimal)settings.PenWidth;

                        canvasColor = Color.FromArgb(settings.CanvasColor);
                        penColor = Color.FromArgb(settings.PenColor);
                        btnCanvasColor.BackColor = canvasColor;
                        btnPenColor.BackColor = penColor;

                        stepSize = settings.Step;
                        angleSize = settings.Angle;
                        penWidth = settings.PenWidth;

                        MessageBox.Show("Settings loaded successfully!", "Load Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading settings: {ex.Message}", "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveImage()
        {
            if (canvas == null)
            {
                MessageBox.Show("No image to save!", "Save Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|Bitmap (*.bmp)|*.bmp";
                saveDialog.DefaultExt = "png";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var format = System.Drawing.Imaging.ImageFormat.Png;

                    if (saveDialog.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    else if (saveDialog.FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        format = System.Drawing.Imaging.ImageFormat.Bmp;

                    canvas.Save(saveDialog.FileName, format);
                    MessageBox.Show("Image saved successfully!", "Save Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }

    public class LSystemSettings
    {
        public string Axiom { get; set; }
        public string Rules { get; set; }
        public string Variables { get; set; }
        public string Constants { get; set; }
        public int Iterations { get; set; }
        public float Angle { get; set; }
        public float Step { get; set; }
        public float PenWidth { get; set; }
        public int CanvasColor { get; set; }
        public int PenColor { get; set; }
    }
}