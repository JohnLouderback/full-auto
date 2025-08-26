const fs = require('fs-extra');
const path = require('path');
const crypto = require('crypto');
const { glob } = require('glob');

console.log('Starting build output collection with DLL deduplication...');

class BuildOutputCollector {
  constructor() {
    this.dllHashes = new Map(); // Map of DLL filename -> { hash, path, size }
    this.duplicateCount = 0;
    this.hashMismatches = [];
    this.outputDir = path.join(__dirname, '..', 'build-outputs');
  }

  async initialize() {
    // Clean and create output directory
    await fs.ensureDir(this.outputDir);
    await fs.emptyDir(this.outputDir);
    console.log(`Initialized output directory: ${this.outputDir}`);
  }

  calculateFileHash(filePath) {
    const fileBuffer = fs.readFileSync(filePath);
    return crypto.createHash('sha256').update(fileBuffer).digest('hex');
  }

  async collectProjectOutputs() {
    const projectConfigs = [
      // C# Console/WinForms Applications
      {
        name: 'GameLauncher',
        type: 'console',
        binPath: 'GameLauncher/bin/Release/net8.0-windows',
        patterns: ['**/*.exe', '**/*.dll', '**/*.pdb', '**/*.config', '**/Assets/**/*', '**/ExampleScripts/**/*', '**/Libs/**/*', '**/TypeScript/**/*']
      },
      {
        name: 'GenericModLauncher',
        type: 'winforms',
        binPath: 'GenericModLauncher/bin/Release/net8.0-windows',
        patterns: ['**/*.exe', '**/*.dll', '**/*.pdb', '**/*.config', '**/Assets/**/*']
      },
      {
        name: 'IdentifyMonitorsUtil',
        type: 'utility',
        binPath: 'IdentifyMonitorsUtil/bin/Release/net8.0-windows',
        patterns: ['**/*.exe', '**/*.dll', '**/*.pdb', '**/*.config']
      },
      {
        name: 'MonitorFadeUtil',
        type: 'utility', 
        binPath: 'MonitorFadeUtil/bin/Release/net8.0-windows',
        patterns: ['**/*.exe', '**/*.dll', '**/*.pdb', '**/*.config']
      },
      {
        name: 'GameLauncherTaskGenerator',
        type: 'build-tool',
        binPath: 'GameLauncherTaskGenerator/bin/Release/net8.0',
        patterns: ['**/*.exe', '**/*.dll', '**/*.pdb', '**/*.config']
      },
      // WinUI Application
      {
        name: 'Downscaler',
        type: 'winui',
        binPath: 'Downscaler/bin/x64/Release/net8.0-windows10.0.22621.0/win10-x64',
        patterns: ['**/*.exe', '**/*.dll', '**/*.pdb', '**/*.config', '**/*.winmd', '**/Assets/**/*']
      },
      // C++ Applications
      {
        name: 'DiagnosticWindow',
        type: 'native',
        binPath: 'DiagnosticWindow/x64/Release',
        patterns: ['**/*.exe', '**/*.dll', '**/*.pdb']
      },
      {
        name: 'Downscaler.Cpp.Core',
        type: 'native-lib',
        binPath: 'Downscaler.Cpp.Core/x64/Release',
        patterns: ['**/*.dll', '**/*.lib', '**/*.pdb', '**/*.winmd']
      },
      {
        name: 'Downscaler.Cpp.WinRT',
        type: 'native-lib',
        binPath: 'Downscaler.Cpp.WinRT/x64/Release',
        patterns: ['**/*.dll', '**/*.lib', '**/*.pdb', '**/*.winmd']
      },
      {
        name: 'Cpp.Core',
        type: 'native-lib',
        binPath: 'Cpp.Core/x64/Release',
        patterns: ['**/*.dll', '**/*.lib', '**/*.pdb']
      },
      // JavaScript/TypeScript Projects
      {
        name: 'ScriptEditor',
        type: 'electron',
        binPath: 'ScriptEditor/electron-app/dist',
        patterns: ['**/*'],
        fallbackPath: 'ScriptEditor/lib'
      },
      {
        name: 'YamlSchemaTypes',
        type: 'types',
        binPath: 'YamlSchemaTypes/lib',
        patterns: ['**/*.js', '**/*.d.ts', '**/*.json'],
        fallbackPath: 'YamlSchemaTypes/src'
      },
      {
        name: 'TypeScriptCompiler',
        type: 'compiler',
        binPath: 'TypeScriptCompiler/lib',
        patterns: ['**/*'],
        fallbackPath: 'TypeScriptCompiler/node_modules'
      }
    ];

    for (const config of projectConfigs) {
      await this.collectProjectFiles(config);
    }
  }

