using System;
using Microsoft.PSharp;

namespace Quorum
{
    /// <summary>
    /// An implementation of a simplified version of Fast Leader Election (FLE)
    /// from Apache Zookeeper. To find out more about FLE, read the following
    /// technical report:
    ///
    ///   http://www.tcs.hut.fi/Studies/T-79.5001/reports/2012-deSouzaMedeiros.pdf
    ///
    /// Another useful resource is the actual implementation of FLE:
    ///
    ///   https://git.io/fjftX
    /// 
    /// </summary>
    public class Program
    {
        // As usual, the execution of a C# program starts in the method Main.
        // Here we create and configure a P# runtime, and execute our P# program on it.
        public static void Main(string[] args)
        {
            // Optional: increases verbosity level to see the P# runtime log.
            var configuration = Configuration.Create().WithVerbosityEnabled(1);

            // Creates a new P# runtime instance, and passes an optional configuration.
            var runtime = PSharpRuntime.Create(configuration);

            // Executes the P# program.
            Program.Execute(runtime);

            // The P# runtime executes asynchronously, so we wait
            // to not terminate the process.
            Console.WriteLine("Press Enter to terminate...");
            Console.ReadLine();
        }

        // A method that executes our P# program, annotated with the atribute
        // Microsoft.PSharp.Test, so that the P# tester knows what to execute
        // during testing.
        [Microsoft.PSharp.Test]
        public static void Execute(PSharpRuntime runtime)
        {
            // We register the safety and liveness monitors, which encode
            // the correctness requirements for leader election.
            runtime.RegisterMonitor(typeof(SingleLeaderElected));
            runtime.RegisterMonitor(typeof(EventuallyLeaderElected));

            // We create the initial machine that will initialize a cluster
            // of nodes. We pass it a Config event carrying the size of the cluster.
            int clusterSize = 5;
            runtime.CreateMachine(typeof(Cluster), new Config(clusterSize));
        }
    }
}
