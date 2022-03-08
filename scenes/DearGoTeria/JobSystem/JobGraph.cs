using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

///
/// A JobGraph is an abstraction of an (hopefully) acyclic directed graph of
/// Jobs. A JobGraph is created using a JobGroup that contains all the jobs.
///
/// Running all the jobs in a JobGraph should look similar to the following:
///
///     while (jobGraph.HasJobs())
///     {
///         Job? job = jobGraph.DequeueJob();
///         if (job == null)
///             break;
///     
///         Task.Run(() =>
///         {
///             job.Run(jg);
///         });
///     }
///     jobGraph.Join();
///
/// JobGraphs are designed to have the jobs run in a thread pool, however this
/// is not strictly required.
///
public class JobGraph
{
    // All jobs in the graph
    public IEnumerable<Job> Jobs => graph.Jobs;

    public Dictionary<string, IEnumerable<Job>> Tags => graph.Tags;

    public string? Name { get; }

    private readonly JobGroup graph;
    private readonly Queue<Job> runnableJobs;
    private int finishedJobs = 0;
    private int requestedJobs = 0;
    private readonly EventWaitHandle jobsDoneHandle;
    private readonly Dictionary<Job, EventWaitHandle> taggedJobsHandles;

    ///
    /// Creates a new JobGraph. A JobGraph can be optionally supplied with a
    /// name and a master seed. The name is used when viewing the trace, and the
    /// master seed will be used to initialise all the jobs' Random() instances.
    ///
    public JobGraph(JobGroup graph, string? name = null, int randomSeed = 1337)
    {
        this.graph = graph;
        Name = name;
        var masterRandom = new Random(randomSeed);

        foreach (var job in Jobs)
        {
            job.InjectRandomSeed(masterRandom.Next());
        }

        runnableJobs = new Queue<Job>(graph.Roots);
        jobsDoneHandle = new ManualResetEvent(false);

        taggedJobsHandles = new Dictionary<Job, EventWaitHandle>();
        foreach (var tagPair in graph.Tags)
        {
            var taggedJobs = tagPair.Value;
            foreach (var job in taggedJobs)
            {
                if (taggedJobsHandles.ContainsKey(job))
                    continue;

                taggedJobsHandles.Add(job, new ManualResetEvent(false));
            }
        }
    }

    ///
    /// Returns true if all the jobs have not yet been dispatched
    ///
    public bool HasJobs()
    {
        return requestedJobs < Jobs.Count();
    }

    ///
    /// Dequeues and returns the next job to be run when it is available.
    /// If all the jobs have been dequeued, then this method will return null.
    ///
    /// When we still has jobs, but the next jobs are waiting for their
    /// dependencies to finish, this method will wait (using a condition
    /// variable) until the next job is available to be dequeued.
    ///
    public Job? DequeueJob()
    {
        if (!HasJobs())
            return null;

        lock (runnableJobs)
        {
            requestedJobs += 1;
            while (runnableJobs.Count == 0)
            {
                Monitor.Wait(runnableJobs);
            }

            return runnableJobs.Dequeue();
        }
    }

    ///
    /// Used by the Job class to notify us that this job is ready to be run.
    ///
    public void MarkJobRunnable(Job job)
    {
        lock (runnableJobs)
        {
            runnableJobs.Enqueue(job);
            Monitor.Pulse(runnableJobs);
        }
    }

    ///
    /// Used by the Job class to notify us that this job has completed its
    /// task.
    ///
    public void MarkJobDone(Job job)
    {
        Interlocked.Increment(ref finishedJobs);
        if (finishedJobs == Jobs.Count())
        {
            jobsDoneHandle.Set();
        }

        if (taggedJobsHandles.ContainsKey(job))
            taggedJobsHandles[job].Set();
    }

    ///
    /// This method will block until all task in the JobGraph have completed.
    ///
    public void Join()
    {
        jobsDoneHandle.WaitOne();
    }

    ///
    /// This method will block until all tasks in a JobGroup (including those
    /// used to construct the final JobGroup) that have been given a tag have
    /// completed. Example:
    /// 
    ///     JobGraph(
    ///         JobGroup({ A }, "foo") -> JobGroup({ D, E })
    ///     )
    ///
    ///     In this example, the following graph will be created:
    ///
    ///     A -> D
    ///       -> E
    ///
    ///     So, jobGraph.Join("foo") will return when task A has completed. 
    ///
    /// Throws an ArgumentException if the tag does not match any known tag.
    ///
    public void Join(string tag)
    {
        if (!graph.Tags.ContainsKey(tag))
            throw new ArgumentException();

        foreach (var job in graph.Tags[tag])
        {
            taggedJobsHandles[job].WaitOne();
        }
    }

    ///
    /// Returns a pretty print format of the JobGraph that includes all jobs.
    ///
    public override string ToString()
    {
        return string.Format(
            "JobGraph(Name={0})\n{1}", Name, string.Join("\n", Jobs));
    }
}