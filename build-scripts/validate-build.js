const fs = require("fs-extra");
const path = require("path");

console.log("Validating merged build output...");

class BuildValidator {
  constructor() {
    this.distDir = path.join(__dirname, "..", "dist");
    this.buildOutputsDir = path.join(__dirname, "..", "build-outputs");
    this.errors = [];
    this.warnings = [];
  }

  async validateMergedStructure() {
    console.log("\n🔍 Validating merged distribution structure...");

    // Check if dist directory exists
    if (!(await fs.pathExists(this.distDir))) {
      this.errors.push("Distribution directory does not exist");
      return;
    }

    // Check for expected executables
    const expectedExecutables = [
      "Downscaler.exe",
      "GameLauncher.exe",
      "IdentifyMonitorsUtil.exe",
      "MonitorFadeUtil.exe",
      "ScriptEditor.exe"
    ];

    for (const exeName of expectedExecutables) {
      const exePath = path.join(this.distDir, exeName);
      if (await fs.pathExists(exePath)) {
        const stats = await fs.stat(exePath);
        console.log(
          `  ✅ Found ${exeName} (${Math.round(stats.size / 1024)}KB)`
        );
      } else {
        this.warnings.push(`Expected executable not found: ${exeName}`);
      }
    }

    // Check for expected subdirectories that should be preserved
    const expectedSubdirs = [
      "Assets", // GameLauncher assets
      "ExampleScripts", // GameLauncher examples
      "Libs", // GameLauncher TypeScript libraries
      "TypeScript" // TypeScript compiler libraries
    ];

    for (const subdirName of expectedSubdirs) {
      const subdirPath = path.join(this.distDir, subdirName);
      if (await fs.pathExists(subdirPath)) {
        const files = await fs.readdir(subdirPath);
        console.log(`  ✅ Found ${subdirName}/ with ${files.length} items`);
      } else {
        this.warnings.push(`Expected subdirectory not found: ${subdirName}/`);
      }
    }

    // Check for DLL files
    const allFiles = await fs.readdir(this.distDir);
    const dllFiles = allFiles.filter((f) => f.endsWith(".dll"));
    console.log(`  ✅ Found ${dllFiles.length} DLL files in root directory`);

    if (dllFiles.length === 0) {
      this.warnings.push("No DLL files found in distribution");
    }

    // Check for documentation
    const mainReadme = path.join(this.distDir, "README.md");
    if (await fs.pathExists(mainReadme)) {
      console.log("  ✅ Found README.md");
    } else {
      this.warnings.push("README.md not found");
    }

    // Check for launcher script
    const launcher = path.join(this.distDir, "launcher.bat");
    if (await fs.pathExists(launcher)) {
      console.log("  ✅ Found launcher.bat");
    } else {
      this.warnings.push("launcher.bat not found");
    }
  }

  async validateBuildReport() {
    console.log("\n📊 Validating build report...");

    const reportPath = path.join(this.distDir, "build-report.json");
    if (!(await fs.pathExists(reportPath))) {
      this.errors.push("Build report not found");
      return;
    }

    try {
      const report = await fs.readJson(reportPath);

      console.log(`  ✅ Report generated at: ${report.timestamp}`);
      console.log(
        `  📦 Total DLLs processed: ${report.summary.totalDLLsProcessed}`
      );
      console.log(
        `  🔄 Duplicates skipped: ${report.summary.duplicatesSkipped}`
      );

      if (report.summary.hashMismatches > 0) {
        this.errors.push(
          `Build has ${report.summary.hashMismatches} hash mismatches`
        );
        report.errors.forEach((error) => {
          this.errors.push(`Hash mismatch: ${error}`);
        });
      } else {
        console.log("  ✅ No hash mismatches detected");
      }

      // Validate DLL information
      if (report.dlls && report.dlls.length > 0) {
        console.log(`  ✅ DLL manifest contains ${report.dlls.length} entries`);

        // Check for common expected DLLs
        const expectedDlls = ["System.", "Microsoft."];
        const foundExpected = report.dlls.filter((dll) =>
          expectedDlls.some((expected) => dll.name.includes(expected))
        ).length;

        if (foundExpected > 0) {
          console.log(`  ✅ Found ${foundExpected} expected system DLLs`);
        }

        // Verify distribution type
        if (report.distributionType === "merged") {
          console.log("  ✅ Confirmed merged distribution type");
        } else {
          this.warnings.push(
            "Distribution type not marked as merged in report"
          );
        }
      }
    } catch (error) {
      this.errors.push(`Failed to parse build report: ${error.message}`);
    }
  }

