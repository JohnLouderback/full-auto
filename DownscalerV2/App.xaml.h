#pragma once

#include "App.xaml.g.h"

namespace winrt::DownscalerV2::implementation {
  struct App : AppT<App> {
    App();

    void OnLaunched(const Microsoft::UI::Xaml::LaunchActivatedEventArgs&);

    private:
      Microsoft::UI::Xaml::Window window{nullptr};
  };
}
