using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundJobCodingChallenge.JobEngine;

public class Constant
{
    public class WorkerProcess
    {
        public const int MaxConcurrency = 10;
        public const int ScalingThreshold = 5;
        public const int RetryLimit = 3;
        public const string FinancialSystemEndpoint = "finacialSystemEndpoint";
    }
}
