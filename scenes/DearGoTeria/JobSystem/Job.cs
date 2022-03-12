using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

///
/// Represents a single unit of work that can be assigned to a JobGraph. A job
/// consists of dependencies, and waiters. This job can only start once all
/// dependencies are finished. When this job finishes, it will notify all the
/// waiters and give them the result of the calculation.
///
/// To ensure deterministic random results in the delegate function, an
/// external entity (usually a JobGraph) is required to inject a seed that we
/// can create our Random() from.
///
/// According to Bungie's Barry Genova in the GDC talk 'Multithreading the
/// Entire Destiny Engine', jobs should take around 0.5ms or 2ms to complete to
/// counteract the overhead of using a thread. This, however, may not be
/// better than aggressively trying to utilise as much of the CPU as possible.
///
public class Job
{
    public string Name { get; }
    public List<Job> Waiters { get; } = new List<Job>();
    public List<Job> Dependencies { get; } = new List<Job>();
    public object? Args { get; }
    public object? Result = null;
    public delegate object WorkDelegate(Random random, List<object> dependencyResults, object? args);
    private readonly WorkDelegate workFunction;

    private int dependenciesDone = 0;
    private readonly List<object> dependencyResults = new List<object>();
    private readonly object dependencyResultsLock = new object();
    private bool isDone = false;
    private readonly object isDoneLock = new object();
    private Random? random = null;

    ///
    /// Create a new Job. The WorkDelegate defines the function that this job
    /// should run when job.Run() is called.
    ///
    public Job(string name, WorkDelegate workFunction, object? args = null)
    {
        Name = name;
        this.workFunction = workFunction;
        Args = args;
    }

    ///
    /// This method needs to be called from an external source such that this
    /// job has a random seed it can use.
    ///
    public void InjectRandomSeed(int seed)
    {
        random = new Random(seed);
    }

    ///
    /// Runs the job and tells its waiters the result.
    ///
    public object Run(JobGraph jobGraph)
    {
        Assert.NotNull(random, "Job requires a random seed to be run");
        Assert.True(CanRun());
        lock (isDoneLock)
        {
            Assert.False(isDone);
            Result = workFunction(random!, dependencyResults, Args);
            isDone = true;
            foreach (var waiter in Waiters)
            {
                lock (waiter)
                {
                    waiter.GiveResult(this, Result);
                    if (waiter.CanRun())
                        jobGraph.MarkJobRunnable(waiter);
                }
            }

            jobGraph.MarkJobDone(this);
            return Result;
        }
    }

    ///
    /// Returns true if all dependencies have completed.
    ///
    private bool CanRun()
    {
        return dependenciesDone == Dependencies.Count && random != null;
    }

    ///
    /// This method should be called on a waiter of a job that just completed.
    /// Example:
    ///
    ///     A -> B -> C
    ///     A just finished so, B.GiveResult(A, 5);
    ///
    /// This ensures the jobs with dependencies can read the results of its
    /// dependencies in their delegate function.
    ///
    private void GiveResult(Job fromJob, object fromJobResult)
    {
        Assert.False(CanRun());
        lock (dependencyResultsLock)
        {
            var resultIndex = Dependencies.IndexOf(fromJob);

            while (resultIndex >= dependencyResults.Count)
                dependencyResults.Add(new object());

            Assert.LessThan(resultIndex, dependencyResults.Count);
            dependencyResults[resultIndex] = fromJobResult;
            dependenciesDone += 1;
        }
    }

    ///
    /// Returns a pretty print format of the Job that includes its name,
    /// dependencies, and waiters.
    ///
    public override string ToString()
    {
        string depString;
        string waitString;
        if (Dependencies.Count > 0)
            depString = string.Format("[{0}]", string.Join(", ", Dependencies.Select(i => i.Name)));
        else
            depString = "ROOT";

        if (Waiters.Count > 0)
            waitString = string.Format("[{0}]", string.Join(", ", Waiters.Select(i => i.Name)));
        else
            waitString = "TAIL";

        return string.Format("  {0} --> {1} (CanRun:{2})(Done:{3}) --> {4}",
            depString, Name, CanRun(), isDone, waitString);
    }
}