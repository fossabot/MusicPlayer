const path = require('path');
const electron = require('electron');
const MainWindow = require('./app/main_window');
const os = require('./app/operating_system');
const c = require('./app/constants');

const { app, ipcMain } = electron;

let mainWindow;
let offlineWindow;
let appIconPath;

app.on('ready', () => {

  let appIcon = 'app-win.ico';
  if (os.isLinux()) appIcon = 'app-linux512x512.png';
  if (os.isMacOS()) appIcon = 'app-mac.png';

  appIconPath = path.join(__dirname, 'src', 'assets', appIcon);

  loadAppWindows();
});

function loadAppWindows() {
  let appPath = c.settings.appUrl;

  mainWindow = new MainWindow(appPath, appIconPath);
  mainWindow.setMenu(null);

  mainWindow.on('closed', () => app.quit());

  /* DEBUG: force show offline window */
  //offlineWindow = new MainWindow(`file://${__dirname}/src/offline.html`, appIconPath);

  // show offline-page if no connectivity
  mainWindow.webContents.on('did-fail-load', function(ev, errorCode, errorDesc, url) {
    offlineWindow = new MainWindow(`file://${__dirname}/src/offline.html`, appIconPath, true, false);
    offlineWindow.setResizable(false);
    
    offlineWindow.webContents.on('console-message', function(e, lvl, message) {
      if (message == "WindowButtons:Register") offlineWindow.webContents.send('WindowButtons:Register');
    });

    mainWindow.hide();
  });

  // wait until window buttons are loaded
  mainWindow.webContents.on('console-message', function(e, lvl, message) {
    if (message == "WindowButtons:Register") mainWindow.webContents.send('WindowButtons:Register');
  });
}

ipcMain.on('app:refresh', () => {
  // hide offline window if applicable
  if (offlineWindow && offlineWindow.isVisible()) offlineWindow.hide();
  
  offlineWindow = null;

  if (mainWindow) {
    // mainWindow is hidden, refresh and show it directly
    mainWindow.loadHome();
    mainWindow.show();
  } else {
    // instantiate mainWindow additionally
    loadAppWindows();
  }
});

// register window buttons click event
ipcMain.on('app:minimize', () => (offlineWindow == null ? mainWindow : offlineWindow).minimize());
ipcMain.on('app:min-max', () => {
  var window = (offlineWindow == null ? mainWindow : offlineWindow);
  if(window.isMaximized()) window.unmaximize();
  else window.maximize();
});
ipcMain.on('app:quit', () => app.quit());