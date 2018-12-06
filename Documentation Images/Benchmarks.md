## Benchmark results
Tested using a 50x50 tile map. Total 2500 tiles.
Tested with 1 and 4 threads, with varying number of pathfinding agents.
CPU used: i7-7700k (4.2GHz, 4 cores, 8 logical processors)

__Terminology__
> __Calculation delay:__ The time between the request and repose, in frames. Assumes 60fps.
> 
> __Aproximate work:__ The percentage of each second that was spent calculating paths. 0% means idle, 100% means full load.
> 
> __FPS:__ Frames per second. Pathfinding should not actually affect this. FPS goes down because more agents are being rendered.

Note that smaller maps perform significantly better (60+fps and no calculation delay with 5000+ agents).

Larger maps perform worse (10-40 frames of calculation delay on 200x200 map with 100 agents).

Complexity of geometry also affects speed. Open areas are slower, corridors are faster.

## 1 Thread
__20 agents:__
* _Aprox. work_: 2%
* _FPS_: 960
* No noticeable calculation delay. (1 frame)
![50x50, 20 agents, 1 thread](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/50x50%2020c.jpg)

__100 agents:__
* _Aprox. work_: 23%
* _FPS_: 630
* No noticeable calculation delay. (1-2 frames)
![50x50, 100 agents, 1 thread](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/50x50%20100c.jpg)

__~500 agents:__
* _Aprox. work_: 94%
* _FPS_: 460
* Somewhat noticeable calculation delay. (10-25 frames)
![50x50, around 500 agents, 1 thread](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/50x50%20~500c.jpg)

## 4 Threads
__20 agents:__
* _Aprox. work_: 1%
* _FPS_: 950
* No noticeable calculation delay. (1 frame)
![50x50, 20 agents, 4 threads](https://raw.githubusercontent.com/Epicguru/ThreadedPathfinding/master/Documentation%20Images/50x50%204t%2020c.jpg)

__100 agents:__
* _Aprox. work_: 5%
* _FPS_: 830
* No noticeable calculation delay. (1-2 frames)
![50x50, 100 agents, 4 threads](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/50x50%204t%20100c.jpg)

__300 agents:__
* _Aprox. work_: 12%
* _FPS_: 512
* Tiny calculation delay. (1-4 frames)
![50x50, 300 agents, 4 threads](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/50x50%204t%20300c.jpg)

__1000 agents:__
* _Aprox. work_: 68%
* _FPS_: 320
* Small calculation delay. (4-8 frames)
![50x50, 300 agents, 4 threads](https://github.com/Epicguru/ThreadedPathfinding/blob/master/Documentation%20Images/50x50%204t%201000c.jpg)
