
using System;
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
    public long LatestTime;
    public float AproximateWork;
    public long TimeThisSecond;

    private PathfindingManager manager;
    private System.Diagnostics.Stopwatch watch;
    private System.Diagnostics.Stopwatch secondWatch;


    public PathfindingThread(PathfindingManager m, int number)
    {
        this.ThreadNumber = number;
        this.manager = m;
        this.Pathfinder = new Pathfinding();
        this.watch = new System.Diagnostics.Stopwatch();
        this.secondWatch = new System.Diagnostics.Stopwatch();
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

        secondWatch.Start();

        while (Run)
        {
            try
            {
                if(secondWatch.ElapsedMilliseconds >= 1000)
                {
                    secondWatch.Reset();
                    secondWatch.Start();
                    AproximateWork = Mathf.Clamp01(TimeThisSecond / 1000f);
                    TimeThisSecond = 0;
                }

                int count = Queue.Count;
                if (count == 0)
                {
                    Thread.Sleep(IDLE_SLEEP);
                }
                else
                {
                    // Lock to prevent simultaneous read and write.
                    PathfindingRequest request;
                    lock (manager.QueueLock)
                    {
                        request = Queue.Dequeue();
                    }

                    if (request == null)
                    {
                        // Very ocassionally happens. Nothing to worry about :)
                        continue;
                    }

                    List<PNode> l;
                    watch.Reset();
                    watch.Start();
                    var result = Pathfinder.Run(request.StartX, request.StartY, request.EndX, request.EndY, manager.Provider, request.ExistingList, out l);
                    watch.Stop();
                    LatestTime = watch.ElapsedMilliseconds;
                    TimeThisSecond += LatestTime;

                    // Got the results, now enqueue them to be given back to the main thread. The called method automatically locks.         
                    manager.AddResponse(new PathReturn() { Callback = request.ReturnEvent, Path = l, Result = result });
                }
            }
            catch(Exception e)
            {
                Debug.Log("Exception in pathfinding thread #" + number + "! Execution on this thread will attempt to continue as normal. See:");
                Debug.LogError(e);
            }
        }

        secondWatch.Stop();
        Debug.Log("Stopped pathfinding thread #" + number);
    }
}
