﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TTRCalc_GUI
{
    enum FacilityType { Sellbot, Cashbot, Lawbot, Bossbot };

    struct FacilityItem
    {
        /// <summary>
        /// How many of this facility should be done.
        /// </summary>
        public uint Count { get; set; }
        /// <summary>
        /// The "pretty" name of the facility.
        /// </summary>
        public string FacilityName { get; set; }
        public FacilityItem(uint count, string facilityName)
        {
            Count = count;
            FacilityName = facilityName;
        }
    }

    struct Facility
    {
        /// <summary>
        /// The name of the facility.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The (average) points rewarded by the facility.
        /// </summary>
        public uint Points { get; set; }
        /// <summary>
        /// The (average) time it takes to complete the facility.
        /// </summary>
        public TimeSpan Time { get; set; }
        public Facility(string name, uint points, TimeSpan time)
        {
            Name = name;
            Points = points;
            Time = time;
        }
    }

    class Calculator
    {
        public FacilityType SelectedFacilityType { get; set; }
        public bool ShortsPreferred { get; set; }
        public bool IncludeBuildings { get; set; }

        // Point values associated w/ Cog HQ facilities; dependent on type
        private readonly Dictionary<FacilityType, Facility[]> HQFacilityValues = new Dictionary<FacilityType, Facility[]>()
        {
            // Since Under New Management, all facility payout values and times are now averages
            {
                // TODO: Update sellbot for UNM
                FacilityType.Sellbot,
                new Facility[] {
                    new Facility("Long Steel Factory", 1549, TimeSpan.FromSeconds(700)),
                    new Facility("Short Steel Factory", 992, TimeSpan.FromSeconds(500)),
                    new Facility("Long Scrap Factory", 629, TimeSpan.FromSeconds(600)),
                    new Facility("Short Scrap Factory", 403, TimeSpan.FromSeconds(400))
                }
            },
            {
                FacilityType.Cashbot,
                new Facility[] {
                    new Facility("Bullion Mint", 1700, TimeSpan.FromSeconds(900)),
                    new Facility("Coin Mint", 735, TimeSpan.FromSeconds(720))
                }
            },
            {
                FacilityType.Lawbot,
                new Facility[] {
                    new Facility("Senior Wing", 1950, TimeSpan.FromSeconds(1260)),
                    new Facility("Junior Wing", 808, TimeSpan.FromSeconds(860)),
                }
            },
            {
                FacilityType.Bossbot,
                new Facility[] {
                    new Facility("Final Fringe", 2240, TimeSpan.FromSeconds(1980)),
                    new Facility("First Fairway", 925, TimeSpan.FromSeconds(1200)),
                }
            }
        };

        // Values for cog buildings; independent of type
        private readonly Facility[] BuildingValues =
        {
            new Facility("Max 5 Story Bldg", 1000, TimeSpan.FromSeconds(440)),
            new Facility("5 Story Bldg", 500, TimeSpan.FromSeconds(360)),
            new Facility("4 Story Bldg", 200, TimeSpan.FromSeconds(300)),
        };

        // Function for calculating and listing the suggested route
        public List<FacilityItem> CalculateList(uint PointsNeeded)
        {
            Facility[] pointSources = (Facility[])HQFacilityValues[SelectedFacilityType].Clone();

            // Include building values if user specified
            if (IncludeBuildings)
                pointSources = pointSources.Concat(BuildingValues).OrderByDescending(x => x.Points).ToArray();

            if (SelectedFacilityType == FacilityType.Sellbot)
            {
                // Adjust shorts to be considered more efficient if the user prefers them
                if (ShortsPreferred)
                {
                    for (int i = 0; i < pointSources.Length; ++i)
                        if (pointSources[i].Name.Contains("Short"))
                            pointSources[i].Time = TimeSpan.FromMinutes(pointSources[i].Time.TotalMinutes / 2);
                }
            }

            // TOONFEST: Multiply all source points by 2
            //for (int i = 0; i < PointSources.Length; ++i)
            //PointSources[i].Points *= 2;

            uint[] SourceCounts = CalculateMostEfficientRoute(pointSources, PointsNeeded);

            List<FacilityItem> Outputs = new List<FacilityItem>();

            for (int i = 0; i < SourceCounts.Length; ++i)
            {
                if (SourceCounts[i] == 0)
                    continue;
                Outputs.Add(new FacilityItem(SourceCounts[i], $"{pointSources[i].Name}{(SourceCounts[i] > 1 ? "s" : "")}"));
            }
            return Outputs;
        }

        // The actual recursive function for calculating the route
        private uint[] CalculateMostEfficientRoute(Facility[] PointSources, uint PointsNeeded)
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
                    uint Points = PointSource.Points;
                    TimeSpan Time = PointSource.Time;
                    double FractionNeeded = (double)PointsRemaining / Points;
                    uint RoundedFraction = (uint)Math.Floor(FractionNeeded); // Ideal amount of runs needed = floor(points remaining / points given)
                    uint CountNeeded = Math.Max(RoundedFraction, 1); // Always require at least 1 run
                    double TimeNeeded = Time.TotalSeconds * CountNeeded; // Total time for N runs = time * N
                    if (Points * CountNeeded < PointsRemaining) // If we'll still need more points, do a recursive route calculation starting from the current point
                    {
                        uint[] TestCounts = CalculateMostEfficientRoute(PointSources, PointsRemaining - (Points * CountNeeded));
                        // Add the total time needed to do the calculated route to this route's time
                        for (int j = 0; j < TestCounts.Length; ++j)
                            TimeNeeded += TestCounts[j] * PointSources[j].Time.TotalSeconds;
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
                uint PointsEarned = PointSources[BestSourceIndex].Points * BestCount;
                if (PointsEarned > PointsRemaining)
                    break;
                else
                    PointsRemaining -= PointsEarned;
            };
            return SourceCounts;
        }
    }
}
