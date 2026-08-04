using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lip.Core.RegressionTests;

public sealed class WinX64TheoryAttribute : TheoryAttribute {
  public WinX64TheoryAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1) {
    bool isWinX64 =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
        RuntimeInformation.OSArchitecture == Architecture.X64;

    if (!isWinX64) {
      Skip = "Regression tests run only on win-x64.";
    }
  }
}
