const fs = require("fs-extra");
const path = require("path");

console.log("Creating merged distribution directory...");

class DistributionBuilder {
  constructor() {
    this.buildOutputsDir = path.join(__dirname, "..", "build-outputs");
    this.distDir = path.join(__dirname, "..", "dist");
    this.dllHashes = new Map(); // Track DLLs for deduplication
    this.duplicateCount = 0;
    this.hashMismatches = [];
  }

  async initialize() {
    // Clean and create distribution directory
    await fs.ensureDir(this.distDir);
    await fs.emptyDir(this.distDir);
    console.log(`Initialized distribution directory: ${this.distDir}`);
  }

  calculateFileHash(filePath) {
    const crypto = require("crypto");
    const fileBuffer = fs.readFileSync(filePath);
    return crypto.createHash("sha256").update(fileBuffer).digest("hex");
  }

  async mergeAllProjects() {
    console.log(
      "\n📦 Creating distribution with isolated project structure..."
    );

    // Separate Downscaler (WinUI/Windows 10+) from utilities
    const utilityProjects = [
      "GameLauncher",
      "IdentifyMonitorsUtil",
      "MonitorFadeUtil",
      "DiagnosticWindow",
      "ScriptEditor"
    ];

    let totalFilesProcessed = 0;

    // Process utility projects into main directory (these are compatible)
    console.log("\n🔧 Processing utility projects (shared compatibility)...");
    for (const projectName of utilityProjects) {
      const projectSourceDir = path.join(this.buildOutputsDir, projectName);

      if (!(await fs.pathExists(projectSourceDir))) {
        console.warn(`  ⚠️  Project ${projectName} not found in build outputs`);
        continue;
      }

      console.log(`\nProcessing ${projectName}...`);
      const filesProcessed = await this.mergeProjectFiles(
        projectSourceDir,
        projectName
      );
      totalFilesProcessed += filesProcessed;
      console.log(`  ✅ ${projectName}: ${filesProcessed} files processed`);
    }

    // Process Downscaler into its own subdirectory (WinUI/Windows 10+ isolation)
    console.log(
      "\n🏠 Processing Downscaler (isolated due to WinUI dependencies)..."
    );
    const downscalerSourceDir = path.join(this.buildOutputsDir, "Downscaler");
    if (await fs.pathExists(downscalerSourceDir)) {
      const downscalerDestDir = path.join(this.distDir, "Downscaler");
      await fs.ensureDir(downscalerDestDir);

      // Reset DLL tracking for Downscaler's isolated space
      const originalHashes = new Map(this.dllHashes);
      this.dllHashes.clear();

      console.log(`\nProcessing Downscaler (isolated)...`);
      const downscalerFiles = await this.mergeProjectFilesToPath(
        downscalerSourceDir,
        downscalerDestDir,
        "Downscaler"
      );
      totalFilesProcessed += downscalerFiles;
      console.log(
        `  ✅ Downscaler: ${downscalerFiles} files processed (isolated)`
      );

      // Restore original hashes for final reporting
      this.dllHashes = originalHashes;
    } else {
      console.warn(`  ⚠️  Downscaler not found in build outputs`);
    }

    console.log(`\n🎉 Distribution completed with project isolation!`);
    console.log(`📦 Total files: ${totalFilesProcessed}`);
    console.log(`🔄 Utility DLLs deduplicated: ${this.duplicateCount}`);
    console.log(`🏠 Downscaler isolated to prevent dependency conflicts`);

    if (this.hashMismatches.length > 0) {
      console.error(
        `❌ Hash mismatches detected in utilities: ${this.hashMismatches.length}`
      );
      this.hashMismatches.forEach((error) => console.error(`  - ${error}`));
      console.log(
        `\n💡 Note: Downscaler is isolated in its own directory to avoid conflicts`
      );
      process.exit(1);
    }
  }

