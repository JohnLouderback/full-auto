using System.Collections;
using System.Dynamic;
using System.Reflection;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Utils;

/// <summary>
///   Represents a JavaScript array. It acts as a proxy to a JavaScript array, allowing for
///   manipulation of the array from C#. It implements the <see cref="IList{T}" /> interface,
///   unlike the V8Array class provided by ClearScript, which only implements the IList and
///   IList&lt;object&gt; interfaces.
/// </summary>
/// <typeparam name="T"> The type of the array. </typeparam>
public class JSArray<T> : DynamicHostObject, IEnumerable<T>, IList, IList<T> {
  private readonly ScriptObject jsArray;

  private dynamic? _hasMemberFunc;

  private dynamic? _getMemberFunc;

  /// <inheritdoc />
  public int Count => (int)jsArray.GetProperty("length");

  /// <inheritdoc />
  public bool IsSynchronized { get; } = false;

  /// <inheritdoc />
  public object SyncRoot => this;

  /// <inheritdoc />
  public bool IsFixedSize { get; } = false;

  /// <inheritdoc />
  public bool IsReadOnly { get; } = false;

  private dynamic HasMemberFunc =>
    _hasMemberFunc ??= ScriptEngine.Current.Evaluate("(name, obj) => name in obj");

  private dynamic GetMemberFunc =>
    _getMemberFunc ??= ScriptEngine.Current.Evaluate(
      """
      (name, obj) => {
        const returnVal = obj[name];
        // If the return value is a function, bind it to the object.
        if (typeof returnVal === 'function') {
          return returnVal.bind(obj);
        }
        return returnVal;
      }
      """
    );

  /// <inheritdoc />
  public object? this[int index] {
    get => jsArray.InvokeMethod("at", index);
    set => jsArray.InvokeMethod("Insert", index, value);
  }

  /// <inheritdoc />
  T IList<T>.this[int index] {
    get => throw new NotImplementedException();
    set => throw new NotImplementedException();
  }


  public JSArray(ScriptObject jsArray) {
    this.jsArray = jsArray;
  }


  /// <summary>
  ///   Creates a new JSArray from an existing IEnumerable.
  /// </summary>
  /// <param name="list"> The enumerable to convert. </param>
  /// <typeparam name="T"> The type of the enumerable. </typeparam>
  /// <returns> The new JSArray instance. </returns>
  public static JSArray<T> FromIEnumerable<T>(IEnumerable<T> list) {
    var engine = ScriptEngine.Current;
    return new JSArray<T>(engine.Script.Array.from(list));
  }


  // Implicit conversion from JSArray to ScriptObject
  public static implicit operator ScriptObject(JSArray<T> jsArray) {
    return jsArray.jsArray;
  }


  /// <inheritdoc />
  public int Add(object? value) {
    jsArray.InvokeMethod("push", value);
    return Count - 1;
  }


  /// <inheritdoc />
  public void Add(T item) {
    jsArray.InvokeMethod("push", item);
  }


  /// <inheritdoc />
  public void Clear() {
    jsArray.InvokeMethod("splice", 0, Count);
  }


  /// <inheritdoc />
  public bool Contains(T item) {
    foreach (var element in this) {
      if (Equals(element, item)) {
        return true;
      }
    }

    return false;
  }


  /// <inheritdoc />
  public bool Contains(object? value) {
    foreach (var item in this) {
      if (Equals(item, value)) {
        return true;
      }
    }

    return false;
  }


  /// <inheritdoc />
  public void CopyTo(Array array, int index) {
    var i = index;
    foreach (var item in this) {
      array.SetValue(item, i++);
    }
  }


  /// <inheritdoc />
  public void CopyTo(T[] array, int arrayIndex) {
    var i = arrayIndex;
    foreach (var item in this) {
      array[i++] = item;
    }
  }


  /// <inheritdoc />
  public IEnumerator<T> GetEnumerator() {
    // Get the length of the JavaScript array
    var lengthObj = jsArray.GetProperty("length");
    if (lengthObj is not int length) {
      throw new InvalidOperationException("Could not retrieve the length of the JS array.");
    }

    for (var i = 0; i < length; i++) {
      var element = jsArray.GetProperty(i);
      if (element is T typedElement) {
        yield return typedElement;
      }
      else {
        throw new InvalidCastException(
          $"Could not cast element at index {i} to type {typeof(T).Name}."
        );
      }
    }
  }


  public override bool HasMember(string name, bool ignoreCase) {
    return HasMemberFunc(name, jsArray);
  }


  /// <inheritdoc />
  public int IndexOf(object? value) {
    var index = 0;
    foreach (var item in this) {
      if (Equals(item, value)) {
        return index;
      }

      index++;
    }

    return -1;
  }


  /// <inheritdoc />
  public int IndexOf(T item) {
    var index = 0;
    foreach (var element in this) {
      if (Equals(element, item)) {
        return index;
      }

      index++;
    }

    return -1;
  }


  /// <inheritdoc />
  public void Insert(int index, object? value) {
    jsArray.InvokeMethod("splice", index, 0, value);
  }


  /// <inheritdoc />
  public void Insert(int index, T item) {
    jsArray.InvokeMethod("splice", index, 0, item);
  }


  /// <inheritdoc />
  public bool Remove(T item) {
    throw new NotImplementedException();
  }


  /// <inheritdoc />
  public void Remove(object? value) {
    var index = IndexOf(value);
    if (index != -1) {
      RemoveAt(index);
    }
  }


  /// <inheritdoc />
  public void RemoveAt(int index) {
    jsArray.InvokeMethod("splice", index, 1);
  }


  public override bool TryGetMember(GetMemberBinder binder, out object result) {
    try {
      // First, try to get the member directly from the JavaScript array members.
      if (HasMemberFunc(binder.Name, jsArray)) {
        result = GetMemberFunc(binder.Name, jsArray);
        return true;
      }

      // Failing that, try to get the member from the DynamicObject members.
      result =
        jsArray.GetType()
          .GetProperty(binder.Name, BindingFlags.Public | BindingFlags.Instance)
          ?.GetValue(jsArray) ??
        (jsArray is DynamicObject dynObj && dynObj.TryGetMember(binder, out result)
           ? result
           : null);
      return result != null;
    }
    catch {
      // If an exception occurs, return false.
      result = null;
      return false;
    }
  }


  public override bool TryInvokeMember(
    InvokeMemberBinder binder,
    object[] args,
    out object result
  ) {
    try {
      var method = jsArray.GetType()
        .GetMethod(binder.Name, BindingFlags.Public | BindingFlags.Instance);
      if (method != null) {
        result = method.Invoke(jsArray, args);
        return true;
      }

      if (jsArray is DynamicObject dynObj) {
        return dynObj.TryInvokeMember(binder, args, out result);
      }

      result = null;
      return false;
    }
    catch {
      result = null;
      return false;
    }
  }


  public override bool TrySetMember(SetMemberBinder binder, object value) {
    try {
      var property = jsArray.GetType()
        .GetProperty(binder.Name, BindingFlags.Public | BindingFlags.Instance);
      if (property != null &&
          property.CanWrite) {
        property.SetValue(jsArray, value);
        return true;
      }

      if (jsArray is DynamicObject dynObj) {
        return dynObj.TrySetMember(binder, value);
      }

      return false;
    }
    catch {
      return false;
    }
  }


  /// <inheritdoc />
  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }
}
