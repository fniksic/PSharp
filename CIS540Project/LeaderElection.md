CIS 540 Project: Leader Election in P#
===

In the course so far, among other things we have discussed the _leader election_ problem: the nodes in a network need to decide on a unique leader by exchanging messages. More precisely, each node should eventually reach a decision either to lead or to follow, and exactly one node should decide to lead.

The solution to the problem (or lack thereof) depends on what assumptions we make about the network and the knowledge of the nodes. The basic assumption is that there needs to be a way to break the symmetry among the nodes, and for that we assumed that each node has a unique identifier. By comparing the identifiers, the nodes can elect the one with the highest identifier. Additional assumptions can be about whether the network is synchronous or asynchronous, whether each node is connected to all or only a subset of other nodes, whether the connections are directed or undirected, whether the nodes know the size of the network, and whether the nodes or the network can fail. If the network is synchronous and strongly connected, we saw the problem can be solved using the flooding algorithm, but the nodes need to know an upper bound on the number of nodes. If the network is asynchronous and the nodes are organized as a unidirectional ring, we saw the problem can be solved even if the nodes do not know the number of other nodes.

In this project we will study concrete implementations of leader election algorithms in Microsoft P#. P# is a framework for building and testing asynchronous software systems. It provides a programming model similar to the asynchronous model from the lectures, a programming language that extends C# and is called P# like the framework, a way to specify safety and liveness monitors, and a systematic testing engine that acts as a scheduler and systematically explores different executions of a P# program in order to find safety and liveness violations.

How to Start
---

In order to start with the project, you should clone the following Git repository and switch to the branch called _CIS540_.
```bash
git clone https://github.com/fniksic/PSharp
cd PSharp
git checkout CIS540
```
There is some setup necessary to make everything work. You can follow the instructions in the file `CIS540Project/Setup.md` (the path is relative to the root of the PSharp repository). Alternatively, I will most likely provide a virtual machine with everything prepared.

To familiarize yourself with P#, start by reading the file `Docs/README.md` and follow the links to the additional topics. The most relevant topics can be found in `Docs/Overview.md`, `Docs/WriteFirstProgram.md`, `Docs/Features/SafetyLivenessProperties.md`, and `Docs/Testing/TestingMethodology.md`. The best way to browse the documentation is with your Internet browser directly in the Github repo.

Note that the documentation mostly assumes you are working on Windows. Some steps involving building and running your programs may differ on other operating systems. I use macOS and the verbatim commands in this document will apply to macOS and Linux; I assume you will know how to execute analogous commands if you work on Windows. I have tried to make the project independent of the operating system, but technical problems are always possible. If you get stuck, ask on Piazza and we will resolve the issues.

In case you need to get familiar with C#, there is a wealth of resources on <https://docs.microsoft.com/en-us/dotnet/csharp/>.

Ring
---

As a warmup, we will implement the algorithm from the lectures for leader election in the ring topology. The communication in the ring is asynchronous, and the ring is directed: we say that a node receives messages from the node to its left, and it sends messages to the node to its right.

Recall that in the simpler version of the algorithm, the nodes start by sending their identifier to the right. Each time a node receives an identifier from the left, it compares it to its current identifier. If the received identifier is higher, it stores it as its current identifier and sends it to the right. If the received identifier equals the current identifier, this is a signal that a node can make a decision. It decides to lead if its original identifier is the same as the current identifier, otherwise it decides to follow. In either case it needs to propagate the received identifier one more time so that other nodes can decide.

The simpler version of the algorithm is implemented in the folder `CIS540Project/SimpleRing`. I recommend opening the folder as a workspace in Visual Studio Code, but you may also use the editor of your choice to view and edit the files. The file `Program.cs` contains the function `Main`, which sets up the P# runtime and creates a P# machine of type `Ring`. This machine creates and configures the machines of type `Node`, which implement the logic of the algorithm. There are two other machines registered as monitors: `SingleLeaderElected` and `EventuallyLeaderElected`. These machines specify the correctness of the algorithm; the former specifies the election safety requirement---that at most one leader is elected---and the latter specifies the election liveness requirement---that a leader is eventually elected. Each machine is implemented in a file with the corresponding name and the extension `.psharp`.

Your first task will be to study the code to familiarize yourself with the syntax and the structure of a P# program. Then you will build the program, run it to see the output, and test it with `PSharpTester`.

