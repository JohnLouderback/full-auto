namespace IdentifyMonitorsUtil.Views;

partial class NumberForm {
  /// <summary>
  ///  Required designer variable.
  /// </summary>
  private System.ComponentModel.IContainer components = null;


  /// <summary>
  ///  Clean up any resources being used.
  /// </summary>
  /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
  protected override void Dispose(bool disposing) {
    if (disposing && (components != null)) {
      components.Dispose();
    }

    base.Dispose(disposing);
  }


  #region Windows Form Designer generated code
  /// <summary>
  ///  Required method for Designer support - do not modify
  ///  the contents of this method with the code editor.
  /// </summary>
  private void InitializeComponent()
  {
    monitorIndex = new Label();
    deviceID = new Label();
    SuspendLayout();
    // 
    // monitorIndex
    // 
    monitorIndex.AutoSize = true;
    monitorIndex.BackColor = Color.Transparent;
    monitorIndex.Dock = DockStyle.Left;
    monitorIndex.Font = new Font("Impact", 175F, FontStyle.Regular, GraphicsUnit.Point, 0);
    monitorIndex.ForeColor = Color.White;
    monitorIndex.ImageAlign = ContentAlignment.TopLeft;
    monitorIndex.Location = new Point(0, 0);
    monitorIndex.Margin = new Padding(0);
    monitorIndex.Name = "monitorIndex";
    monitorIndex.Size = new Size(367, 427);
    monitorIndex.TabIndex = 0;
    monitorIndex.Text = "5";
    monitorIndex.Click += label1_Click;
    // 
    // deviceID
    // 
    deviceID.AutoSize = true;
    deviceID.BackColor = Color.Transparent;
    deviceID.Dock = DockStyle.Fill;
    deviceID.Font = new Font("Trebuchet MS", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
    deviceID.ForeColor = Color.White;
    deviceID.Location = new Point(367, 0);
    deviceID.Name = "deviceID";
    deviceID.Padding = new Padding(0, 70, 0, 0);
    deviceID.Size = new Size(287, 131);
    deviceID.TabIndex = 1;
    deviceID.Text = "\\.\\\\DISPLAY2";
    deviceID.TextAlign = ContentAlignment.MiddleLeft;
    deviceID.Click += label2_Click;
    // 
    // NumberForm
    // 
    AutoScaleDimensions = new SizeF(10F, 25F);
    AutoScaleMode = AutoScaleMode.Font;
    BackColor = Color.Black;
    ClientSize = new Size(800, 450);
    Controls.Add(deviceID);
    Controls.Add(monitorIndex);
    Name = "NumberForm";
    Text = "Form1";
    ResumeLayout(false);
    PerformLayout();
  }
  #endregion

  private Label monitorIndex;
  private Label deviceID;
}
