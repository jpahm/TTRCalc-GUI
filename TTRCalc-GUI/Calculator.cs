using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TTRCalc_GUI
{
    public enum FacilityType { Sellbot, Cashbot, Lawbot, Bossbot };

    struct FacilityRoute
    {
        public string Name { get; set; }
        public uint Points { get; set; }
        public double TimeFactor { get; set; }
    } 

    class Calculator
    {
        private MainWindow CalculatorWindow;

        // Point values associated w/ facilities
        private Dictionary<FacilityType, FacilityRoute[]> FacilityRoutes = new Dictionary<FacilityType, FacilityRoute[]>()
        {
            {
                // Medium factory not included by default
                FacilityType.Sellbot,
                new[] {
                    new FacilityRoute() { Name = "Long", Points = 776, TimeFactor = 1.75 },
                    new FacilityRoute() { Name = "Short", Points = 480, TimeFactor = 1 },
                }
            },
            {
                // Note, cashbot uses avg. values since there is a range of possible points rewarded
                FacilityType.Cashbot,
                new[] {
                    new FacilityRoute() { Name = "Bullion Mint", Points = 1349, TimeFactor = 2.0 },
                    new FacilityRoute() { Name = "Dollar Mint", Points = 842, TimeFactor = 1.5 },
                    new FacilityRoute() { Name = "Coin Mint", Points = 450, TimeFactor = 1.0 }
                }
            },
            {
                FacilityType.Lawbot,
                new[] {
                    new FacilityRoute() { Name = "D Office", Points = 1842, TimeFactor = 2.0 },
                    new FacilityRoute() { Name = "C Office", Points = 1370, TimeFactor = 1.66 },
                    new FacilityRoute() { Name = "B Office", Points = 944, TimeFactor = 1.33 },
                    new FacilityRoute() { Name = "A Office", Points = 564, TimeFactor = 1.0 }
                }
            },
            {
                FacilityType.Bossbot,
                new[] {
                    new FacilityRoute() { Name = "Back 9", Points = 5120, TimeFactor = 3.0 },
                    new FacilityRoute() { Name = "Middle 6", Points = 3020, TimeFactor = 2.0 },
                    new FacilityRoute() { Name = "Front 3", Points = 1120, TimeFactor = 1.0 }
                }
            }
        };

        public Calculator(MainWindow calcWindow)
        {
            CalculatorWindow = calcWindow;
        }

        // Function for calculating and listing the suggested route
        public Dictionary<FacilityRoute, uint> CalculateList(uint PointsNeeded)
        {
            FacilityRoute[] PointSources = FacilityRoutes[CalculatorWindow.SelectedFacility];

            if (CalculatorWindow.SelectedFacility == FacilityType.Sellbot)
            {
                if (CalculatorWindow.MediumFactoryBox.IsChecked ?? false)
                    PointSources = new FacilityRoute[] { PointSources[0], new FacilityRoute() { Name = "Medium", Points = 584, TimeFactor = 1.3 }, PointSources[1] };
                // Adjust longs to be considered more efficient if the user doesn't prefer doing shorts
                if (!(CalculatorWindow.ShortsPreferredBox.IsChecked ?? false))
                    PointSources[0] = new FacilityRoute() { Name = "Long", Points = 776, TimeFactor = 1.5 };
            }

            Dictionary<FacilityRoute, uint> OptimalRoute = new Dictionary<FacilityRoute, uint>();
            CalculateMostEfficientRoute(PointSources, PointsNeeded, new Dictionary<FacilityRoute, uint>(), ref OptimalRoute);

            return OptimalRoute;
        }

        // The actual recursive function for calculating the route
        // **NOTE: I'm well aware that this doesn't need to be calculated recursively, and could instead take
        // advantage of a multitude of convenient (more efficient) shortcuts... I wanted to do it this way anyways :)
        private void CalculateMostEfficientRoute(FacilityRoute[] Routes, uint PointsNeeded, Dictionary<FacilityRoute, uint> CurrentRoute, ref Dictionary<FacilityRoute, uint> OptimalRoute)
        {
            // Make sure we stop recursing if no more points are needed
            if (PointsNeeded <= 0)
            {
                OptimalRoute = CurrentRoute;
                return;
            }

            // Look through all possible routes to find most efficient route for this recursive iteration
            for (int i = 0; i < Routes.Length; ++i)
            {
                // The specific facility route being considered at this step
                FacilityRoute route = Routes[i];
                
                // Make temporary var for the route being considered, based on the current route
                Dictionary<FacilityRoute, uint> tempRoute = new Dictionary<FacilityRoute, uint>(CurrentRoute);
                
                // Figure out how many times we can do this route without exceeding needed points, with at least 1
                uint numNeeded = Math.Max(1, (uint)Math.Floor((double)PointsNeeded / route.Points));
                
                // Calculate how many points still need to be earned after this
                int pointsRemaining = (int)(PointsNeeded - (numNeeded * route.Points));

                // Add this calc to the temp route
                if (tempRoute.ContainsKey(route))
                    tempRoute[route] += numNeeded;
                else
                    tempRoute[route] = numNeeded;

                if (pointsRemaining > 0)
                {
                    // Recursively calculate optimal route from this point if there are points remaining...
                    CalculateMostEfficientRoute(Routes, (uint)pointsRemaining, tempRoute, ref OptimalRoute);
                } else
                {
                    // Calculate the time cost of the current finished route
                    double timeCost = tempRoute.Sum(x => x.Key.TimeFactor * x.Value);
                    // Calculate the time cost of the current optimal route
                    double optimalTimeCost = OptimalRoute.Sum(x => x.Key.TimeFactor * x.Value);
                    // Compare and set new optimal route if current route is better
                    if (OptimalRoute.Count == 0 || timeCost < optimalTimeCost)
                        OptimalRoute = tempRoute;
                }
            }
        }
    }
}
