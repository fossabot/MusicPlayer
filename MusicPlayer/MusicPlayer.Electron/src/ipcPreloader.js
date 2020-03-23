const electron = require('electron');
const {ipcRenderer, remote} = electron;

window.ipcRenderer = ipcRenderer;