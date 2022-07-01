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

        // Point values associated w/ facilities
        private static Dictionary<FacilityType, (string, uint, double)[]> FacilityValues = new Dictionary<FacilityType, (string, uint, double)[]>()
        {
            {
                FacilityType.Sellbot,
                new (string, uint, double)[] {
                    ("Long", 776, 1.5),
                    //("Medium", 584, 2.0), Commenting this out for now because who does medium factory runs lol
                    ("Short", 480, 1.0)
                }
            },
            {
                FacilityType.Cashbot,
                new (string, uint, double)[] {
                    ("Bullion Mint", 1202, 2.0),
                    ("Dollar Mint", 679, 1.5),
                    ("Coin Mint", 356, 1.0)
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
        public Calculator(FacilityType type)
        {
                SelectedFacility = type;
        }

        // Function for calculating and listing the suggested route
        public List<FacilityItem> CalculateList(uint PointsNeeded)
        {
            (string, uint, double)[] PointSources = FacilityValues[SelectedFacility];

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
            uint[] SourceCounts = new uint[PointSources.Length];
            uint PointsRemaining = PointsNeeded;
            while (PointsRemaining > 0)
            {
                double LowestTime = 0;
                uint BestCount = 0;
                uint BestSourceIndex = 0;
                for (uint i = 0; i < PointSources.Length; ++i)
                {
                    var PointSource = PointSources[i];
                    uint Points = PointSource.Item2;
                    double Time = PointSource.Item3;
                    double FractionNeeded = ((double)PointsRemaining / (double)Points);
                    uint RoundedFraction = (uint)Math.Floor(FractionNeeded);
                    uint CountNeeded = Math.Max(RoundedFraction, 1);
                    double TimeNeeded = Time * CountNeeded;
                    if (Points * CountNeeded < PointsRemaining)
                    {
                        uint[] TestCounts = CalculateMostEfficientRoute(PointSources, PointsRemaining - (Points * CountNeeded));
                        for (int j = 0; j < TestCounts.Length; ++j)
                            TimeNeeded += TestCounts[j] * PointSources[j].Item3;
                    }
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
