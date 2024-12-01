using System.Numerics;

namespace Downscaler.Core.Utils;

/// <summary>
///   Provides utility methods for mathematical operations.
/// </summary>
public static class MathUtils {
  /// <summary>
  ///   Rounds the given value to the nearest multiple of the given multiple. For example, if the
  ///   value is 5 and the multiple is 3, the result will be 6.
  /// </summary>
  /// <param name="value"> The value to round. </param>
  /// <param name="multiple"> The multiple to round to. Must not be zero. </param>
  /// <param name="rounding"> The rounding strategy to use. </param>
  /// <typeparam name="T"> The numeric type of the value. </typeparam>
  /// <returns> The rounded value. </returns>
  /// <exception cref="ArgumentException"> Thrown if the multiple is zero. </exception>
  public static T RoundToNearestMultiple<T>(
    T value,
    T multiple,
    MidpointRounding rounding = MidpointRounding.AwayFromZero
  )
    where T : IFloatingPoint<T> {
    if (multiple == T.Zero) {
      throw new ArgumentException("Multiple must not be zero.", nameof(multiple));
    }

    // value / multiple -> rounded -> * multiple
    return T.Round(value / multiple, rounding) * multiple;
  }
}
