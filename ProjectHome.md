iPhile mounts your iPhone's fileystem into Windows Explorer.

# Important information #

**iPhile has development quality only. Be very careful when using it because it might harm your device running iOS. Although I use iPhile to read and write data from and to my iPhone, I can and will take no warranty for this tool's functions. _USE THIS TOOL AT YOUR OWN RISK ONLY._**

# Usage #

Oh, of course you need to install [Dokan](http://dokan-dev.net/en) version 0.5.3 or higher first to use iPhile.

You just run iPhile.exe and it should work. In its current releases it logs events and errors into the file log.txt.
When you run iPhile.exe, you see some kind of disclaimer (you can skip it by -skipinfo argument). After displaying this message, iPhile will sit in your tray and listen for iDevice connect/disconnect events.
If any device is found, it is mounted into Windows Explorer using a free drive letter.
When a device gets disconnected, it is dismounted. (If iPhile should crash, a device might still be in Windows Explorer and it might block a drive letter. Use DriveUnmounter.exe to unmount it then.)
If your device is jailbroken (afc2 service installed), two connections to it will be opened.
  * One to afc2 service. This is mapped into Windows Explorer.
  * One to afc service which is availabe even if the device is not jailbroken. This is not mapped into Windows Explorer, but instead, it will be used to determine the correct space values (free bytes, total bytes) of your iDevice instead of just showing the root partition's space.
If your device is not jailbroken (or has the afc2 service not installed), only a connection to afc service will be set up giving you only access to the device's Media partition (and its space values).
Isn't that simple?