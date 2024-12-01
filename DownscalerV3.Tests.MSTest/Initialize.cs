using Downscaler;

[assembly: WinUITestTarget(typeof(App))]

namespace DownscalerV3.Tests.MSTest;

[TestClass]
public class Initialize {
  [AssemblyCleanup]
  public static void AssemblyCleanup() {
    Bootstrap.Shutdown();
  }


  [AssemblyInitialize]
  public static void AssemblyInitialize(TestContext context) {
    // TODO: Initialize the appropriate version of the Windows App SDK.
    // This is required when testing MSIX apps that are framework-dependent on the Windows App SDK.
    Bootstrap.TryInitialize(0x00010001, out var _);
  }
}
