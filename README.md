# Project: Senko
Formerly a personal project called Remo or Remo.kon, it is now renamed Senko after the fox in the anime "The Helpful Fox Senko-san". Senko is also a backronym for "SENd KOmmand" pertaining to its expected function of controlling my Sony Bravia TV (and eventually other devices) by sending commands through an IP network.

I have created an early prototype that allowed me to bark certain commands to Google Assistant which would then inform IFTTT to create or overwrite a certain text file on my OneDrive which the program monitors for changes. The file contains simple, one-word commands that the program parses into commands that my TV could understand.

Senko is a complete rewrite of that prototype and is currently not fully functional at the moment. It is now a .NET Core console application which should allow the possibility to compile it for other operating systems like Linux and run it on a Raspberry Pi, perhaps as it should not use a lot of RAM.

Anyway, I'm working on this on my free time so, yeah, this might take a while.

## Things the original prototype could do
At the time of writing, the original prototype could:
- Parse specific files for commands, codes, and aliases (for example "Xbox One" for the "Hdmi1" code, etc.)
- Get and send IR codes, get and set volume (with caveats) through POST with JSON and XML using the RestSharp library.
- Get list of apps installed on my TV and start them too.
- Get certain device information (model number, serial etc.)

I decided to not share the original code as it is a mess (though it works). It's a working mess.

## Overview
The Polymer namespace (foremerly known as Kontext) contains the Konsole classes, subclasses, fields and methods that manage how information is displayed in the console. It features easy console color changes using inline color tags, automatic word-wrapping, automatic prompts and indenting, and a persistent log that can be written onto a binary file. More features and methods (like Read and ReadLine) will be added as needed.

The SonyDevice class will handle communication with and control of Sony TV and compatible devices through IP commands. At the time of writing, some of its subclasses are now functional to a point such as parsing device information, commands, apps and aliases from JSON or ALIAS files. It should be trivial to modify the code to parse live JSON from the device once the REST subclass is functional.

## Future plans
- Implement a config (*.konfig!) file system to store device information on so that the app will autoconfigure itself everytime it starts.
- Twitter integration will be nice. Might even be faster than a synced file which can sometimes be delayed when the filesystem is busy.
- Implement a REST/Webhooks server so I can control it remotely with other devices in the local network, maybe even beyond.
- Maybe whip up a separate app that would let me control my PC or a remote PC as well.
- Some other stuff from the future that I have not thought about yet...
