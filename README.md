# IRL-Royale
## Introduction
IRL RoyaLe is a retro-cyberpunk themed battle royale game that is played in real life. It supports 2 - 50 players, and is great fun when playing as a group!
## Quck Start Guide
IRL RoyaLe only works on Android, although an iOS build is on its way. Make sure the app is [installed] beforehand!
1. To enter the game, open the app and wait for the home screen to load.

<img src="https://raw.githubusercontent.com/chenjefferson/IRL-Royale-Doc-Assets/master/Home%20Screen.png" alt="The home screen" width="200px" />

2. Create an account or hit "Guest" if you want to hit the ground running.

<img src="https://raw.githubusercontent.com/chenjefferson/IRL-Royale-Doc-Assets/master/Find%20Lobby%20Screen.png" alt="The finding a lobby screen" width="200px" />

3. Then, walk to a nearby lobby. When you're in the area of a lobby, the Join button should appear. Tap it to join the lobby.

<img src="https://raw.githubusercontent.com/chenjefferson/IRL-Royale-Doc-Assets/master/Lobby%20Screen.png" alt="The in a lobby screen" width="200px" />

4. When enough players have joined the lobby, the countdown for the game start will begin.


5. When you're at the game screen, enemy players will appear when they are close enough. Tap the screen to attack them!


6. What happens when you're the last player standing? What if you die beforehand? Guess you'll have to play to find out.
## Installing
The .apk file in the root directory can be [installed] from the Android device. Steps are below:

1. From your Android device, download the .apk file [here](https://github.com/Panthson/IRL-Royale/raw/README/AndroidBuild.apk).
2. Navigate to the file in your File Explorer. Note that File Explorers tend to differ based on your manufacturer.
3. Tap the .apk file and then tap **Install** to install it.
## Building and Deploying
Creating the .apk file can be done as follows:

1. Add a signature to the Unity project. Go to **Player Settings>Android>Publishing Settings** and uncheck **Custom Keystore** to use the standard Android debug key, or add your own key.
![The Android tab in Unity's Player Settings](https://raw.githubusercontent.com/chenjefferson/IRL-Royale-Doc-Assets/master/Android%20Settings%20Tab.PNG)
2. Go to **Build Settings** and go to the **Android** tab denoted by the icon below:
![The Android tab in Unity's Build Settings](https://raw.githubusercontent.com/chenjefferson/IRL-Royale-Doc-Assets/master/Build%20Settings.PNG)
    3. Click **Build** in the bottom-right corner of the **Build Settings** screen.
4. Specify a location and name for the .apk file.
## Example Source Code
Here's a glimpse of our reusable database reading module. If you like it, or especially if you dislike it, we encourage you to contribute!
```cs
    public Task JoinLobby(string lobbyId)
    {
        var data = new Dictionary<string, object>();
        data["playerId"] = LoginInfo.Uid;
        data["username"] = LoginInfo.Username;
        data["lobbyId"] = lobbyId;

        var function = Functions.GetHttpsCallable("joinLobby");
        Player.Instance.lobby = lobbyId;

        return function.CallAsync(data).ContinueWith((task) =>
        {
            return task.Result.Data;
        });
    }

    public Task ExitLobby(string lobbyId)
    {
        var data = new Dictionary<string, object>();
        data["playerId"] = LoginInfo.Uid;
        data["lobbyId"] = lobbyId;

        var function = Functions.GetHttpsCallable("exitLobby");
        Player.Instance.lobby = null;

        return function.CallAsync(data).ContinueWith((task) =>
        {
            return task.Result.Data;
        });
    }
```
## Contributing
IRL RoyaLe is open source and written in Unity, Mapbox, and Firebase. An official bug tracker for the project will be out Soon. In the meantime, feel free to add any functionality you want or fix any bugs that you find!