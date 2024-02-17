using System.Runtime.InteropServices;
using System.Text;

namespace DownscalerV3.Core.Utils;

/// <summary>
///   A type-safe wrapper around a native buffer (C array) that can be used in C#. The
///   <c> GetPointer </c>
///   method can be used to get a pointer to the buffer for use in native interop.
/// </summary>
public class NativeBuffer<T> : IDisposable where T : struct {
  /// <summary>
  ///   Whether or not the buffer has been disposed from destructors.
  /// </summary>
  private bool disposed;

  /// <summary>
  ///   The length of the buffer. A buffer with a single item has a length of 1. The length is
  ///   immutable after the buffer is created.
  /// </summary>
  public int Length { get; }

  /// <summary>
  ///   The buffer itself.
  /// </summary>
  private T[] Buffer { get; }

  /// <summary>
  ///   A handle to the buffer. This is used to pin the buffer in memory so that it is not moved by
  ///   the garbage collector.
  /// </summary>
  private GCHandle GCHandle { get; }


  ~NativeBuffer() {
    Dispose(false);
  }


  /// <summary>
  ///   Indexer for the buffer. This allows for array-like access to the buffer.
  /// </summary>
  /// <param name="index"> The index of the item to get or set. </param>
  public ref T this[int index] => ref Buffer[index];


  /// <summary>
  ///   Creates a new native buffer with the given length.
  /// </summary>
  /// <param name="length"> The length of the buffer. </param>
  public NativeBuffer(int length) {
    Length = length;
    Buffer = new T[length];

    // Pin the buffer in memory
    GCHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
  }


  /// <inheritdoc />
  public void Dispose() {
    Dispose(true);
    GC.SuppressFinalize(this);
  }


  /// <summary>
  ///   <para>
  ///     Gets a pointer to the buffer. This method is unsafe and should be used with caution. The
  ///     pointer should not be used after the buffer is disposed. The purpose of this method is to
  ///     allow for interop with native code.
  ///   </para>
  ///   <para>
  ///     This method differs from <see cref="GetPointerAsUnknown" /> in that it returns a typed pointer
  ///     to the buffer whereas <see cref="GetPointerAsUnknown" /> returns a native untyped pointer. This
  ///     method is useful when the type of the buffer is known at compile time.
  ///   </para>
  /// </summary>
  /// <returns> </returns>
  /// <exception cref="ObjectDisposedException"> </exception>
  public unsafe T* GetPointer() {
    if (disposed) {
      throw new ObjectDisposedException(
        nameof(NativeBuffer<T>),
        "Cannot get pointer, the buffer is disposed."
      );
    }

    return (T*)GCHandle.AddrOfPinnedObject().ToPointer();
  }


  /// <summary>
  ///   <para>
  ///     Gets a pointer to the buffer. This method is unsafe and should be used with caution. The
  ///     pointer should not be used after the buffer is disposed. The purpose of this method is to
  ///     allow for interop with native code.
  ///   </para>
  ///   <para>
  ///     This method differs from <see cref="GetPointer" /> in that it returns a native untyped pointer
  ///     to the buffer whereas <see cref="GetPointer" /> returns a typed pointer. This method is useful
  ///     when the type of the buffer is not known at compile time.
  ///   </para>
  /// </summary>
  /// <returns> A pointer to the buffer. </returns>
  public unsafe nint GetPointerAsUnknown() {
    if (disposed) {
      throw new ObjectDisposedException(
        nameof(NativeBuffer<T>),
        "Cannot get pointer, the buffer is disposed."
      );
    }

    return (nint)GCHandle.AddrOfPinnedObject().ToPointer();
  }


  /// <summary>
  ///   Converts the buffer to a managed string. This method is useful for converting a buffer of
  ///   bytes to a string. If no encoding is provided, the default encoding is UTF-8. If you wish to
  ///   decode native types such as <c> wchar_t </c> or <c> char16_t </c>, you will need to provide
  ///   the UTF-16 or UTF-32 encoding, respectively.
  /// </summary>
  /// <param name="encoding"> Optionally, the encoding to use when converting the buffer to a string. </param>
  /// <returns> A managed string representation of the buffer. </returns>
  /// <example>
  ///   <code lang="cs">
  ///   // Convert a buffer of ASCII / UTF-8 characters to a managed string.
  ///   var buffer = new NativeBuffer&lt;char&gt;(new char[] { 72, 101, 108, 108, 111 });
  ///   var str = buffer.ToManagedString(Encoding.UTF8); // "Hello"
  /// 
  ///   // Convert a buffer of UTF-16 characters to a managed string.
  ///   var buffer = new NativeBuffer&lt;char&gt;(new char[] { 72, 0, 101, 0, 108, 0, 108, 0, 111, 0 });
  ///   var str = buffer.ToManagedString(Encoding.Unicode); // "Hello"
  ///   </code>
  /// </example>
  public string ToManagedString(Encoding? encoding = null) {
    // If no encoding is provided, default to UTF-8.
    encoding ??= Encoding.UTF8;

    return encoding.GetString(Buffer.Select(b => (byte)(object)b).ToArray());
  }


  /// <summary>
  ///   Disposes of the buffer. This method is called by both the <c> Dispose </c> method and the
  ///   destructor.
  /// </summary>
  /// <param name="disposing">
  ///   Whether or not the buffer is being disposed from the <c> Dispose </c>
  ///   method.
  /// </param>
  protected virtual void Dispose(bool disposing) {
    if (!disposed) {
      if (disposing) {
        // Free the GCHandle when the buffer is no longer needed
        if (GCHandle.IsAllocated) {
          GCHandle.Free();
        }
      }

      disposed = true;
    }
  }
}
