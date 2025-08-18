using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using static GameLauncher.Script.Utils.JSTypeConverter;
using static GameLauncher.Script.Utils.JSInteropUtils;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a callback that is invoked for each line read from a file.
/// </summary>
/// <param name="line">
///   Represents the current line of text read from the file. Lines are separated by the newline
///   character.
/// </param>
public delegate void ReadLinesCallback(string line);

public delegate void ReadBytesCallback([TsTypeOverride("number")] byte @byte);

[TypeScriptExport]
public class File : DirectoryEntry {
  public File(string path) : base(path) {}


  /// <summary>
  ///   Reads the entire content of the file as a byte array asynchronously.
  /// </summary>
  /// <returns>
  ///   A promise that resolves when the file is fully read, returning a <c>Uint8Array</c>
  ///   containing the file's bytes.
  /// </returns>
  [TsReturnTypeOverride("Promise<Uint8Array>")]
  [ScriptMember("readAllBytes")]
  public async Task<ScriptObject> ReadAllBytes() {
    var fileInfo = new FileInfo(Path);
    var result   = new byte[fileInfo.Length]; // preallocate exact size

    await using var stream = new FileStream(
      Path,
      FileMode.Open,
      FileAccess.Read,
      FileShare.Read,
      128 * 1024, // larger buffer reduces syscalls
      FileOptions.Asynchronous | FileOptions.SequentialScan
    );

    var offset = 0;
    while (offset < result.Length) {
      var read = await stream.ReadAsync(result.AsMemory(offset)).ConfigureAwait(false);
      if (read == 0) break; // truncated file edge case
      offset += read;
    }

    if (offset != result.Length) {
      // File shrank while reading; trim to the actual bytes read.
      Array.Resize(ref result, offset);
    }

    return CreateUInt8Array(result);
  }


  /// <summary>
  ///   Reads the content of the file assuming it is a text file and returns it as a string.
  /// </summary>
  [ScriptMember("readAllText")]
  public async Task<string> ReadAllText() {
    using var reader = new StreamReader(Path);
    return await reader.ReadToEndAsync().ConfigureAwait(false);
  }


  /// <summary>
  ///   Asynchronously reads the file byte by byte and invokes the specified callback for each byte.
  ///   This method is useful for processing binary files or when you need to handle each byte
  ///   individually, such as for custom parsing or processing of binary data.
  /// </summary>
  /// <param name="callback">
  ///   A callback to be called for each byte read from the file. The callback receives a single
  ///   byte as an argument, which represents the current byte read from the file.
  /// </param>
  [ScriptMember("readBytes")]
  public async Task ReadBytes(ReadBytesCallback callback) {
    var buffer = new byte[4096];
    await using var stream = new FileStream(
      Path,
      FileMode.Open,
      FileAccess.Read,
      FileShare.Read,
      buffer.Length,
      useAsync: true
    );

    int bytesRead;
    while (
      (bytesRead = await stream
                     .ReadAsync(buffer, offset: 0, buffer.Length)
                     .ConfigureAwait(false)) >
      0) {
      for (var i = 0; i < bytesRead; i++) {
        callback(buffer[i]);
      }
    }
  }


  /// <inheritdoc cref="ReadBytes(ReadBytesCallback)" />
  [HideFromTypeScript]
  public async Task ReadBytes(ScriptObject callback) {
    if (!IsFunction(callback)) {
      throw new ArgumentException(
        $"Invalid callback. Expected a function, but got \"{GetJSType(callback)}\"."
      );
    }

    await ReadBytes(@byte => { callback.Invoke(asConstructor: false, @byte); }
      )
      .ConfigureAwait(false);
  }


  /// <summary>
  ///   Asynchronously reads the file line by line and invokes the specified callback for each line.
  /// </summary>
  /// <param name="callback">A callback to be called for each line of text.</param>
  [ScriptMember("readLines")]
  public async Task ReadLines(ReadLinesCallback callback) {
    using var reader = new StreamReader(Path);
    string?   line;
    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null) {
      callback(line);
    }
  }


  /// <inheritdoc cref="ReadLines(ReadLinesCallback)" />
  [HideFromTypeScript]
  public async Task ReadLines(ScriptObject callback) {
    if (!IsFunction(callback)) {
      throw new ArgumentException(
        $"Invalid callback. Expected a function, but got \"{GetJSType(callback)}\"."
      );
    }

    await ReadLines(line => { callback.Invoke(asConstructor: false, line); }).ConfigureAwait(false);
  }
}
