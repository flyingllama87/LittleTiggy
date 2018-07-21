### DESCRIPTION:

LittleTiggy is a small, arcade-like multi-platform game I created as a learning experience.  You control a character and try to navigate him through a randomly generated maze to the bottom of each level.  The enemies will try to tag you & you have to either avoid them or get the power-up and tag them back.  It runs on Windows, Linux, MacOS and Android.  The game is implemented in MonoGame & C#.  I invite all feedback on the game or my code as I'm sure I have plenty of room for improvement.

### DOWNLOADS:

Windows (2.5MB):
http://morganrobertson.net/LittleTiggy.zip

Android (13.9MB):
http://morganrobertson.net/LittleTiggy.apk

All desktop platforms (6MB) - Requires mono:
http://morganrobertson.net/LittleTiggyAllDesktop.zip

### SCREENSHOTS:

![Menu - Android](/Screenshots/LT_ANDROID_1.png?raw=true "Menu - Android")
![Leaderboard - Android](/Screenshots/LT_ANDROID_2.png?raw=true "Leaderboard - Android")
![Ingame - Android](/Screenshots/LT_ANDROID_3.png?raw=true "Ingame - Android")

### BACKGROUND & FEATURES:

Originally this project started out with a very small scope but it just kept growing.  The core game was actually implemented fairly quickly however as the project progressed I wanted to build some of the niceties you'd expect from a modern game.

It has the following features:

- Multi-platform: Windows & Android were the primary platforms.  Getting the game to run on Android wasn't much trouble but getting it to work well was!  The game supports two touch control methods.
- A* pathfinding: My own A* pathfinding implementation.  This was one of the hardest yet most interesting challenges.  Originally the enemies were very dumb and it was so laughable that I researched pathfinding and implemented one of the more popular solutions.  Use the player name 'debug' to see a visualisation of the pathfinding.
- LeaderBoard: A leaderBoard (aka networked high score) is a common feature in today's mobile games.  As I wanted to learn more about networking and web technologies I implemented a leaderBoard server in Python, Flask & JSON-RPC.  More details on that at https://github.com/flyingllama87/LTLeaderBoard.
- 3 difficulty levels: To suit the player's skill.  I found that few people could keep up my initial game settings when they first played it and this was frustrating for the player.  
- Game settings: Are saved / loaded for you automatically.

### WHAT I LEARNED:

In addition to learning about the above implemented features, I learned a lot about the following:

- Sprite based animation systems: In monogame you don't have an in-built animation system so you must implement one yourself.
- Art: I created all the art used in the game.  As you can tell I'm no artist but I'd like to think it's slightly better than most programmer art :P
- UI - This actually took a significant amount of time.  Originally there was no menu/UI system but I needed one to support a leaderboard & the ability for the player to set a name (for the leaderboard).  I then kept fleshing out the implementation of this to work well on multiple platforms with different control methods.  There are so many corner cases when it comes to UIs/Game Menus that you have to keep track of.  I'd definitely look at a UI toolkit if I was to do it over again.
- Basic state machines: With the menu system came a basic state machine to keep track of the game / menu state & update/draw the correct components.
- Control on mobile devices: This was another pain point and something I experimented with fairly extensively.  As the game relies on 'twitch' skills, a responsive controller is critical.  I prefer the 'screen tap' method but I found most of the people that would play it on an android device wanted a virtual joystick.
- Project size: This was the largest project I've ever coded at 3.5K lines.  This is nothing compared to professional coders but I did start encountering some difficulties with navigating and structured a growing project & maintaining a consistent code style.  More work could be done on this 
- Threading with C#:  Although performance was not an issue for me, I did want to make use of threaded programming for network operations so they don't block.  I also moved the A* pathfinding instances to their own threads as well.  I implemented this via background workers as it is Microsoft's recommended way if you need to pass and return data from the threads.
- MonoGame/XNA: I choose MonoGame as it's fairly lightweight but still gives you the tools you need to create a game without doing everything from the ground up.  I believe it taught me more than if I used Unity.  

### CREDITS:

- My daughter for the name of the game, playtesting & art for the 'power up'.
- The 'Sweet Easy' font from Billy Argel.  Thanks man! : https://www.facebook.com/billyargelfonts
- Sound effects & music from the awesome OpenGameArt project: https://opengameart.org/
- Adam Ashton's JSON RPC CHARP client: https://github.com/adamashton/json-rpc-csharp
- All the useful info on stack overflow!
