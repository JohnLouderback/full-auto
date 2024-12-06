namespace IdentifyMonitorsUtil;

partial class MonitorList {
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
    dataGridView1 = new DataGridView();
    Column1 = new DataGridViewTextBoxColumn();
    Column2 = new DataGridViewTextBoxColumn();
    ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
    SuspendLayout();
    // 
    // dataGridView1
    // 
    dataGridView1.AllowUserToOrderColumns = true;
    dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
    dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2 });
    dataGridView1.Dock = DockStyle.Fill;
    dataGridView1.Location = new Point(0, 0);
    dataGridView1.Name = "dataGridView1";
    dataGridView1.RowHeadersWidth = 62;
    dataGridView1.Size = new Size(609, 794);
    dataGridView1.TabIndex = 0;
    // 
    // Column1
    // 
    Column1.HeaderText = "Column1";
    Column1.MinimumWidth = 8;
    Column1.Name = "Column1";
    Column1.Width = 150;
    // 
    // Column2
    // 
    Column2.HeaderText = "Column2";
    Column2.MinimumWidth = 8;
    Column2.Name = "Column2";
    Column2.Width = 150;
    // 
    // MonitorList
    // 
    ClientSize = new Size(609, 794);
    Controls.Add(dataGridView1);
    Name = "MonitorList";
    ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
    ResumeLayout(false);
  }
  #endregion

  private DataGridView dataGridView1;
  private DataGridViewTextBoxColumn Column1;
  private DataGridViewTextBoxColumn Column2;
}
