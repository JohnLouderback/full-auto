using Windows.Win32;
using Windows.Win32.Foundation;
using DownscalerV3.Core.Utils;
using DownscalerV3.Tests.MSTest.TestUtils;

namespace DownscalerV3.Tests.MSTest.Tests.Utils;

[TestClass]
public class HwndExtensions {
  public static HWND Hwnd { get; private set; }


  [ClassCleanup]
  public static void ClassCleanup() {
    // Close the window
    PInvoke.DestroyWindow(Hwnd);
  }


  [ClassInitialize]
  public static void ClassInitialize(TestContext context) {
    // Create a new Win32 Window and get its HWND
    Hwnd = ShellUtils.CreateAndShowTestWindow();
  }


  [TestMethod]
  public void GetClassNameTest() {
    // Get the class name of the window
    var className = Hwnd.GetClassName();
    Assert.AreEqual(ShellUtils.ClassName, className);
  }
}
