const fs = require("fs-extra");
const path = require("path");
const { glob } = require("glob");

console.log("Starting build output collection...");

class BuildOutputCollector {
  constructor() {
    this.outputDir = path.join(__dirname, "..", "build-outputs");
  }

  async initialize() {
    // Clean and create output directory
    await fs.ensureDir(this.outputDir);
    await fs.emptyDir(this.outputDir);
    console.log(`Initialized output directory: ${this.outputDir}`);
  }

  async collectProjectOutputs() {
    const projectConfigs = [
      // C# Console/WinForms Applications
      {
        name: "GameLauncher",
        type: "console",
        binPath: "GameLauncher/bin/Release/net8.0-windows",
        patterns: [
          "**/*.exe",
          "**/*.dll",
          "**/*.pdb",
          "**/*.config",
          "**/Assets/**/*",
          "**/ExampleScripts/**/*",
          "**/Libs/**/*",
          "**/TypeScript/**/*"
        ]
      },
      {
        name: "GenericModLauncher",
        type: "winforms",
        binPath: "GenericModLauncher/bin/Release/net8.0-windows",
        patterns: [
          "**/*.exe",
          "**/*.dll",
          "**/*.pdb",
          "**/*.config",
          "**/Assets/**/*"
        ]
      },
      {
        name: "IdentifyMonitorsUtil",
        type: "utility",
        binPath: "IdentifyMonitorsUtil/bin/Release/net8.0-windows",
        patterns: ["**/*.exe", "**/*.dll", "**/*.pdb", "**/*.config"]
      },
      {
        name: "MonitorFadeUtil",
        type: "utility",
        binPath: "MonitorFadeUtil/bin/Release/net8.0-windows",
        patterns: ["**/*.exe", "**/*.dll", "**/*.pdb", "**/*.config"]
      },
      {
        name: "GameLauncherTaskGenerator",
        type: "build-tool",
        binPath: "GameLauncherTaskGenerator/bin/Release/net8.0",
        patterns: ["**/*.exe", "**/*.dll", "**/*.pdb", "**/*.config"]
      },
      // WinUI Application
      {
        name: "Downscaler",
        type: "winui",
        binPath:
          "Downscaler/bin/x64/Release/net8.0-windows10.0.22621.0/win10-x64",
        patterns: [
          "**/*.exe",
          "**/*.dll",
          "**/*.pdb",
          "**/*.config",
          "**/*.winmd",
          "**/Assets/**/*"
        ]
      },
      // C++ Applications
      {
        name: "DiagnosticWindow",
        type: "native",
        binPath: "DiagnosticWindow/x64/Release",
        patterns: ["**/*.exe", "**/*.dll", "**/*.pdb"]
      },
      {
        name: "Downscaler.Cpp.Core",
        type: "native-lib",
        binPath: "Downscaler.Cpp.Core/x64/Release",
        patterns: ["**/*.dll", "**/*.lib", "**/*.pdb", "**/*.winmd"]
      },
      {
        name: "Downscaler.Cpp.WinRT",
        type: "native-lib",
        binPath: "Downscaler.Cpp.WinRT/x64/Release",
        patterns: ["**/*.dll", "**/*.lib", "**/*.pdb", "**/*.winmd"]
      },
      {
        name: "Cpp.Core",
        type: "native-lib",
        binPath: "Cpp.Core/x64/Release",
        patterns: ["**/*.dll", "**/*.lib", "**/*.pdb"]
      },
      // JavaScript/TypeScript Projects
      {
        name: "ScriptEditor",
        type: "electron",
        binPath: "ScriptEditor/electron-app/dist",
        patterns: ["**/*"],
        fallbackPath: "ScriptEditor/lib"
      },
      {
        name: "YamlSchemaTypes",
        type: "types",
        binPath: "YamlSchemaTypes/lib",
        patterns: ["**/*.js", "**/*.d.ts", "**/*.json"],
        fallbackPath: "YamlSchemaTypes/src"
      },
      {
        name: "TypeScriptCompiler",
        type: "compiler",
        binPath: "TypeScriptCompiler/lib",
        patterns: ["**/*"],
        fallbackPath: "TypeScriptCompiler/node_modules"
      }
    ];

    for (const config of projectConfigs) {
      await this.collectProjectFiles(config);
    }
  }

  async collectProjectFiles(config) {
    console.log(`\nCollecting outputs for ${config.name} (${config.type})`);

    const fullBinPath = path.join(__dirname, "..", config.binPath);
    let searchPath = fullBinPath;

    // Check if primary path exists, fallback if needed
    if (!(await fs.pathExists(fullBinPath)) && config.fallbackPath) {
      searchPath = path.join(__dirname, "..", config.fallbackPath);
      console.log(`  Primary path not found, using fallback: ${searchPath}`);
    }

    if (!(await fs.pathExists(searchPath))) {
      console.warn(
        `  ⚠️  Build output not found for ${config.name} at ${searchPath}`
      );
      return;
    }

    const outputSubDir = path.join(this.outputDir, config.name);
    await fs.ensureDir(outputSubDir);

    let filesProcessed = 0;

    for (const pattern of config.patterns) {
      const files = await glob(pattern, {
        cwd: searchPath,
        nodir: true,
        absolute: false
      });

      for (const file of files) {
        const sourceFile = path.join(searchPath, file);
        const destFile = path.join(outputSubDir, file);

        await fs.ensureDir(path.dirname(destFile));

        // Copy all files directly (no deduplication at this stage)
        await fs.copy(sourceFile, destFile);

        filesProcessed++;
      }
    }

    console.log(`  ✅ Processed ${filesProcessed} files`);
  }
  async generateReport() {
    const reportPath = path.join(this.outputDir, "collection-report.json");

    const report = {
      timestamp: new Date().toISOString(),
      summary: {
        message:
          "Build outputs collected successfully - no deduplication performed at this stage"
      }
    };

    await fs.writeJson(reportPath, report, { spaces: 2 });
    console.log(`\n📊 Collection report saved to: ${reportPath}`);

    return report;
  }

  async run() {
    try {
      await this.initialize();
      await this.collectProjectOutputs();
      const report = await this.generateReport();

      console.log("\n🎉 Build output collection completed successfully!");
      console.log("   📦 Files collected to build-outputs/ directory");
      console.log(
        "   🔄 Deduplication will be performed by create-distribution.js"
      );
    } catch (error) {
      console.error("❌ Build output collection failed:", error);
      process.exit(1);
    }
  }
}

// Run the collector
const collector = new BuildOutputCollector();
collector.run();
