# Radio Furniture
Adds a buyable radio to Lethal Company that can play thousands of real radio stations.

The radio is found in the store and purchased for 100 credits.

**⚠️ WARNING:** This mod uses a [live, community-driven database of radio stations](https://www.radio-browser.info/). I am not responsible for the entries, nor the content found on the radio stations themselves. 

## Credits/Licensing

This mod uses the [RadioBrowser API](https://www.radio-browser.info/), an amazing community-driven resource for finding internet radio stations.

### Assets
Original radio 3D model by [@paperusu](https://twitter.com/paperusu) (`nichekino` on Discord).

"Dial Grab" icon by `Donald#6195` on Discord.

### GPL-3 Libraries
The MP3 audio streaming implementation is a modified version of [MSCModLoader]()'s, which is licensed under GPL-3.

Copyright (C) 2023 piotrulos

The RadioBrowser API is [a fork](https://github.com/legoandmars/RadioBrowser.NET) of [~youkai's RadioBrowser.NET](https://git.sr.ht/~youkai/RadioBrowser.NET), which is licensed under GPL-3.

Copyright (C) 2020 bt

### MIT Libraries
To make async code more compatible with unity, this repo uses [UniTask](https://github.com/Cysharp/UniTask), which is licensed under MIT.

To make radio station audio streaming possible, this repo uses [NAudio](https://github.com/naudio/NAudio), which is licensed under MIT.

To add furniture to the game, this repo uses [LethalLib](https://github.com/EvaisaDev/LethalLib), which is licensed under MIT.

To make ClientRPC/ServerRPCs work properly ingame, this repo uses [UnityNetcodeWeaver](https://github.com/EvaisaDev/UnityNetcodeWeaver), which is licensed under MIT.