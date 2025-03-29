using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Core.Utils;

/// <summary>
///   <para>
///     Represents a set of objects that uses weak references to store its elements. This allows
///     elements to be garbage collected if there are no other references to them. Useful for storing
///     lists of existing objects without caching them, preventing them from being garbage collected.
///   </para>
///   <para>
///     For example: If you have a list of objects where instantiation is expensive, you can use a
///     <see cref="WeakSet{T}" /> to store them. This way, the objects can be garbage collected if they
///     are no longer needed, but you can still check if they exist in the set if you need to re-use
///     them later and something else is still holding a reference to them. This also is useful for
///     ensuring that comparisons between objects that are conceptually the same are consistent by
///     allowing re-use of the same object instance rather than creating duplicate objects representing
///     the same entity.
///   </para>
/// </summary>
/// <typeparam name="T"> The type of elements in the set. </typeparam>
public class WeakSet<T> : IEnumerable<T> where T : class {
  private static readonly object                          _placeholder = new();
  private readonly        ConditionalWeakTable<T, object> _table       = new();


  /// <summary>
  ///   Adds an element to the set.
  /// </summary>
  /// <param name="item"> The element to add to the set. </param>
  /// <returns>
  ///   <see langword="true" /> if the element was added to the set; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  ///   Thrown when <paramref name="item" /> is
  ///   <see langword="null" />.
  /// </exception>
  public bool Add([DisallowNull] T item) {
    if (item == null) throw new ArgumentNullException(nameof(item), "Item cannot be null.");

    if (!_table.TryGetValue(item, out var found)) {
      _table.Add(item, _placeholder);
      return true;
    }

    return false;
  }


  /// <summary>
  ///   Determines whether the set contains a specific value.
  /// </summary>
  /// <param name="item"> The object to locate in the set. </param>
  /// <returns>
  ///   <see langword="true" /> if <paramref name="item" /> is found in the set; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  ///   Thrown when <paramref name="item" /> is
  ///   <see langword="null" />.
  /// </exception>
  public bool Contains([DisallowNull] T item) {
    if (item == null) throw new ArgumentNullException(nameof(item), "Item cannot be null.");

    return _table.TryGetValue(item, out _);
  }


  public IEnumerator<T> GetEnumerator() {
    foreach (var kvp in _table) {
      var item = kvp.Key;
      if (item != null) {
        yield return item;
      }
    }
  }


  /// <summary>
  ///   Removes the specified element from the set.
  /// </summary>
  /// <param name="item"> The element to remove from the set. </param>
  /// <returns>
  ///   <see langword="true" /> if the element was successfully removed; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  ///   Thrown when <paramref name="item" /> is
  ///   <see langword="null" />.
  /// </exception>
  public bool Remove([DisallowNull] T item) {
    if (item == null) throw new ArgumentNullException(nameof(item), "Item cannot be null.");

    return _table.Remove(item);
  }


  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }
}
