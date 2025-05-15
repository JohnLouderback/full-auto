const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Compile TypeScript
console.log('Compiling TypeScript...');
execSync('tsc', { stdio: 'inherit' });

// Define source and destination paths
const srcDir = path.join(__dirname, 'src/style');
const destDir = path.join(__dirname, 'lib/style');

// Ensure the destination directory exists
if (!fs.existsSync(destDir)) {
    fs.mkdirSync(destDir, { recursive: true });
}

// Copy CSS files
fs.readdirSync(srcDir).forEach(file => {
    const srcFile = path.join(srcDir, file);
    const destFile = path.join(destDir, file);
    fs.copyFileSync(srcFile, destFile);
    console.log(`Copied ${file} to ${destDir}`);
});

console.log('Build complete.');
