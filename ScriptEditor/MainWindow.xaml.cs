using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace ScriptEditor;

/// <summary>
///   Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
  public MainWindow() {
    InitializeComponent();
    InitializeWebView();
  }


  // Block all external requests, only allow local content
  private void BlockExternalRequests(object? sender, CoreWebView2WebResourceRequestedEventArgs e) {
    var uri = new Uri(e.Request.Uri);

    // Allow only local files or predefined URLs
    if (!(uri.Scheme == "https" && uri.Host == "app.local") &&
        !(uri.Scheme == "file")) {
      e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(
        null,
        403,
        "Blocked",
        "Content-Type: text/plain"
      );
    }
  }


  private void ConfigureWebViewSettings() {
    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled =
      false; // Disable right-click context menus
    //webView.CoreWebView2.Settings.AreDevToolsEnabled = false; // Disable DevTools
    webView.CoreWebView2.Settings.IsReputationCheckingRequired = false; // Disable SmartScreen
    webView.CoreWebView2.Settings.IsScriptEnabled = true; // Enable JavaScript (optional)
  }


  private string GetPrivacyFlags() {
    return string.Join(
      " ",
      "--disable-background-networking", // Prevents WebView2 from making background connections
      "--disable-sync", // Disables Microsoft Edge Sync (prevents syncing data to Microsoft)
      "--disable-search-engine-choice-screen", // Blocks the prompt asking users to select a search engine
      "--disable-default-apps", // Prevents Edge from loading built-in web applications
      "--disable-ipc-flooding-protection", // Removes rate-limiting on inter-process communication (may improve performance)
      "--disable-domain-reliability", // Disables tracking mechanisms used for domain reliability checks
      "--disable-component-update", // Prevents WebView2 from downloading and updating components
      "--disable-client-side-phishing-detection", // Disables Google's Safe Browsing phishing detection (removes unnecessary network requests)
      "--disable-features=msTelemetry,msEdgeTelemetry", // Disables Microsoft telemetry
      //"--disable-remote-fonts", // Prevents downloading and using remote fonts (reduces fingerprinting risk)
      "--disable-network-prediction", // Stops Edge from predicting and preloading network requests
      "--disable-crash-reporter", // Prevents sending crash reports to Microsoft
      "--disable-logging", // Turns off logging features to enhance privacy
      "--disable-dns-prefetch", // Prevents WebView2 from resolving domain names in advance
      "--disable-loading-with-placeholder", // Stops predictive loading behavior (reduces pre-caching)
      "--disable-component-extensions-with-background-pages", // Blocks Edge extensions from running background pages
      "--disable-prediction", // Turns off predictive services and auto-completions
      "--disable-metrics-reporting", // Blocks telemetry that reports usage metrics
      "--no-experiments", // Prevents participation in Microsoft’s A/B testing experiments
      "--disable-site-isolation-trials", // Ensures no site isolation experiments are enabled
      "--disable-web-bluetooth", // Blocks access to Web Bluetooth API (prevents Bluetooth tracking)
      "--disable-webusb", // Blocks access to WebUSB API (prevents unauthorized USB access)
      "--disable-webxr", // Blocks Virtual/Augmented Reality (VR/AR) tracking APIs
      "--disable-print-preview", // Disables the print preview dialog
      "--no-first-run" // Ensures no first-run experience popups appear
    );
  }


  private async void InitializeWebView() {
    var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: Path.Combine(
                  Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                  "SecureWebView2"
                ),
                options: new CoreWebView2EnvironmentOptions(GetPrivacyFlags())
              );

    await webView.EnsureCoreWebView2Async(env);
    ConfigureWebViewSettings();

    // Get the directory where the EXE is located
    var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
    var localWebRoot = Path.Combine(exeDirectory, "wwwroot"); // Local folder inside EXE directory

    // Map the exe's local "wwwroot" folder to an HTTPS host
    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
      "app.local",
      localWebRoot,
      CoreWebView2HostResourceAccessKind.Allow
    );

    // Intercept and block all external requests
    webView.CoreWebView2.AddWebResourceRequestedFilter(
      "*",
      CoreWebView2WebResourceContext.All
    );
    webView.CoreWebView2.WebResourceRequested += BlockExternalRequests;

    webView.Source = new Uri("https://app.local/index.html");

    webView.CoreWebView2.OpenDevToolsWindow();
  }
}
