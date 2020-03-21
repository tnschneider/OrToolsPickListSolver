using System;
using System.Diagnostics;
using System.Linq;
using OrToolsPickListSolver.Models;
using static Google.OrTools.LinearSolver.Solver;

namespace OrToolsPickListSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var pickListCounts = new short[] { 5, 10, 25 };
            var containerCounts = new short[] { 100, 200, 500 };

            foreach (var plc in pickListCounts)
            foreach (var cc in containerCounts)
            for (var i = 0; i < 3; i++)
            {
                var (solution, pctMatched, elapsed) = RunDiagnostic(plc, cc);
                Console.WriteLine($"# pick lists: {plc}; # containers: {cc}; solution: {solution}; matched: {pctMatched:0.0}%; elapsed: {elapsed:0.00}s;");
            }
        }

        private static (string, double, double) RunDiagnostic(short numPickLists, short numContainers)
        {
            var generator = new TestDataGenerator(new TestDataGeneratorOptions
            {
                NumPickLists = numPickLists,
                MaxItemsPerPickList = 50,
                MaxPickListItemQuantity = 200,
                NumContainers = numContainers,
                MaxItemsPerContainer = 25,
                MaxContainerItemQuantity = 1000,
                TotalItemTypes = 100,
                ReplenishmentPct = 0.75
            });

            var testData = generator.Generate();

            var sw = new Stopwatch();

            sw.Start();

            PickListSolverResult result = null;
            
            var retries = 1;
            do 
            {
                var solver = new PickListSolver(testData.PickLists, testData.Containers, 10 * retries);
                result = solver.Solve();
                retries++;
            } while (result.ResultStatus != ResultStatus.OPTIMAL
                && result.ResultStatus != ResultStatus.FEASIBLE
                && retries <= 3);

            sw.Stop();

            var items = result.PickLists.SelectMany(x => x.Items).Select(x => {
                return new { OrderQuantity = x.Orders?.Sum(y => y.Quantity) ?? 0, Quantity = x.Quantity };
            });
            
            var pctMatched = (1.0 * items.Sum(x => x.OrderQuantity))
                / (1.0 * items.Sum(x => x.Quantity)) * 100.0;

            var resultStatus = result.ResultStatus == ResultStatus.OPTIMAL
                ? "Optimal"
                : (result.ResultStatus == ResultStatus.FEASIBLE
                    ? "Feasible"
                    : "None");

            return (
                resultStatus,
                pctMatched,
                sw.Elapsed.TotalMilliseconds / 1000.0
            );
        }
    }
}
