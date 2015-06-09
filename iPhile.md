# Important information #

**iPhile has development quality only. Be very careful when using it because it might harm your device running iOS. Although I use iPhile to read and write data from and to my iPhone, I can and will take no warranty for this tool's functions. _USE THIS TOOL AT YOUR OWN RISK ONLY._**

# About iPhile #

iPhile is a simple utility designed to mount the filesystem of devices running Apple's iOS into Windows Explorer.

I started this project because I was really bored. I was inspired by some projects that offered similar features, both closed and open source ones.
But nothing of these projects fit my needs or worked properly.
So I started iPhile using C#. I found Dokan and its .NET binding to implement a filesystem for Windows. I also wanted to use [Manzana.dll](http://code.google.com/p/manzana), but it did not work properly on Windows 7 with iTunes 10 installed. Trying not to reinvent the wheel in that case, I googled and found [iPhoneFS](http://code.google.com/p/iphonefs/) here on Googlecode. It also had the same aim as my tool but did not fit my needs, either. (And it didn't work on my computer, I don't know why.)
But iPhoneFS relied on a GPL'ed version of Manzana.dll which had patches to work with iTunes 10. So I took this version to continue my project.
I implemented the Dokan commands I needed to and nearly everything worked. I even changed the Dokan .NET binding to use a Manzana.iPhone as parameter instead of an object to simplify some procedures. (No more convertings needed.)
But there were two big problems:
  * Reading did not work flawlessly. I was able to open a picture file in Paint or copy it to my computer. But opening it from the mounted iPhone's filesystem was not possible using Windows Photo Viewer.
  * Writing did not work flawlessly. This was similar to reading files. Small text files were written without any problems but bigger files did not work without changing the files' hashes. (I didn't check how many bytes were different).
So I searched for the problem. It took nearly two days (Well, I didn't work two whole days, but many hours on this project searching the error). I searched the problem only in my own code, but not in the libraries I used. I added some more console output and finally found the error. It seemed that the files' offsets were not properly set while reading or writing. So I created a workaround that opened the files again and read into a buffer until they had the right offsets. This worked, but it was slow and inefficient. But now I finally realized that there had to be a problem with Manzana.dll. I looked into the Seek() procedure of iPhoneFile class, but did not find any problems. Then I looked into the underlying MobileDevice calls and found a comment saying "FIXME - Currently only returns 7 and fails" or something like that. Well, if I found that earlier it might have cost me less time.
But nevertheless I searched for the correct definition of AFCFileRefSeek (or how this procedure is called) and found out that it needed one last int given that is not even good for anything. I added this int and it worked.
I contacted iPhoneFS's author and told him about my fix so that he is able to actually fix it in his repo.


---


Now you know something about this project's backgrounds.
Let's talk about how you use it.

# Usage #

Oh, of course you need to install [Dokan](http://dokan-dev.net/en) version 0.5.3 or higher first to use iPhile.

You just run iPhile.exe and it should work. In its current releases it logs events and errors into the file log.txt.
When you run iPhile.exe, you see some kind of disclaimer. After displaying this message, iPhile will start listening to iDevice connect/disconnect events.
If any device is found, it is mounted into Windows Explorer using a free drive letter.
When a device gets disconnected, it is dismounted. (If iPhile should crash, a device might still be in Windows Explorer and it might block a drive letter. Use DriveUnmounter.exe to unmount it then.)
If your device is jailbroken (afc2 service installed), two connections to it will be opened.
  * One to afc2 service. This is mapped into Windows Explorer.
  * One to afc service which is availabe even if the device is not jailbroken. This is not mapped into Windows Explorer, but instead, it will be used to determine the correct space values (free bytes, total bytes) of your iDevice instead of just showing the root partition's space.
If your device is not jailbroken (or has the afc2 service not installed), only a connection to afc service will be set up giving you only access to the device's Media partition (and its space values).
Isn't that simple?