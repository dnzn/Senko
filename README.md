# Project: Remo
Remo or Remo.kon started as a personal project to control my Sony Bravia Android TV via IP commands with Google Assistant or my Google Home Mini via IFTTT. Barking certain commands to Assitant would inform IFTTT to create or overwrite a certain file on my OneDrive which Remo monitors for changes. The text file, contains simple, one word commands that Remo will parse and process automagically. It doesn't have to be OneDrive as any cloud storage service that can do local file sync and which IFTTT has access too can be used. It's also agnostic to whatever digital assistant you want to use because of IFTTT.

Do note that the files currently synced here is **not the same functional prototype** that I already had working. I have decided too rewrite the whole thing to port it into a .NET Core console app for the possibility of compiling it for Linux and run it on a Raspberry Pi. I wonder if I can get some implemtation of this on my Netduino Plus. 

I decided to make it a console app as I plan to just tuck it in a server and let it do its work in peace. The original barely use 20MB of RAM which means I could get it to work in even a potato.

Speaking of potatoes, damn, I want some spicy fries right now...

Anyway, I'm working on this on my free time so, yeah, this might take a while.

## Things that this current build can do
Basically...

>NOTHING THAT I MENTIONED ABOVE YET!

Darn it! Haha! I'm still currently working on the Konsole class which should make displaying stuff on the console easy.Once I'm happy with that, I will start actual work on copying or porting over the code I already made from the original project and refining them. If you're interested in this project, watch this space. You are free to contribute and please don't hesitate to provide suggestions.

## Things the original could do
At the time of writing, the original could (including what I mentioned above):
- Parse specific files for commands, codes, and aliases (for example "Xbox One" for the "Hdmi1" code, etc.)
- Get and send IR codes, get and set volume (with caveats) through POST with JSON and XML using the RestSharp library.
- Get list of apps installed on my TV and start them too.
- Get certain device information (model number, serial etc.)

I decided to not share the original code as it is a mess (though it works). It's a working mess.

## Things I will be working on
- I would like to implement new JSON and XML generatiom methods. Although the ones I made are pretty fast with well-formed results, I would like something more extensible that I can use in future projects. The current one just manipulate strings in certain ways but their fine-tuned to work with what I need for my TV. I would like to serialize or deserialize them, instead. I still need to learn how to do those properly. JSON and XML aren't my strongest points, after all.
- I may try ditching RestSHarp and use HttpWebRequest. I would like to make use of whatever is natively available on .NET Core, if possible. I also thing it would be a good learning experience. I've never worked with REST until I started this project so I still have a way to go. I'm also getting issues when running certain commands in sequence (they'd just stop working) with RestSharp. They rarely happen, though.
- The original implements classes to store and search for IR codes, app URIs and aliases. I'm thinking of using dictionaries instead which I only learned how to use recently. I currently implement dictionaries for word to number conversions and numeric homophones (I ended up needing this because stupid Google Assistant would only hear "HDMI 2" as "HDMI to") and I liked how simple they are to use.

## Future plans
- Implement a config (*.konfig!) file system to store device information on so that the app will autoconfigure itself everytime it starts.
- Twitter integration will be nice. Might even be faster than a synced file which can sometimes be delayed when the filesystem is busy.
- Implement a REST/Webhooks server so I can control it remotely with other devices in the local network, maybe even beyond.
- Maybe whip up a separate app that would let me control my PC as well.
- Some other stuff from the future that I have not thought about yet...
