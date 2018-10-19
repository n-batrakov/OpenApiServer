const binwrap = require("binwrap");
const path = require("path");

const packageInfo = require(path.join(__dirname, "package.json"));
//const version = packageInfo.version;

            //https://github.com/n-batrakov/OpenApiServer/releases/download/0.2.1/oas-0.2.1-linux-x64.tar.gz
const root = `https://github.com/n-batrakov/OpenApiServer/releases/download/0.2.1/oas-0.2.1-`;

module.exports = binwrap({
  dirname: __dirname,
  binaries: [
    "oas"
  ],
  urls: {
    "darwin-x64": root + "osx-x64.tar.gz",
    "linux-x64": root + "linux-x64.tar.gz",
    "win32-x64": root + "win-x64.zip"
  }
});