  async mergeProjectFiles(sourceDir, projectName) {
    let filesProcessed = 0;

    const processDirectory = async (srcDir, relativePath = "") => {
      const entries = await fs.readdir(srcDir, { withFileTypes: true });

      for (const entry of entries) {
        const srcPath = path.join(srcDir, entry.name);
        const relativeFilePath = path.join(relativePath, entry.name);
        const destPath = path.join(this.distDir, relativeFilePath);

        if (entry.isDirectory()) {
          // Preserve subdirectory structure
          await fs.ensureDir(destPath);
          await processDirectory(srcPath, relativeFilePath);
        } else {
          // Handle individual files
          const ext = path.extname(entry.name).toLowerCase();

          if (ext === ".dll") {
            // Handle DLL with deduplication
            const result = await this.handleDllFile(
              srcPath,
              destPath,
              relativeFilePath,
              projectName
            );
            if (result.error) {
              this.hashMismatches.push(
                `${entry.name} in ${projectName}: ${result.error}`
              );
              return;
            }
            if (result.skipped) {
              this.duplicateCount++;
            }
          } else {
            // Handle non-DLL files (EXEs, configs, etc.)
            if (await fs.pathExists(destPath)) {
              // If file already exists, check if it's identical or rename
              if (ext === ".exe") {
                // For executables, preserve both with project prefix if conflict
                const baseName = path.basename(entry.name, ext);
                const newName = `${projectName}_${entry.name}`;
                const newDestPath = path.join(
                  this.distDir,
                  relativeFilePath.replace(entry.name, newName)
                );
                await fs.copy(srcPath, newDestPath);
                console.log(
                  `    📝 Renamed EXE to avoid conflict: ${entry.name} → ${newName}`
                );
              } else {
                // For other files, skip if identical, otherwise overwrite
                await fs.copy(srcPath, destPath);
              }
            } else {
              // File doesn't exist, copy directly
              await fs.copy(srcPath, destPath);
            }
          }

          filesProcessed++;
        }
      }
    };

    await processDirectory(sourceDir);
    return filesProcessed;
  }

  async mergeProjectFilesToPath(sourceDir, destDir, projectName) {
    let filesProcessed = 0;

    const processDirectory = async (srcDir, relativePath = "") => {
      const entries = await fs.readdir(srcDir, { withFileTypes: true });

      for (const entry of entries) {
        const srcPath = path.join(srcDir, entry.name);
        const relativeFilePath = path.join(relativePath, entry.name);
        const destPath = path.join(destDir, relativeFilePath);

        if (entry.isDirectory()) {
          // Preserve subdirectory structure
          await fs.ensureDir(destPath);
          await processDirectory(srcPath, relativeFilePath);
        } else {
          // Handle individual files - copy directly without DLL conflict checking
          await fs.copy(srcPath, destPath);
          filesProcessed++;
        }
      }
    };

    await processDirectory(sourceDir);
    return filesProcessed;
  }

