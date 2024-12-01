using System.Numerics;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Downscaler.Controls;

public sealed partial class TextBlockWithShadow : UserControl {
  public static DependencyProperty TextProperty = DependencyProperty.Register(
    nameof(Text),
    typeof(string),
    typeof(TextBlockWithShadow),
    new PropertyMetadata("")
  );

  public static DependencyProperty FontSizeProperty = DependencyProperty.Register(
    nameof(FontSize),
    typeof(double),
    typeof(TextBlockWithShadow),
    new PropertyMetadata(12d)
  );

  public static DependencyProperty FontFamilyProperty = DependencyProperty.Register(
    nameof(FontFamily),
    typeof(FontFamily),
    typeof(TextBlockWithShadow),
    new PropertyMetadata(new FontFamily("Segoe UI"))
  );

  public static DependencyProperty FontWeightProperty = DependencyProperty.Register(
    nameof(FontWeight),
    typeof(FontWeight),
    typeof(TextBlockWithShadow),
    new PropertyMetadata(FontWeights.Normal)
  );

  public static DependencyProperty ShadowColorProperty = DependencyProperty.Register(
    nameof(ShadowColor),
    typeof(Color),
    typeof(TextBlockWithShadow),
    new PropertyMetadata(Color.FromArgb(255, 190, 87, 199))
  );

  public static DependencyProperty ShadowRadiusProperty = DependencyProperty.Register(
    nameof(ShadowRadius),
    typeof(double),
    typeof(TextBlockWithShadow),
    new PropertyMetadata(20d)
  );

  public static DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(
    nameof(ShadowOpacity),
    typeof(double),
    typeof(TextBlockWithShadow),
    new PropertyMetadata(20d)
  );

  public string Text {
    get => (string)GetValue(TextProperty);
    set => SetValue(TextProperty, value);
  }

  public double FontSize {
    get => (double)GetValue(FontSizeProperty);
    set => SetValue(FontSizeProperty, value);
  }

  public FontFamily FontFamily {
    get => (FontFamily)GetValue(FontFamilyProperty);
    set => SetValue(FontFamilyProperty, value);
  }

  public FontWeight FontWeight {
    get => (FontWeight)GetValue(FontWeightProperty);
    set => SetValue(FontWeightProperty, value);
  }

  public Color ShadowColor {
    get => (Color)GetValue(ShadowColorProperty);
    set => SetValue(ShadowColorProperty, value);
  }

  public double ShadowRadius {
    get => (double)GetValue(ShadowRadiusProperty);
    set => SetValue(ShadowRadiusProperty, value);
  }

  public double ShadowOpacity {
    get => (double)GetValue(ShadowOpacityProperty);
    set => SetValue(ShadowOpacityProperty, value);
  }


  public TextBlockWithShadow() {
    InitializeComponent();
  }


  public new void SetValue(DependencyProperty dp, object value) {
    base.SetValue(dp, value);
    RefreshShadow();
  }


  private void ClearShadow() {
    ElementCompositionPreview.SetElementChildVisual(Host, null);
  }


  private void MakeShadow() {
    var compositor = ElementCompositionPreview.GetElementVisual(Host).Compositor;

    var dropShadow = compositor.CreateDropShadow();
    dropShadow.Color      = ShadowColor;
    dropShadow.BlurRadius = (float)ShadowRadius;
    dropShadow.Opacity    = (float)ShadowOpacity;

    var mask = TextBlock.GetAlphaMask();
    dropShadow.Mask = mask;

    var spriteVisual = compositor.CreateSpriteVisual();
    spriteVisual.Size   = new Vector2((float)Host.ActualWidth, (float)Host.ActualHeight);
    spriteVisual.Shadow = dropShadow;
    ElementCompositionPreview.SetElementChildVisual(Host, spriteVisual);
  }


  private void OnLoaded(object sender, RoutedEventArgs e) {
    MakeShadow();
  }


  private void RefreshShadow() {
    ClearShadow();
    MakeShadow();
  }
}