Once you understand `SimpleRing`, we are ready to proceed to the more interesting version of the algorithm in which $n$ nodes are guaranteed to exchange at most $O(n \log n)$ messages.
Recall that the algorithm works in phases. At the beginning of each phase, a node is either a follower or it is undecided. A follower acts as a relay: it relays a message received from the node to its left to the node to its right. An undecided node receives identifiers from two undecided nodes to its left (possibly through a sequence of relays); let us call these nodes the left node and the leftmost node. If the left node's identifier equals the receiving node's current identifier, that means the receiving node is the only undecided node in the ring, so it elects itself as the leader. Otherwise the receiving node compares the two received identifiers to its current identifier. If the left node's identifier is the highest of the three, the receiving node stores it as its current identifier, sends it to the node to its right, and proceeds to the next phase as an undecided node. Otherwise the receiving node becomes a follower. Initially all nodes are undecided and they initiate the protocol by sending their identifiers to the right.

The initial implementation is located in the folder `CIS540Project/Ring`. As before, I recommend opening the folder as a workspace in Visual Studio Code. The structure of the project is the same as for `SimpleRing`.

**Your task (10 points):**

1. Study the code of `SimpleRing` to familiarize yourself with the syntax and the structure of a P# program.

2. Build the program and run it. Then test it with `PSharpTester`. The tester should report that there are no bugs in the program.

3. Now study the code of `Ring`. Everything is the same as in `SimpleRing`, except for the file `Node.psharp`.

2. The key part of the $O(n \log n)$ algorithm described above is missing. Specifically, look at the method `HandleMessage()` in the file `Node.psharp` and add the missing part.

3. Test the program with `PSharpTester`. The tester should report that there are no bugs in the program.

**Technical instructions:**

1. For either `SimpleRing` or `Ring`, if you open the corresponding folder in Visual Studio Code, you can build the program by executing `Terminal->Run Build Task...`. You can run the program by executing `Debug->Start Debugging`.

2. You can build the program in the terminal by executing `dotnet build` in the program's folder. You can run it by executing `dotnet run` or `dotnet bin/Debug/netcoreapp2.1/Ring.dll`.

3. To test the program, you need to run the tester utility from the terminal. The tester is the executable file `bin/netcoreapp2.1/PSharpTester.dll`. If you are located in the folder `CIS540Project/SimpleRing` (the same goes for `CIS540Project/Ring`), execute the following:
   ```bash
   dotnet ../../bin/netcoreapp2.1/PSharpTester.dll \
     -test:bin/Debug/netcoreapp2.1/Ring.dll \
     -sch:random \
     -i:10
   ```
   This will execute 10 schedules of the program using the random scheduler. If the tester finds a bug in the program, it will store the buggy schedule, the program trace, and the log file in the folder `./bin/Debug/netcoreapp2.1/Output`. To see what other options are available, run the tester with the option `-?`.

Quorum
---

Classical computer science approach to algorithm design assumes that the machine executing the algorithm will perform every step of the algorithm correctly. The assumption is justified for a single machine: after all, modern computers are quite robust and they rarely fail. However, in a distributed system with many nodes, even if the probability that a specific node will fail is negligible, since the individual failures are independent, the probability that _some_ node will fail becomes sufficiently large and it needs to be taken into account. To quote a highly influential distributed systems researcher Leslie Lamport: "A distributed system is one in which the failure of a computer you didn’t even know existed can render your own computer unusable."

The algorithm from the previous section clearly does not tolerate failures: even if a single node fails, the system cannot make progress. In this part of the project, we will look an attempt at a fault-tolerant leader election algorithm. The algorithm is a simplified version of the Fast Leader Election algorithm from Apache Zookeeper.

The algorithm assumes a fully connected network of nodes. Additionally, it assumes that each node knows the total number of nodes in the system. Intuitively, the nodes will try to reach a _quorum_---a situation in which a majority of nodes (i.e., more than a half) decides on the same leader. The idea behind this is that even if some nodes fail, there is still a majority that will continue making progress and finally elect a leader. Theoretically, if there are $2f+1$ nodes in the system, the algorithm should tolerate failures of up to $f$ nodes.