  async handleDllFile(sourcePath, destPath, relativeFilePath, projectName) {
    try {
      const hash = this.calculateFileHash(sourcePath);
      const stats = await fs.stat(sourcePath);

      // Enhanced diagnostics for Core.dll specifically
      const isDllCore =
        path.basename(relativeFilePath).toLowerCase() === "core.dll";

      if (isDllCore) {
        console.log(`\n🔍 DIAGNOSTIC: Core.dll from ${projectName}`);
        console.log(`  📂 Source Path: ${sourcePath}`);
        console.log(`  📏 Size: ${stats.size} bytes`);
        console.log(`  🔑 Hash: ${hash}`);
        console.log(`  📅 Modified: ${stats.mtime}`);

        // Try to get assembly metadata if possible
        try {
          const { execSync } = require("child_process");
          const powershellCmd = `"Add-Type -AssemblyName System.Reflection; [System.Reflection.Assembly]::LoadFile('${sourcePath.replace(
            /\\/g,
            "\\\\"
          )}').FullName"`;
          const assemblyInfo = execSync(
            `powershell -Command ${powershellCmd}`,
            { encoding: "utf8", timeout: 5000 }
          ).trim();
          console.log(`  🏷️  Assembly: ${assemblyInfo}`);
        } catch (metaError) {
          console.log(
            `  🏷️  Assembly: Could not read metadata - ${
              metaError.message.split("\n")[0]
            }`
          );
        }

        // Check for native dependencies
        try {
          const { execSync } = require("child_process");
          const dependenciesCmd = `dumpbin /dependents "${sourcePath}" 2>nul | findstr /i ".dll"`;
          const deps = execSync(dependenciesCmd, {
            encoding: "utf8",
            timeout: 3000
          }).trim();
          if (deps) {
            console.log(
              `  🔗 Dependencies: ${deps.split("\n").slice(0, 3).join(", ")}...`
            );
          }
        } catch (depError) {
          console.log(
            `  🔗 Dependencies: Could not analyze (${
              depError.code || "unknown error"
            })`
          );
        }
      }

      // Use the full relative path as the key to preserve directory structure
      // This ensures files with the same name in different directories are treated as separate files
      const dllKey = relativeFilePath;

      if (this.dllHashes.has(dllKey)) {
        const existing = this.dllHashes.get(dllKey);

        if (existing.hash === hash) {
          // Same DLL, skip duplication
          console.log(`    🔄 Skipped duplicate DLL: ${dllKey} (hash match)`);
          return { skipped: true };
        } else {
          // Hash mismatch - this is an error condition
          const error =
            `Hash mismatch for ${dllKey}!\n` +
            `  Existing: ${existing.hash} (${existing.size} bytes) from ${existing.project}\n` +
            `  New:      ${hash} (${stats.size} bytes) from ${projectName}`;
          console.error(`    ❌ ${error}`);

          // Enhanced diagnostics for Core.dll hash mismatches
          if (isDllCore) {
            console.error(`\n🚨 DETAILED CORE.DLL MISMATCH ANALYSIS:`);
            console.error(
              `  📊 Size Difference: ${stats.size - existing.size} bytes`
            );
            console.error(`  📁 Paths being compared:`);
            console.error(
              `    - Existing (${existing.project}): ${
                existing.sourcePath || "unknown"
              }`
            );
            console.error(`    - New (${projectName}): ${sourcePath}`);

            // Store source path for existing for future comparisons
            existing.sourcePath = existing.sourcePath || "unknown";
          }

          return { error };
        }
      } else {
        // First occurrence of this DLL
        await fs.copy(sourcePath, destPath);
        this.dllHashes.set(dllKey, {
          hash: hash,
          size: stats.size,
          project: projectName,
          sourcePath: sourcePath // Store source path for diagnostics
        });
        console.log(`    ✅ Added DLL: ${dllKey} (${hash.substring(0, 8)}...)`);
        return { processed: true };
      }
    } catch (error) {
      return {
        error: `Failed to process DLL ${relativeFilePath}: ${error.message}`
      };
    }
  }

