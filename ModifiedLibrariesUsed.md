# Information about changes in used libraries #
In this short article, I will tell what has been modified in the dynamically linked libraries used in iPhile.

## Dokan .NET binding ##

Dokan's .NET binding did not have the possibility to mount a drive as removable, but Dokan itself had.
So I implemented a new DOKAN\_OPTIONS option with value 32 which is called DOKAN\_OPTION\_REMOVABLE.
Furthermore, I changed Dokan's DokanFileInfo class. It now uses a Manzana.iPhoneFile Context instead of object Context. This simplifies iPhile's work a lot since it is not needed to use (iPhoneFile)Context.

## Manzana ##

I changed a lot in Manzana's code. I fixed AFCFileRefSeek function.
I also changed the iPhone class:
  * iPhone class doesn't have connect/disconnect events any longer
  * instead, this is managed in a new class called "MultiPhone"
  * this MultiPhone doesn't have any properties, it only handles iPhone's connect/disconnect events
  * A new class called "iPhone\_ForceAFC" was created. It is exactly the same as my new iPhone class, but always connects to afc service instead of afc2 (when available, i.e. jailbroken)
  * this class is only used to establish an afc connection to get media partition's size values.