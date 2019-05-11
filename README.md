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

## What works now?
Basically, nothing much yet.

First, I had to work on the way to display information in a clean and consistent way first. I created the Konsole class under the Kontext namespace to do that and it wraps around the System.Console class and make it easier to add colors, prefixes and such. It is a complete rewrite of a rudimentary version that I created with Remo, the earlier prototype of Senko. It is now on a different namespace because it may become a project of its own in the future. At the moment of writing, the Write and WriteLine methods seem to be feature complete.

Some subclasses and methods under the SonyDevice class now works. It can now parse device information from a saved JSON file which should be trivial to modify to parse live JSON from the device itself once the REST subclass is working. Parsing of command aliases from a JSON or ALIAS file is now functional as well.

## Future plans
- Implement a config (*.konfig!) file system to store device information on so that the app will autoconfigure itself everytime it starts.
- Twitter integration will be nice. Might even be faster than a synced file which can sometimes be delayed when the filesystem is busy.
- Implement a REST/Webhooks server so I can control it remotely with other devices in the local network, maybe even beyond.
- Maybe whip up a separate app that would let me control my PC or a remote PC as well.
- Some other stuff from the future that I have not thought about yet...