  async validateMergedIntegrity() {
    console.log("\n🔒 Validating merged distribution integrity...");

    try {
      // Check for conflicting executable names
      const allFiles = await fs.readdir(this.distDir);
      const exeFiles = allFiles.filter((f) => f.endsWith(".exe"));

      // Look for renamed executables (indicating conflicts)
      const renamedExes = exeFiles.filter((exe) => exe.includes("_"));
      if (renamedExes.length > 0) {
        console.log(
          `  ⚠️  Found ${renamedExes.length} renamed executables due to conflicts:`
        );
        renamedExes.forEach((exe) => console.log(`    - ${exe}`));
      }

      // Validate that we have a reasonable number of files
      const totalFiles = await this.countAllFiles(this.distDir);
      console.log(`  ✅ Total files in distribution: ${totalFiles}`);

      if (totalFiles < 10) {
        this.warnings.push("Distribution contains fewer files than expected");
      }

      // Check for empty subdirectories
      await this.checkForEmptyDirectories(this.distDir, "");
    } catch (error) {
      this.errors.push(
        `Failed to validate distribution integrity: ${error.message}`
      );
    }
  }

  async countAllFiles(dir) {
    let count = 0;
    const entries = await fs.readdir(dir, { withFileTypes: true });

    for (const entry of entries) {
      if (entry.isDirectory()) {
        const subDir = path.join(dir, entry.name);
        count += await this.countAllFiles(subDir);
      } else {
        count++;
      }
    }

    return count;
  }

  async checkForEmptyDirectories(dir, relativePath) {
    const entries = await fs.readdir(dir, { withFileTypes: true });

    if (entries.length === 0) {
      this.warnings.push(`Empty directory found: ${relativePath || "root"}`);
      return;
    }

    for (const entry of entries) {
      if (entry.isDirectory()) {
        const subDir = path.join(dir, entry.name);
        const subRelative = path.join(relativePath, entry.name);
        await this.checkForEmptyDirectories(subDir, subRelative);
      }
    }
  }

  async validateCriticalFiles() {
    console.log("\n🎯 Validating critical files...");

    // Check for configuration files that should exist
    const configFiles = [
      "tsconfig.json" // From GameLauncher
    ];

    for (const configFile of configFiles) {
      const configPath = path.join(this.distDir, configFile);
      if (await fs.pathExists(configPath)) {
        console.log(`  ✅ Found configuration: ${configFile}`);
      } else {
        this.warnings.push(`Configuration file missing: ${configFile}`);
      }
    }

    // Validate that required DLLs exist for applications
    const criticalDlls = [
      // Common .NET DLLs that should be present
      { pattern: /System\./i, description: ".NET System libraries" },
      { pattern: /Microsoft\./i, description: "Microsoft libraries" }
    ];

    const allFiles = await fs.readdir(this.distDir);
    const dllFiles = allFiles.filter((f) => f.endsWith(".dll"));

    for (const critical of criticalDlls) {
      const found = dllFiles.filter((dll) => critical.pattern.test(dll));
      if (found.length > 0) {
        console.log(`  ✅ Found ${found.length} ${critical.description}`);
      } else {
        this.warnings.push(`No ${critical.description} found`);
      }
    }
  }

  async generateValidationReport() {
    const report = {
      timestamp: new Date().toISOString(),
      distributionType: "merged",
      validation: {
        passed: this.errors.length === 0,
        errors: this.errors.length,
        warnings: this.warnings.length
      },
      errors: this.errors,
      warnings: this.warnings,
      summary: {
        distributionExists: await fs.pathExists(this.distDir),
        buildOutputsExist: await fs.pathExists(this.buildOutputsDir),
        isMergedStructure: true
      }
    };

    const reportPath = path.join(this.distDir, "validation-report.json");
    await fs.writeJson(reportPath, report, { spaces: 2 });
    console.log(`\n📋 Validation report saved to: ${reportPath}`);

    return report;
  }

  async run() {
    try {
      console.log("🔍 Starting merged build validation...\n");

      await this.validateMergedStructure();
      await this.validateBuildReport();
      await this.validateMergedIntegrity();
      await this.validateCriticalFiles();

      const report = await this.generateValidationReport();

      console.log("\n" + "=".repeat(50));
      console.log("📋 VALIDATION SUMMARY");
      console.log("=".repeat(50));

      if (report.validation.passed) {
        console.log("🎉 ✅ MERGED BUILD VALIDATION PASSED!");
        console.log(`   Warnings: ${report.validation.warnings}`);
      } else {
        console.log("❌ MERGED BUILD VALIDATION FAILED!");
        console.log(`   Errors: ${report.validation.errors}`);
        console.log(`   Warnings: ${report.validation.warnings}`);

        if (this.errors.length > 0) {
          console.log("\n🚨 ERRORS:");
          this.errors.forEach((error, i) =>
            console.log(`   ${i + 1}. ${error}`)
          );
        }
      }

      if (this.warnings.length > 0) {
        console.log("\n⚠️  WARNINGS:");
        this.warnings.forEach((warning, i) =>
          console.log(`   ${i + 1}. ${warning}`)
        );
      }

      console.log(
        "\n📦 Distribution Type: MERGED (all outputs in single directory)"
      );
      console.log("=".repeat(50));

      process.exit(report.validation.passed ? 0 : 1);
    } catch (error) {
      console.error("❌ Validation failed:", error);
      process.exit(1);
    }
  }
}

// Run the validator
const validator = new BuildValidator();
validator.run();