  async createReadmeFiles() {
    console.log("\n📋 Creating documentation...");

    const mainReadme = `# Downscaler Merged Distribution

This is an automated build distribution containing all executables and libraries from the Downscaler repository merged into a single directory.

## Included Applications

### Main Applications
- **Downscaler.exe** - Main WinUI application for display scaling
- **GameLauncher.exe** - Console application for game launching and script execution
- **ScriptEditor.exe** - Electron-based script editor (Eclipse Theia)

### Utilities
- **DiagnosticWindow.exe** - Native diagnostic utility
- **IdentifyMonitorsUtil.exe** - Monitor identification utility
- **MonitorFadeUtil.exe** - Monitor fade control utility

### Build Tools
- **GameLauncherTaskGenerator.exe** - Code generation utility
- Other TypeScript and build tools

## Support Files

- **DLL files**: Shared libraries (deduplicated to prevent conflicts)
- **Assets/**: GameLauncher assets and resources
- **ExampleScripts/**: GameLauncher example scripts
- **Libs/**: TypeScript libraries and type definitions
- **TypeScript/**: TypeScript compiler libraries

## Build Information

Built on: ${new Date().toISOString()}
Platform: Windows x64
Configuration: Release

## Usage Notes

- All executables are in the same directory - no need to navigate subfolders
- .NET 8.0 runtime is required for managed applications
- All DLLs have been deduplicated to prevent conflicts
- Windows 10 version 1809 or later required

## Getting Started

1. Extract this ZIP to any directory
2. Run any of the .exe files directly
3. For GameLauncher, see the ExampleScripts/ directory for usage examples
`;

    await fs.writeFile(path.join(this.distDir, "README.md"), mainReadme);
    console.log("  ✅ Documentation created");
  }

  async createLauncherScript() {
    console.log("Creating launcher script...");

    const launcherScript = `@echo off
echo Downscaler Merged Distribution
echo ============================
echo.
echo Available Applications:
echo 1. Downscaler (Main Application)
echo 2. Game Launcher
echo 3. Script Editor
echo 4. Diagnostic Window
echo 5. Identify Monitors Utility
echo 6. Monitor Fade Utility
echo 0. Exit
echo.
set /p choice="Enter your choice (0-6): "

if "%choice%"=="1" (
    echo Starting Downscaler...
    start "Downscaler" "Downscaler.exe"
) else if "%choice%"=="2" (
    echo Starting Game Launcher...
    start "Game Launcher" "GameLauncher.exe"
) else if "%choice%"=="3" (
    echo Starting Script Editor...
    start "Script Editor" "ScriptEditor.exe"
) else if "%choice%"=="4" (
    echo Starting Diagnostic Window...
    start "Diagnostic Window" "DiagnosticWindow.exe"
) else if "%choice%"=="5" (
    echo Starting Identify Monitors...
    start "Identify Monitors" "IdentifyMonitorsUtil.exe"
) else if "%choice%"=="6" (
    echo Starting Monitor Fade...
    start "Monitor Fade" "MonitorFadeUtil.exe"
) else if "%choice%"=="0" (
    echo Goodbye!
    exit
) else (
    echo Invalid choice. Please run the script again.
)

pause
`;

    await fs.writeFile(path.join(this.distDir, "launcher.bat"), launcherScript);
    console.log("  ✅ Launcher script created");
  }

  async generateReport() {
    const reportPath = path.join(this.distDir, "build-report.json");

    const dllReport = Array.from(this.dllHashes.entries()).map(
      ([name, info]) => ({
        name: name,
        hash: info.hash,
        size: info.size,
        sourceProject: info.project
      })
    );

    const report = {
      timestamp: new Date().toISOString(),
      distributionType: "merged",
      summary: {
        totalDLLsProcessed: this.dllHashes.size,
        duplicatesSkipped: this.duplicateCount,
        hashMismatches: this.hashMismatches.length
      },
      dlls: dllReport,
      errors: this.hashMismatches
    };

    await fs.writeJson(reportPath, report, { spaces: 2 });
    console.log(`📊 Build report saved to: ${reportPath}`);

    return report;
  }

  async run() {
    try {
      await this.initialize();
      await this.mergeAllProjects();
      await this.createReadmeFiles();
      await this.createLauncherScript();
      await this.generateReport();

      console.log("\n🎉 Merged distribution creation completed successfully!");
      console.log(`📁 All outputs merged into: ${this.distDir}`);
    } catch (error) {
      console.error("❌ Distribution creation failed:", error);
      process.exit(1);
    }
  }
}

// Run the distribution builder
const builder = new DistributionBuilder();
builder.run();