  async collectProjectFiles(config) {
    console.log(`\nCollecting outputs for ${config.name} (${config.type})`);
    
    const fullBinPath = path.join(__dirname, '..', config.binPath);
    let searchPath = fullBinPath;
    
    // Check if primary path exists, fallback if needed
    if (!await fs.pathExists(fullBinPath) && config.fallbackPath) {
      searchPath = path.join(__dirname, '..', config.fallbackPath);
      console.log(`  Primary path not found, using fallback: ${searchPath}`);
    }
    
    if (!await fs.pathExists(searchPath)) {
      console.warn(`  ⚠️  Build output not found for ${config.name} at ${searchPath}`);
      return;
    }

    const outputSubDir = path.join(this.outputDir, config.name);
    await fs.ensureDir(outputSubDir);

    let filesProcessed = 0;
    let dllsSkipped = 0;

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

        // Special handling for DLL files
        if (path.extname(file).toLowerCase() === '.dll') {
          const handled = await this.handleDllFile(sourceFile, destFile, file);
          if (handled.skipped) {
            dllsSkipped++;
            continue;
          }
          if (handled.error) {
            this.hashMismatches.push({
              file: file,
              project: config.name,
              error: handled.error
            });
            process.exit(1);
          }
        } else {
          await fs.copy(sourceFile, destFile);
        }
        
        filesProcessed++;
      }
    }

    console.log(`  ✅ Processed ${filesProcessed} files (${dllsSkipped} DLLs deduplicated)`);
  }

  async handleDllFile(sourcePath, destPath, fileName) {
    try {
      const hash = this.calculateFileHash(sourcePath);
      const stats = await fs.stat(sourcePath);
      const dllName = path.basename(fileName);
      
      if (this.dllHashes.has(dllName)) {
        const existing = this.dllHashes.get(dllName);
        
        if (existing.hash === hash) {
          // Same DLL, skip duplication
          this.duplicateCount++;
          console.log(`    🔄 Skipped duplicate DLL: ${dllName} (hash match)`);
          return { skipped: true };
        } else {
          // Hash mismatch - this is an error condition
          const error = `Hash mismatch for ${dllName}!\n` +
                       `  Existing: ${existing.hash} (${existing.size} bytes) at ${existing.path}\n` +
                       `  New:      ${hash} (${stats.size} bytes) at ${sourcePath}`;
          console.error(`    ❌ ${error}`);
          return { error };
        }
      } else {
        // First occurrence of this DLL
        await fs.copy(sourcePath, destPath);
        this.dllHashes.set(dllName, {
          hash: hash,
          path: destPath,
          size: stats.size
        });
        console.log(`    ✅ Added DLL: ${dllName} (${hash.substring(0, 8)}...)`);
        return { processed: true };
      }
    } catch (error) {
      return { error: `Failed to process DLL ${fileName}: ${error.message}` };
    }
  }

  async generateReport() {
    const reportPath = path.join(this.outputDir, 'build-report.json');
    
    const dllReport = Array.from(this.dllHashes.entries()).map(([name, info]) => ({
      name: name,
      hash: info.hash,
      size: info.size,
      path: path.relative(this.outputDir, info.path)
    }));

    const report = {
      timestamp: new Date().toISOString(),
      summary: {
        totalDLLsProcessed: this.dllHashes.size,
        duplicatesSkipped: this.duplicateCount,
        hashMismatches: this.hashMismatches.length
      },
      dlls: dllReport,
      errors: this.hashMismatches
    };

    await fs.writeJson(reportPath, report, { spaces: 2 });
    console.log(`\n📊 Build report saved to: ${reportPath}`);
    
    return report;
  }

  async run() {
    try {
      await this.initialize();
      await this.collectProjectOutputs();
      const report = await this.generateReport();
      
      console.log('\n🎉 Build output collection completed successfully!');
      console.log(`   📦 Total DLLs: ${report.summary.totalDLLsProcessed}`);
      console.log(`   🔄 Duplicates skipped: ${report.summary.duplicatesSkipped}`);
      
      if (report.summary.hashMismatches > 0) {
        console.error(`   ❌ Hash mismatches: ${report.summary.hashMismatches}`);
        process.exit(1);
      }
      
    } catch (error) {
      console.error('❌ Build output collection failed:', error);
      process.exit(1);
    }
  }
}

// Run the collector
const collector = new BuildOutputCollector();
collector.run();