using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    [Header("Debug")]
    [TextArea(20, 20)]
    public string Info;

    public object QueueLock = new object(); // The lock for adding to and removing from the processing queue.
    public object ReturnLock = new object(); // The lock for adding to and removing from the return queue.
    private PathfindingThread[] threads;
    private List<PathfindingRequest> pending = new List<PathfindingRequest>();
    private List<PathReturn> toReturn = new List<PathReturn>();
    private StringBuilder str = new StringBuilder();

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
                {
                    //Debug.Log("Returned! to " + item.Callback.Target);
                    item.Callback.Invoke(item.Result, item.Path);
                }
            }
            toReturn.Clear();
        }
    }

    public void LateUpdate()
    {
        if (threads == null || threads.Length == 0)
            return;

# if UNITY_EDITOR
        // Compile debug information.
        str.Append("Overall work strain: ");
        float sum = 0f;
        for (int i = 0; i < threads.Length; i++)
        {
            float w = threads[i].AproximateWork;
            sum += w;
        }
        sum /= threads.Length;
        str.Append((sum * 100f).ToString("N0"));
        str.Append("%");
        str.AppendLine();
        str.AppendLine();
        str.Append("Requests this frame: ");
        str.Append(pending.Count);
        str.AppendLine();
        str.Append("Pending returns: ");
        str.Append(toReturn.Count);
        str.AppendLine();
        for (int i = 0; i < threads.Length; i++)
        {
            var t = threads[i];
            int count = t.Queue.Count;
            long time = t.LatestTime;
            float work = t.AproximateWork;

            str.Append("Thread #");
            str.Append(i);
            str.Append(": ");
            str.AppendLine();
            str.Append("  -");
            str.Append(count);
            str.Append(" pending requests.");
            str.AppendLine();
            str.Append("  -Last path time: ");
            str.Append(time);
            str.Append("ms");
            str.AppendLine();
            str.Append("  -Aprox. work: ");
            str.Append((work * 100f).ToString("N0"));
            str.Append("%");
            str.AppendLine();
        }

        this.Info = str.ToString();
        str.Length = 0;
#endif

        // Lock because the number of pending requests for each thread should not change.
        // Hopefully this lock should last no more than a millisecond at most, probably less.
        lock (QueueLock)
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
