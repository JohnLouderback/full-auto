/**
 * @file
 * This script is used to conduct any necessary clean-up operations before the schema generation process.
 */
const fs = require('fs');
const path = require('path');

// Remove the generated schema files.
fs.unlinkSync(path.resolve(__dirname, '../downscaler.schema.json'));