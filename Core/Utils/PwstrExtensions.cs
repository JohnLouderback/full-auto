using Windows.Win32.Foundation;

namespace Core.Utils;

public static class PwstrExtensions {
  /// <summary>
  ///   Converts a <see cref="PCWSTR" /> to a managed <see cref="string" />.
  /// </summary>
  /// <param name="pwstr"> The <see cref="PCWSTR" /> to convert. </param>
  /// <returns> A new <see cref="string" /> with the value of the <see cref="PCWSTR" />. </returns>
  public static unsafe string ToString(this PCWSTR pwstr) {
    return new string(pwstr.Value);
  }


  /// <summary>
  ///   Converts a <see cref="PWSTR" /> to a managed <see cref="string" />.
  /// </summary>
  /// <param name="pwstr"> The <see cref="PWSTR" /> to convert. </param>
  /// <returns> A new <see cref="string" /> with the value of the <see cref="PWSTR" />. </returns>
  public static unsafe string ToString(this PWSTR pwstr) {
    return new string(pwstr.Value);
  }
}