In order to reach a quorum, the nodes execute the following protocol. Initially, they propose themselves as leaders and broadcast their votes to their peers. While a node is undecided, each time it receives a vote from an undecided peer, it updates its vote if the peer proposed a leader with a higher identifier. If they receive a vote from a peer that has already decided whom to elect, they blindly believe them and update their vote correspondingly even if the peer voted for a node with a lower identifier. If a node ever updates its vote, it  broadcasts the new vote to its peers. During the execution, each node maintains a view of its knowledge about the votes of its peers. If at any point a node detects that according to its view the nodes have reached a quorum, it decides on the leader supported by the quorum. In the decided state, the node never updates its vote, and thus it never broadcasts it. However, if it receives a vote from an undecided peer, it informs the peer of its decision.

The described algorithm is implemented in the folder `CIS540Project/Quorum`. As before, the execution starts in the file `Program.cs` in the function `Main`. The function initializes a P# runtime and starts a machine of type `Cluster`, which in turn starts several machines of type `Node`. There are again two monitors---`SingleLeaderElected` and `EventuallyLeaderElected`---to ensure election safety and progress.

**Your task (15 points):**

1. Study the code to understand how it works. Then try to test the program with the P# tester:
   ```bash
   dotnet ../../bin/netcoreapp2.1/PSharpTester.dll \
     -test:bin/Debug/netcoreapp2.1/Quorum.dll \
     -i:10
   ```
   You should observe that it quickly fails. Analyze the output produced by the tester---you are likely to see that two nodes declared themselves as the leader.

2. The reason for the failure is that our monitors do not properly encode our new correctness requirement. Recall that a node is the leader only if it is supported by a quorum. A scenario that should be allowed is that one node elects itself and fails, and the other two proceed by electing a leader among themselves.

   Update the monitors to reflect the updated correctness requirement. In `SingleLeaderElected` simply comment out the assertion since we want to allow multiple nodes to declare themselves as leaders. In `EventuallyLeaderElected` we do not want to transition to the "cold" state as soon as we see the first node declaring themselves as the leader. Instead, the monitor should only transition if the self-declared leader is supported by a quorum.

3. Is the program now correct? Try to increase the number of nodes to 5 and test it more thoroughly, with higher number of iterations. Instead of the random scheduler, you may want to try FairPCT with the priority switch bound of 4:
   ```bash
   dotnet ../../bin/netcoreapp2.1/PSharpTester.dll \
     -test:bin/Debug/netcoreapp2.1/Quorum.dll \
     -i:1000 \
     -sch:fairpct:4 \
     -max-steps:200
   ```
   When you find a bug, analyze the log file. Try to understand what went wrong.

**Optional tasks:**

4. As the previous task shows, there is a reason for the added complexity of the complete Fast Leader Election. You can read about the complete algorithm in a technical report by André Medeiros: [ZooKeeper's atomic broadcast protocol: Theory and practice](http://www.tcs.hut.fi/Studies/T-79.5001/reports/2012-deSouzaMedeiros.pdf). If you feel ambitious, implement the complete Fast Leader Election and test it with the P# tester. You may want to consult the [source code of Apache Zookeeper](https://github.com/apache/zookeeper/blob/master/zookeeper-server/src/main/java/org/apache/zookeeper/server/quorum/FastLeaderElection.java).

5. The nodes in our program do not exactly fail. Try to model node crashes using an additional machine that will nondeterministically send `halt` events to the nodes. You may find timers useful for this (see `Docs/Features/Timers.md`), as well as machine methods `bool Random()` and `int Random(int maxValue)`.

6. One might ask: if the nodes in the original program never fail, then why should we ever see nodes deciding they are leaders without a quorum? Strengthen the monitors to also require that in the case of no failures a single node should be elected and supported by all nodes in the system. Test the complete Fast Leader Election with this specification. Can you find bugs?

Completing the Project
---

Once you are done with the project, send me the changes you made to the source code. The most elegant way to do this is to commit your changes in git and generate a diff:

```bash
git commit -am "Solve all tasks"
git diff cis540-start --no-prefix > solution.patch
```

Along with the patch file, send me the final buggy traces and log files produced by `PSharpTester`.

We will arrange 10--15 minute meetings where you will have a chance to explain your solutions and demonstrate how you test the programs with `PSharpTester`. The demonstration will be worth another 15 points, for a total of possible 40 points for the project.

The project is due on April 1, 2019.
