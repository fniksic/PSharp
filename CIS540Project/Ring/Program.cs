using System;
using Microsoft.PSharp;

namespace Ring
{
    /// <summary>
    /// An implementation of leader election in a network organized as a
    /// unidirectional ring. In a ring with N nodes, the algorithm implemented
    /// here exchanges at most O(N log(N)) messages.
    /// </summary>
    public class Program
    {
        // The execution of a C# program starts in method Main. Here, we will
        // create and configure a new P# runtime, and execute our P# program on it.
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

        // Our P# program is defined in a separate method annotated with the
        // attribute Microsoft.PSharp.Test. The attribute is there for the P#
        // tester. By annotating the method with this attribute, the P# tester
        // will know what to execute during testing.
        [Microsoft.PSharp.Test]
        public static void Execute(PSharpRuntime runtime)
        {
            // We register two monitors and create a machine that
            // will initialize the ring and start the election.
            runtime.RegisterMonitor(typeof(SingleLeaderElected));
            runtime.RegisterMonitor(typeof(EventuallyLeaderElected));

            // When creating a machine, we can optionally pass it an event
            // that will trigger immediately upon entering the starting state
            // of the machine. This is useful for passing configuration to
            // the machine. Here, we pass the machine an array of unique node
            // identifiers.
            var config = new Ring.Config(new int[]{ 10, 2, 5, 7, 3 });
            runtime.CreateMachine(typeof(Ring), config);
        }
    }
}
