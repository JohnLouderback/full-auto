using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Downscaler.Contracts.Services;

public interface INavigationService {
  bool CanGoBack { get; }

  Frame? Frame { get; set; }

  event NavigatedEventHandler Navigated;

  bool GoBack();

  bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);
}
