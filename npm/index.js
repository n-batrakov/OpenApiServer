var binwrap = require("binwrap");
var path = require("path");

var packageInfo = require(path.join(__dirname, "package.json"));
var version = packageInfo.version;

var root = 'https://github.com/n-batrakov/OpenApiServer/releases/download/' + version + '/oas-' + version + '-';

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