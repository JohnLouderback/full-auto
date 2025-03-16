/**
 * This script downloads the Supermium-Electron binaries and replaces the Electron
 * binaries in the node_modules folder. This allows us to support Windows 7 and GDI
 * rendering. The script downloads the binaries from the Supermium-Electron GitHub
 * releases page and extracts them to the node_modules/electron/dist folder.
 */

const fs = require("fs");
const path = require("path");
const https = require("https");
const crypto = require("crypto");
const AdmZip = require("adm-zip");

const ELECTRON_DIST_PATH = path.resolve(
  __dirname,
  "../node_modules/electron/dist"
);
const ELECTRON_EXE_PATH = path.join(ELECTRON_DIST_PATH, "electron.exe");
const MARKER_FILE = path.join(ELECTRON_DIST_PATH, "supermium-marker.txt");
const DOWNLOAD_URL =
  "https://github.com/win32ss/supermium-electron/releases/download/v28-testing/dist_x64.zip";
// const DOWNLOAD_URL =
//   "https://github.com/KenCorma/supermium-electron/releases/download/v30.5.1-0/electron-v30.5.1-win32-x64.zip";
const TEMP_ZIP_PATH = path.resolve(__dirname, "dist_x64.zip");
// const TEMP_ZIP_PATH = path.resolve(__dirname, "electron-v30.5.1-win32-x64.zip");

// Function to compute SHA-256 hash of a file
function getFileHash(filePath) {
  return new Promise((resolve, reject) => {
    if (!fs.existsSync(filePath)) {
      return resolve(null);
    }

    const hash = crypto.createHash("sha256");
    const stream = fs.createReadStream(filePath);

    stream.on("data", (data) => hash.update(data));
    stream.on("end", () => resolve(hash.digest("hex")));
    stream.on("error", (err) => reject(err));
  });
}

// Function to read stored hash from marker file
function readStoredHash() {
  if (!fs.existsSync(MARKER_FILE)) return null;
  return fs.readFileSync(MARKER_FILE, "utf8").trim();
}

// Function to download a file
function downloadFile(url, destination, callback) {
  console.log(`Downloading: ${url}`);

  const file = fs.createWriteStream(destination);

  function handleRequest(url) {
    https
      .get(url, (response) => {
        if (
          response.statusCode >= 300 &&
          response.statusCode < 400 &&
          response.headers.location
        ) {
          // Handle redirect
          const newUrl = new URL(response.headers.location, url).toString();
          console.log(`Redirecting to: ${newUrl}`);
          handleRequest(newUrl);
          return;
        } else if (response.statusCode !== 200) {
          console.error(`Failed to download file: ${response.statusCode}`);
          return;
        }

        response.pipe(file);
        file.on("finish", () => file.close(callback));
      })
      .on("error", (err) => {
        console.error(`Error downloading file: ${err.message}`);
      });
  }

  handleRequest(url);
}

// Function to extract ZIP file
function extractZip(zipPath, destPath) {
  try {
    if (!fs.existsSync(destPath)) {
      fs.mkdirSync(destPath, { recursive: true });
    }

    const zip = new AdmZip(zipPath);
    zip.extractAllTo(destPath, true);

    console.log("Supermium-Electron binaries extracted successfully.");
  } catch (err) {
    console.error(`Error extracting ZIP file: ${err.message}`);
  }
}

// Main Logic
(async () => {
  const storedHash = readStoredHash();
  const currentHash = await getFileHash(ELECTRON_EXE_PATH);

  if (storedHash && storedHash === currentHash) {
    console.log("Supermium-Electron is already up to date. Skipping download.");
    return;
  }

  console.log(
    `Supermium-Electron update required (Stored: ${
      storedHash || "None"
    }, Current: ${currentHash || "Missing"})`
  );

  downloadFile(DOWNLOAD_URL, TEMP_ZIP_PATH, async () => {
    console.log("Download complete. Extracting...");
    extractZip(TEMP_ZIP_PATH, ELECTRON_DIST_PATH);
    fs.unlinkSync(TEMP_ZIP_PATH); // Clean up zip file after extraction

    // Compute and store the new hash
    const newHash = await getFileHash(ELECTRON_EXE_PATH);
    if (newHash) {
      fs.writeFileSync(MARKER_FILE, newHash);
      console.log(`Supermium-Electron updated. Hash stored: ${newHash}`);
    } else {
      console.error("Failed to compute new hash after extraction.");
    }
  });
})();
