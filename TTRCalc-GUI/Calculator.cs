using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TTRCalc_GUI
{
    enum FacilityType { Sellbot, Cashbot, Lawbot, Bossbot };

    class FacilityItem
    {
        public uint Count { get; set; }
        public string FacilityName { get; set; }
        public FacilityItem(uint count, string facilityName)
        {
            Count = count;
            FacilityName = facilityName;
        }
    }
    class Calculator
    {
        private FacilityType SelectedFacility;
        private bool IncludeMediumFactory;
        private bool ShortsPreferred;

        // Point values associated w/ facilities
        private Dictionary<FacilityType, (string, uint, double)[]> FacilityValues = new Dictionary<FacilityType, (string, uint, double)[]>()
        {
            {
                // Medium factory not included by default
                FacilityType.Sellbot,
                new (string, uint, double)[] {
                    ("Long", 776, 1.75),
                    ("Short", 480, 1.0)
                }
            },
            {
                // Note, cashbot uses avg. values since there is a range of possible points rewarded
                FacilityType.Cashbot,
                new (string, uint, double)[] {
                    ("Bullion Mint", 1349, 2.0),
                    ("Dollar Mint", 842, 1.5),
                    ("Coin Mint", 450, 1.0)
                }
            },
            {
                FacilityType.Lawbot,
                new (string, uint, double)[] {
                    ("D Office", 1842, 2.0),
                    ("C Office", 1370, 1.66),
                    ("B Office", 944, 1.33),
                    ("A Office", 564, 1.0)
                }
            },
            {
                FacilityType.Bossbot,
                new (string, uint, double)[] {
                    ("Back 9", 5120, 3.0),
                    ("Middle 6", 3020, 2.0),
                    ("Front 3", 1120, 1.0)
                }
            }
        };
        public Calculator(FacilityType type, bool includeMediumFactory, bool shortsPreferred)
        {
            SelectedFacility = type;
            IncludeMediumFactory = includeMediumFactory;
            ShortsPreferred = shortsPreferred;
        }

        // Function for calculating and listing the suggested route
        public List<FacilityItem> CalculateList(uint PointsNeeded)
        {
            (string, uint, double)[] PointSources = FacilityValues[SelectedFacility];

            if (SelectedFacility == FacilityType.Sellbot)
            {
                if (IncludeMediumFactory)
                    PointSources = new (string, uint, double)[] { PointSources[0], ("Medium", 584, 1.3), PointSources[1] };
                // Adjust longs to be considered more efficient if the user doesn't prefer doing shorts
                if (!ShortsPreferred)
                    PointSources[0] = ("Long", 776, 1.5);
            }

            uint[] SourceCounts = CalculateMostEfficientRoute(PointSources, PointsNeeded);

            List<FacilityItem> Outputs = new List<FacilityItem>();

            for (int i = 0; i < SourceCounts.Length; ++i)
            {
                if (SourceCounts[i] == 0)
                    continue;
                Outputs.Add(new FacilityItem(SourceCounts[i], $"{PointSources[i].Item1}{(SourceCounts[i] > 1 ? "s" : "")}"));
            }
            return Outputs;
        }

        // The actual recursive function for calculating the route
        private uint[] CalculateMostEfficientRoute((string, uint, double)[] PointSources, uint PointsNeeded)
        {
            // Stores how many of each point source we should do
            uint[] SourceCounts = new uint[PointSources.Length];
            uint PointsRemaining = PointsNeeded;
            // Keep calculating until we don't need any more points
            while (PointsRemaining > 0)
            {
                // Initialize optimized values to 0
                double LowestTime = 0;
                uint BestCount = 0;
                uint BestSourceIndex = 0;
                // Look through all possible point sources to find most efficient route
                for (uint i = 0; i < PointSources.Length; ++i)
                {
                    var PointSource = PointSources[i];
                    uint Points = PointSource.Item2;
                    double Time = PointSource.Item3;
                    double FractionNeeded = (double)PointsRemaining / Points;
                    uint RoundedFraction = (uint)Math.Floor(FractionNeeded); // Ideal amount of runs needed = floor(points remaining / points given)
                    uint CountNeeded = Math.Max(RoundedFraction, 1); // Always require at least 1 run
                    double TimeNeeded = Time * CountNeeded; // Total time for N runs = time * N
                    if (Points * CountNeeded < PointsRemaining) // If we'll still need more points, do a recursive route calculation starting from the current point
                    {
                        uint[] TestCounts = CalculateMostEfficientRoute(PointSources, PointsRemaining - (Points * CountNeeded));
                        // Add the total time needed to do the calculated route to this route's time
                        for (int j = 0; j < TestCounts.Length; ++j)
                            TimeNeeded += TestCounts[j] * PointSources[j].Item3;
                    }
                    // If this is the quickest route, set the relevant variables
                    if (TimeNeeded < LowestTime || LowestTime == 0)
                    {
                        LowestTime = TimeNeeded;
                        BestCount = CountNeeded;
                        BestSourceIndex = i;
                    }
                };
                SourceCounts[BestSourceIndex] += BestCount;
                uint PointsEarned = PointSources[BestSourceIndex].Item2 * BestCount;
                if (PointsEarned > PointsRemaining)
                    break;
                else
                    PointsRemaining -= PointsEarned;
            };
            return SourceCounts;
        }
    }
}
