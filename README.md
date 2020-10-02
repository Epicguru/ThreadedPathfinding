# Threaded Pathfinding
This is a multithreaded, request-callback based implementation of the A* pathfinding algorithm. Intended for use with the Unity (2D) game engine. Requires a __2D tile-based map__, but can be adapted to work with hexagonal or even triagular maps.

[Click here](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/Benchmarks.md) for benchmarks (with pictures :D).

__Important: This is not a 'drag and drop' solution. You will need to be comfortable with fairly advanced programming concepts and knowlege to make this work with your project__

## Features
* Can use 1-16 threads to do pathfinding on.
* Uses a request-callback system that implements seamlessly into the Unity game loop. This means that once the path is calculated, you can call Unity API methods with no problem. Forget about multi-thread headache.
* Has simple load-sharing system that aims to give the fastest path calculation time when one thread is stuck.
* Is very generic - with a reasonable amout of work it can be integrated into almost any existing system.
* Allows for dynamically changing maps (but you will need to recalculate the path when a tile along the path changes).

## Instalation
1. Download the repository from github. It contains an example scene where you can quickly demo how it works.
2. Copy the folder named Pathfinding, which is located in the [Assets/Scripts/](https://github.com/Epicguru/ThreadedPathfinding/tree/master/Assets/Scripts) folder.
3. Paste this Pathfinding folder into your own Unity project (in the Scripts or Assets folder, for example).

## Theory and general working
So how does this work? The general path calculation flow would look something like this:
1. A PathfindingRequest is created. You supply the start and end points, as well as a callback method.
2. This request is placed into a queue in the manager class. At the end of each frame, the items in the queue are assigned to a thread, where they await processing.
3. Each thread has a queue of requests, which it continuously processes. Once a request has been processed, and the path calculated, it is handed back to the manager class in a thread-safe manner.
4. At the beginning of each frame, all completed requests are dispatched to the request makers. This completes the cycle of a request.
In order for this to work, a TileProvider must be created. A TileProvider is the way in which the pathfinding system interacts with your 2D tile based world. Keep on reading to learn how to set up the TileProvider and other systems required to get this system working with your own project.
Unfortunately, since this is a multithreaded system, writing code for it is more complex than a synchronous system, but of course it runs faster and does't affect frame rate.

## Setting up
First, it is important that you already have some kind of 2D tile based map set up. At the very minimum, _you should be able to instantly determine weather a tile at a particular coordinate is solid or not (as in, can it be walked through/over)_.

### In the editor
Once you have done the installation steps, open your project.
Open your game scene and then press this menu button in the editor:

![Create Pathfinding Manager](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/CreateManagerButton.png?raw=true)

This will create a new GameObject in the scene, with the PathfindingManager component on it. The component looks like this:

![PathfindingManager Inspector](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/Inspector.png?raw=true)

Lets look at each part of this simple component. See the red numbers in the image:
1. When these are true (which they are by default) the pathfinding threads are created and started automatically as soon as the scene is loaded. If false, you will have to create and start the threads manually from the API.
2. This is the number of threads to create. This only applies when using automatic thread creation. The default number is 2, but most modern computers can handle 4 threads.
3. This is a space for debug information, which is shown when the game is running.

### Basic code
Now that the editor side of things has been set up, we need to write some code.
_NOTE: You need to import the ThreadedPathfinding namespace to use the classes mentioned here._

First to create the TileProvider. Click [here](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Assets/Scripts/Pathfinding/Pathfinding/TileProvider.cs) to see the source code for TileProvider.
You must create a custom class that inherits from TileProvider and implements the IsTileWalkable method. It might look something like this:
``` C#
import ThreadedPathfinding;

public class MyCustomProvider : TileProvider
{
  // MyMap is a made up class. You would replace it with your own class, or system.
  // In this case, it has a width and height and can return any tile when provided it's coordinate.
  public MyMap Map;
  
  // This is the constructor. You need to provide a width and height to the base TileProvider class.
  public MyCustomProvider(MyMap map) : base(map.Width, map.Height)
  {
    // Store a reference to the map.
    this.Map = map;
  }
  
  public override bool IsTileWalkable(int x, int y)
  {
    // Get the tile at those coordinates, and return it's walkable state.
    return Map.GetTile(x, y).IsWalkable;
  }
}
```
The imporant thing to understand about this class is how the IsTileWalkable method works. Basically, you return true if the tile at the coordinate is walkable, such as air, and return false if the tile is solid, such as a wall. __The speed of this method directly controls how fast the overall pathfinding will be__. Therefore it is very imporant to make this method very optmimized.

### Linking it all together
Now we have the PathfindingManager and the custom TileProvider class. Next we must link them together to be able to start making pathfinding requests. Fortunately, it is very simple. Create a new MonoBehaviour and write the following code:
```C#
import UnityEngine;
import ThreadedPathfinding;

public class Linker : MonoBehaviour
{
  // The same MyMap object from earlier. Replace this with your own class or system.
  public MyMap Map;
  
  // Called once when the scene is loaded.
  // Important to use Awake and not Start so that no pathfinding requests are made before we set the provider.
  void Awake()
  {
    // Get a reference to the PathfindingManager object.
    PathfindingManager manager = PathfindingManager.Instance;
    
    // Now set the provider object. Replace MyCustomProvider with whatever you named your provider.
    manager.Provider = new MyCustomProvider(Map);
  }
}
```
Now save your code and in the editor add the component to a new game object. Save the scene. We are now ready to make requests!

## Making requests
The pathfinding system revolves around requests and callbacks. Requests are the way in which you can request for a path to be calculated. Callbacks are the way in which the results of that request are given back to you. The important and nice thing about callbacks are that they integrate into the game loop, meaning that they are thread-safe and you can call Unity functions from within the callback. Here is how a request is made, and how it is returned:
```C#
using UnityEngine;
using ThreadedPathfinding;

public class Character : MonoBehaviour
{
  // Stores the currently active request.
  private PathfindingRequest CurrentRequest;
  
  void Start()
  {
    // Create a new request. We start at (0, 0) and want a path to (10, 10). The UponPathCalculated method is used as the callback.
    CurrentRequest = PathfindingRequest.Create(0, 0, 10, 10, UponPathCalculated);
  }
  
  void UponPathCalculated(PathfindingResult result, List<PNode> path)
  {
    // The path calculation is complete, but was not necessarily successful.
    
    // IMPORTANT: Dispose of the request.
    CurrentRequest.Dispose();
    CurrentRequest = null;    
    
    // Print the result state of the calculation. Ideally, it will be PathfindingResult.SUCCESSFUL
    Debug.Log("The result of the request is: " + result);
    
    if(path != null)
    {
      Debug.Log("There are " + path.Count + " points in the path!");
    }
    else
    {
      // The path is null when the request was not successful.
      Debug.Log("The path is null.");
    }
  }
}
```

I hope that the code is clear enough. We make a request in the Start method, and once the request has been processed UponPathCalculated is called. In small, simple maps where there are few requests, the calculation may only take 1 frame. In large maps, the calculation could take many frames to be processed. You can use Unity's Time.frameCount to determine the time between request and reponse.

Important notes about the 'path' that is given to you in the callback:
* It will be null if the path calculation failed.
* PNode is a simple class that contains X and Y values. It can be casted to Vector2.
* The path includes both the start and end positions.

That concludes the documentation for this system. The way you move along that path is up to you, but if you want to see one way of doing it see [this class](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Assets/Scripts/Example%20Code/Character.cs) which is part of the demo scene.

## Can I use this in a commercial project?
You are free to use this wherever you like. If this does help you out a lot, you might consider adding my name (Epicguru or James B) into the credits section of your project. This is using the MIT license, read it for more info on how you are allowed to use the code. You must always include the license with the code.

## Disclamers
This uses [BlueRaja's](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp) High Speed Priority Queue. All credit goes to them for their code. A copy of their license is included next to their code, in it's own seperate folder.
I can't guarantee that this is bug-free, or that it will necessarily work for you. I've done my best to test and optimize this, but it is far from perfect. If you find any bugs, report them here on Github by opening an issue.

## Contribution
Any contributions is welcome. If you make any changes that you think other people would benefit from, create a Pull Request and I'll merge it as soon as possible. 
