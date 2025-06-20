using System.ComponentModel;

namespace GenericModLauncher.ViewModels;

public partial class ItemViewModel : INotifyPropertyChanged {
  public string Name        { get; set; }
  public string Group       { get; set; }
  public string Description { get; set; } = string.Empty;
}
