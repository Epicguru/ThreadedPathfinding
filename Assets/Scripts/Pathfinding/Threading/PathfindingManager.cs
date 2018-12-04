using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance;

    public TileProvider Provider
    {
        get
        {
            return _provider;
        }
        set
        {
            _provider = value;
        }
    }
    private TileProvider _provider;

    [Header("Startup")]
    public bool AutoCreateThreads = true;
    public bool AutoStartThreads = true;

    [Header("Threading Settings")]
    [Range(1, 16)]
    public int ThreadCount = 1;

    public object QueueLock = new object(); // The lock for adding to and removing from the processing queue.
    public object ReturnLock = new object(); // The lock for adding to and removing from the return queue.
    private PathfindingThread[] threads;
    private List<PathfindingRequest> pending = new List<PathfindingRequest>();
    private List<PathReturn> toReturn = new List<PathReturn>();

    public void Awake()
    {
        Instance = this;

        if (AutoCreateThreads)
        {
            // Create threads...
            CreateThreads(ThreadCount);
            if (AutoStartThreads)
            {
                // Run those threads.
                StartThreads();
            }
        }
    }

    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (threads != null)        
            StopThreads(); 
    }

    /// <summary>
    /// Should only be called from unity events such as Start or Update.
    /// The requests are actually scheduled at the end of LateUpdate.
    /// Requests are returned (once completed) before Update.
    /// For example, a request is made in frame 0 in the Update thread. The path takes 1 frame to calculate.
    /// Then, in frame 2, before any calls to Update, the return method is called.
    /// </summary>
    /// <param name="request"></param>
    public void Enqueue(PathfindingRequest request)
    {
        if (request == null)
            return;

        if (!pending.Contains(request))
        {
            pending.Add(request);
        }
        else
        {
            Debug.LogWarning("That pathfinding request was already submitted.");
        }
    }

    public void AddResponse(PathReturn pending)
    {
        // Prevent this from being called when currently returning stuff.
        lock (ReturnLock)
        {
            toReturn.Add(pending);
        }
    }

    public void Update()
    {
        // Assumes that this is called before all other script's update calls. It still works regardless of order but
        // it makes more sense to give path requests back before the update.

        lock (ReturnLock)
        {
            foreach (var item in toReturn)
            {
                if(item.Callback != null)
                    item.Callback.Invoke(item.Result, item.Path);
            }
            toReturn.Clear();
        }
    }

    public void LateUpdate()
    {
        if (threads == null || threads.Length == 0)
            return;

        // Lock because the number of pending requests for each thread should not change.
        // Hopefully this lock should last no more than a millisecond at most, probably less.
        lock (ReturnLock)
        {
            foreach (var request in pending)
            {
                // For each request we need to find the thread with the lowest number of requests on it.
                int lowest = int.MaxValue;
                PathfindingThread t = null;
                foreach (var thread in threads)
                {
                    if(thread.Queue.Count < lowest)
                    {
                        lowest = thread.Queue.Count;
                        t = thread;
                    }
                }

                // Enqueue it on this thread.
                t.Queue.Enqueue(request);
                Debug.Log("Enqueued request onto thread #" + t.ThreadNumber);
            }
            pending.Clear();
        }        
    }

    public void CreateThreads(int number)
    {
        if (threads != null)
            return;

        threads = new PathfindingThread[number];
        for (int i = 0; i < number; i++)
        {
            threads[i] = new PathfindingThread(this, i);
        }
    }

    public void StartThreads()
    {
        if (threads == null)
            return;

        for (int i = 0; i < threads.Length; i++)
        {
            var t = threads[i];
            t.StartThread();
        }
    }

    public void StopThreads()
    {
        if (threads == null)
            return;

        for (int i = 0; i < threads.Length; i++)
        {
            var t = threads[i];
            t.StopThread();
        }
    }
}
