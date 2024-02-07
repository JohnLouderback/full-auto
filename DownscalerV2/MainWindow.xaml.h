#pragma once

#include "MainWindow.g.h"

namespace winrt::DownscalerV2::implementation
{
    struct MainWindow : MainWindowT<MainWindow>
    {
        MainWindow()
        {
            // Xaml objects should not call InitializeComponent during construction.
            // See https://github.com/microsoft/cppwinrt/tree/master/nuget#initializecomponent
        }

        int32_t MyProperty();
        void MyProperty(int32_t value);

        void window_SizeChanged(IInspectable const& sender, Microsoft::UI::Xaml::WindowSizeChangedEventArgs const& args);
        void fps_Loaded(IInspectable const& sender, Microsoft::UI::Xaml::RoutedEventArgs const& args);
        
        /** Updates the positions of UI elements that require programmatic positioning */
        void UpdatePositions();
    };
}

namespace winrt::DownscalerV2::factory_implementation
{
    struct MainWindow : MainWindowT<MainWindow, implementation::MainWindow>
    {
    };
}
