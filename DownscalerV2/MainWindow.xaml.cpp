#include "pch.h"
#include "MainWindow.xaml.h"
#if __has_include("MainWindow.g.cpp")
#include "MainWindow.g.cpp"
#endif

using namespace winrt;
using namespace Microsoft::UI::Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace winrt::DownscalerV2::implementation
{
    int32_t MainWindow::MyProperty()
    {
        throw hresult_not_implemented();
    }

    void MainWindow::MyProperty(int32_t /* value */)
    {
        throw hresult_not_implemented();
    }

    void MainWindow::window_SizeChanged(IInspectable const&, WindowSizeChangedEventArgs const&)
    {
      this->UpdatePositions();
    }

    void MainWindow::fps_Loaded(IInspectable const& sender, RoutedEventArgs const& args) {
      this->UpdatePositions();
    }

    void MainWindow::UpdatePositions() {
      // Get the widths for each element so we can calculate the correct positions.
      auto fpsWidth = fps().ActualWidth();
      auto canvasWidth = canvas().ActualWidth();
      
      // Move the FPS text to right edge with 5 pixels padding.
      canvas().SetLeft(fps(), canvasWidth - fpsWidth - 5);
    }
}
