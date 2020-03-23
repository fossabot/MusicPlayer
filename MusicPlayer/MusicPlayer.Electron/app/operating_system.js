const Helper = {
  isMacOS: function() {
    return process.platform === 'darwin';
  },
  isWindows: function() {
    return process.platform === 'win32';
  },
  isLinux: function() {
    return process.platform !== 'win32' && process.platform !== 'darwin';
  },
};

module.exports = Helper;
