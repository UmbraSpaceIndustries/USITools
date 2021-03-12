UmbraSpaceIndustries
====================

Shared components for KSP mods.

## USITools

This is the main assembly in this repo and is a dependency of nearly ever other mod in the USI cinematic universe. Some features of USITools:

- MANY PartModules for things like...
  - Airbags
  - Submersibles
  - Resource converters with swappable recipes
  - Resource and power distribution to nearby vessels
  - Robotic parts
  - ...and more
- Custom skill traits for Kerbals
- A resource and texture switching system
- A dependency injection system to facilitate unit testing, manage singletons, etc.
- A UI window manager (see `USIToolsUI`)

## USIToolsUI

**New for 2021!**

This is a collection of interfaces and MonoBehaviours to facilitate the use of [Unity UI](https://docs.unity3d.com/2019.2/Documentation/Manual/UIToolkits.html) in KSP. This assembly is designed to be used with the `WindowManager` class in `USITools`.

> Developer Notes:
> `USIToolsUI` does not and should not reference `USITools`.
>
> It is safe for your UI controllers to reference `USIToolsUI` but should not reference `USITools` or any other KSP assembly. Any references to objects in KSP assemblies (or assemblies that reference KSP assemblies) should be passed in to your UI controllers by reference at runtime.

Check out [this excellent tutorial](https://forum.kerbalspaceprogram.com/index.php?/topic/151354-unity-ui-creation-tutorial/) by DMagic for help creating Unity UI asset bundles for KSP.

## USI_Converter and LIB

Deprecated...?? These have probably been absorbed into USITools by now and just haven't been cleaned up.

# Contributing

Please make all pull requests to the DEVELOP branch, not master.
