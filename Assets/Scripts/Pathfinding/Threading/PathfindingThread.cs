
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PathfindingThread
{
    /// <summary>
    /// The amount of time, in milliseconds, that is spent idling when no new request is detected. 0 means no sleep time, ever.
    /// Lower values mean that path responses will be faster, often the very next frame. However, it will use more of the CPU time when there are no requests.
    /// Higher values mean that the responses will be slower, but the CPU isn't used as much when there are no requests. Default value is 5, which still allows for next-frame responses at 60fps.
    /// </summary>
    const int IDLE_SLEEP = 5;

    public Thread Thread;
    public int ThreadNumber;
    public bool Run { get; private set; }
    public Queue<PathfindingRequest> Queue = new Queue<PathfindingRequest>();
    public Pathfinding Pathfinder;
    private PathfindingManager manager;
    
    public PathfindingThread(PathfindingManager m, int number)
    {
        this.ThreadNumber = number;
        this.manager = m;
        this.Pathfinder = new Pathfinding();
    }

    public void StartThread()
    {
        if (Run == true)
            return;

        Run = true;
        Thread = new Thread(new ParameterizedThreadStart(RunThread));
        Thread.Start((object)ThreadNumber);
    }

    public void StopThread()
    {
        Run = false;
    }

    public void RunThread(object n)
    {
        int number = (int)n;
        Debug.Log("Started pathfinding thread #" + number);

        while (Run)
        {
            int count = Queue.Count;
            if(count == 0)
            {
                Thread.Sleep(IDLE_SLEEP);
            }
            else
            {
                // Lock to prevent simultaneous read and write.
                PathfindingRequest request;
                Debug.Log("Locking for dequeue...");
                lock (manager.QueueLock)
                {
                    request = Queue.Dequeue();
                }
                Debug.Log("Finished lock, request is " + request);

                if(request == null)
                {
                    // Very ocassionally happens. Nothing to worry about :)
                    continue;
                }

                Debug.Log("Starting pathfinding run...");
                List<PNode> l;
                var result = Pathfinder.Run(request.StartX, request.StartY, request.EndX, request.EndY, manager.Provider, request.OutputPath, out l);

                Debug.Log("Processed request on thread #" + number);

                // Got the results, now enqueue them to be given back to the main thread. The called method automatically locks.         
                manager.AddResponse(new PathReturn() { Callback = request.ReturnEvent, Path = l, Result = result });                
            }
        }

        Debug.Log("Stopped pathfinding thread #" + number);
    }
}
