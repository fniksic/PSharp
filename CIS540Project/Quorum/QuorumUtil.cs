using System;
using System.Collections.Generic;

namespace Quorum
{
    internal class QuorumUtil
    {
        // CheckQuorum returns true if CandidateId has a majority of votes in the system.
        public static bool CheckQuorum(Dictionary<int, int> ElectionVotes, int CandidateId, int ClusterSize)
        {
            int count = 0;
            foreach (int currId in ElectionVotes.Values)
            {
                if (currId == CandidateId)
                {
                    count++;
                }
            }
            return count > ClusterSize / 2;
        }
    }
}