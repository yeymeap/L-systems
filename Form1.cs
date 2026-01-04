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
        private TextBox txtProgram;
        private Button btnStart, btnClear, btnStepForward, btnStepBackward;
        private CheckBox chkPenDown;
        private NumericUpDown numStep, numAngle, numPenWidth;
        private Button btnCanvasColor, btnPenColor;
        private Label lblCurrentStep;

        // Settings
        private Color canvasColor = Color.Orange;
        private Color penColor = Color.Green;
        private float stepSize = 50f;
        private float angleSize = 25f;
        private float penWidth = 3f;
        private bool penDown = true;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            Width = 1000;
            Height = 700;
            Text = "L-systems Turtle";

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Orange,
            };
            pictureBox.SizeChanged += (s, e) => InitCanvas();

            txtProgram = new TextBox
            {
                Dock = DockStyle.Top,
                PlaceholderText = "Enter L-system program here...",
                Height = 30
            };

            // Action buttons
            btnStart = new Button { Text = "Start", AutoSize = true, Margin = new Padding(3) };
            btnClear = new Button { Text = "Clear", AutoSize = true, Margin = new Padding(3) };
            btnStepForward = new Button { Text = "Step +", AutoSize = true, Margin = new Padding(3) };
            btnStepBackward = new Button { Text = "Step -", AutoSize = true, Margin = new Padding(3) };

            btnStart.Click += (_, _) => StartProgram();
            btnClear.Click += (_, _) => ClearCanvas();
            btnStepForward.Click += (_, _) => StepForward();
            btnStepBackward.Click += (_, _) => StepBackward();

            chkPenDown = new CheckBox
            {
                Text = "Pen Down",
                Checked = true,
                AutoSize = true,
                Padding = new Padding(5)
            };
            chkPenDown.CheckedChanged += (_, _) =>
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
                Value = 50,
                Width = 60
            };
            numStep.ValueChanged += (_, _) => stepSize = (float)numStep.Value;

            numAngle = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 180,
                Value = 25,
                Width = 60
            };
            numAngle.ValueChanged += (_, _) => angleSize = (float)numAngle.Value;

            numPenWidth = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 20,
                Value = 3,
                Width = 60
            };
            numPenWidth.ValueChanged += (_, _) =>
            {
                penWidth = (float)numPenWidth.Value;
                if (turtle != null)
                    turtle.SetWidth(penWidth);
            };

            btnCanvasColor = new Button
            {
                Text = "Canvas Color",
                AutoSize = true,
                BackColor = canvasColor
            };
            btnCanvasColor.Click += (_, _) =>
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
                Text = "Pen Color",
                AutoSize = true,
                BackColor = penColor
            };
            btnPenColor.Click += (_, _) =>
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

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                WrapContents = false
            };
            buttonPanel.Controls.Add(btnStart);
            buttonPanel.Controls.Add(btnStepForward);
            buttonPanel.Controls.Add(btnStepBackward);
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
                WrapContents = false
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

            Controls.Add(pictureBox);
            Controls.Add(buttonPanel);
            Controls.Add(settingsPanel);
            Controls.Add(txtProgram);
        }
        private void InitCanvas()
        {
            if (pictureBox.Width <= 0 || pictureBox.Height <= 0) return;

            g?.Dispose();
            canvas?.Dispose();

            canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            g = Graphics.FromImage(canvas);
            g.Clear(canvasColor);

            turtle = new Turtle(g, canvas.Width / 2, canvas.Height / 2);
            turtle.SetWidth(penWidth);
            turtle.SetColor(penColor);
            if (penDown) turtle.PenDown();
            else turtle.PenUp();

            pictureBox.Image = canvas;
            pictureBox.Refresh();
        }

        private void StartProgram()
        {
            program = txtProgram.Text;
            currentStep = 0;
            UpdateStepLabel();
            ClearCanvas();
        }

        private void ClearCanvas()
        {
            if (g == null) return;

            g.Clear(canvasColor);
            turtle = new Turtle(g, canvas.Width / 2, canvas.Height / 2);
            turtle.SetWidth(penWidth);
            turtle.SetColor(penColor);
            if (penDown) turtle.PenDown();
            else turtle.PenUp();

            pictureBox.Image = canvas;
            pictureBox.Refresh();
        }

        private void StepForward()
        {
            if (string.IsNullOrEmpty(program))
            {
                return;
            }

            if (currentStep >= program.Length)
            {
                return;
            }

            char c = program[currentStep];
            InterpretChar(c);
            currentStep++;

            UpdateStepLabel();
            pictureBox.Image = canvas;
            pictureBox.Refresh();
        }

        private void UpdateStepLabel()
        {
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

        private void StepBackward()
        {
            if (currentStep <= 0) return;

            currentStep--;
            g.Clear(canvasColor);

            turtle = new Turtle(g, canvas.Width / 2, canvas.Height / 2);
            turtle.SetWidth(penWidth);
            turtle.SetColor(penColor);
            if (penDown) turtle.PenDown();
            else turtle.PenUp();

            for (int i = 0; i < currentStep; i++)
            {
                InterpretChar(program[i]);
            }

            UpdateStepLabel();
            pictureBox.Image = canvas;
            pictureBox.Refresh();
        }

        private void InterpretChar(char c)
        {
            switch (c)
            {
                case 'F': turtle.Forward(stepSize); break;
                case '+': turtle.Right(angleSize); break;
                case '-': turtle.Left(angleSize); break;
                case '[': turtle.PushState(); break;
                case ']': turtle.PopState(); break;
            }
        }
    }
}