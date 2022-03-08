using System;
using System.Collections.Generic;
using System.Linq;

///
/// Represents a group of jobs that can be optionally tagged. These provide an
/// easily interface to create large JobGraphs.
///
public class JobGroup
{
    public IEnumerable<Job> Jobs => jobs;

    public IEnumerable<Job> Roots => jobs.Where(i => i.Dependencies.Count == 0);

    public IEnumerable<Job> Tails => jobs.Where(i => i.Waiters.Count == 0);

    public Dictionary<string, IEnumerable<Job>> Tags { get; }

    private readonly HashSet<Job> jobs;

    ///
    /// Creates a new JobGroup from a given Dictionary. The format of the
    /// Dictionary is as follows:
    ///
    ///     Key (the task)(valid types):
    ///       - Job
    ///       - JobGroup
    ///     Value (the dependency)(valid types):
    ///       - Array[Job or JobGroup]
    ///
    /// When you assign the Key or Value of the dictionary to another JobGroup,
    /// then the respective Roots and Tails will be updated correctly with
    /// regard to the dependency you are trying to create. Example:
    ///
    ///     JobGroup(
    ///         { JobGroup({ A, B, C }), { D } }
    ///     )
    ///
    ///     In this example, tasks { A, B, C } are each given A as a dependency,
    ///     and A is given { A, B, C } as waiters. This format works
    ///     respectively is the Key & Value were swapped.
    ///
    /// JobGroups can also be given an optional tag, which is propagated if
    /// any keys/values is the dictionary are JobGroups too. Tags can be used by
    /// JobGraph to wait for JobGroups to complete.
    ///
    public JobGroup(Dictionary<object, object[]> jobMatrix, string? tag = null)
    {
        jobs = new HashSet<Job>();
        Tags = new Dictionary<string, IEnumerable<Job>>();
        foreach (var (seconds, firsts) in CastMatrix(jobMatrix))
        {
            foreach (var secondJob in seconds!)
            foreach (var firstJob in firsts!)
            {
                firstJob.Waiters.Add(secondJob);
                secondJob.Dependencies.Add(firstJob);
            }
        }

        if (tag == null)
            return;
        Tags.Add(tag, Tails);
    }

    ///
    /// This function returns a Pairs of Jobs that need to have their
    /// job.Dependencies and job.Waiters updated.
    /// 
    /// Since the dictionary supports a variety of different types, this method
    /// parses the dictionary into one type first. Each job it finds is
    /// automatically added to the jobs Hashset. Any JobGroups found also have
    /// their tags HashSet consolidated into this one.
    ///
    private IEnumerable<ValueTuple<IEnumerable<Job>?, IEnumerable<Job>?>>
        CastMatrix(Dictionary<object, object[]> jobMatrix)
    {
        var deps = new List<ValueTuple<IEnumerable<Job>?, IEnumerable<Job>?>>();
        foreach (var value in jobMatrix)
        {
            var firsts = value.Key;
            var seconds = value.Value;

            if (firsts == null || seconds == null)
            {
                continue;
            }

            // Convert First Jobs
            var firstJobs = new List<Job>();
            IEnumerable<object> firstsArray;
            if (firsts is Array)
                firstsArray = (object[]) firsts;
            else
                firstsArray = new[] {firsts};
            foreach (var jobObject in firstsArray)
            {
                switch (jobObject)
                {
                    case JobGroup jg:
                    {
                        jg.Tags.ToList().ForEach(x => Tags.Add(x.Key, x.Value));
                        firstJobs.AddRange(jg.Tails);
                        foreach (var jgJob in jg.Jobs)
                        {
                            jobs.Add(jgJob);
                        }

                        break;
                    }
                    case Job job:
                        firstJobs.Add(job);
                        jobs.Add(job);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            // Convert Second Jobs
            var secondJobs = new List<Job>();
            foreach (var jobObject in seconds)
            {
                switch (jobObject)
                {
                    case JobGroup jg:
                    {
                        jg.Tags.ToList().ForEach(x => Tags.Add(x.Key, x.Value));
                        secondJobs.AddRange(jg.Roots);
                        foreach (var jgJob in jg.Jobs)
                        {
                            jobs.Add(jgJob);
                        }

                        break;
                    }
                    case Job job:
                        secondJobs.Add(job);
                        jobs.Add(job);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            deps.Add((firstJobs, secondJobs));
        }

        return deps;
    }
}