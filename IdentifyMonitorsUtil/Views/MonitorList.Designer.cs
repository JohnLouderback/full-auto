using BrightIdeasSoftware;

namespace IdentifyMonitorsUtil.Views;

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
    list = new ObjectListView();
    olvColumn1 = new OLVColumn();
    ((System.ComponentModel.ISupportInitialize)list).BeginInit();
    SuspendLayout();
    // 
    // list
    // 
    list.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    list.Location = new Point(0, 0);
    list.Name = "list";
    list.OwnerDraw = true;
    list.Size = new Size(609, 794);
    list.ShowSortIndicators = false;
    list.IsSimpleDragSource = true;
    list.IsSimpleDropSink = true;
    // list.DropSink = new SimpleDropSink() {
    //   CanDropBetween = true,
    //   CanDropOnBackground = false,
    //   CanDropOnItem = false,
    //   CanDropOnSubItem = false
    // };
    list.DropSink         = new RearrangingDropSink();
    list.TabIndex         = 0;
    list.UseExplorerTheme = true;
    list.ShowGroups       = false;
    list.View             = View.Details;
    // 
    // olvColumn1
    // 
    olvColumn1.AspectName = "Name";
    olvColumn1.Text = "Column";
    olvColumn1.Width = 267;
    // 
    // MonitorList
    // 
    ClientSize = new Size(609, 794);
    Controls.Add(list);
    Name = "MonitorList";
    ((System.ComponentModel.ISupportInitialize)list).EndInit();
    ResumeLayout(false);
  }

  private BrightIdeasSoftware.ObjectListView list;
  private BrightIdeasSoftware.OLVColumn      olvColumn1;
  #endregion
}
