using System;
using System.Diagnostics;
using OrToolsPickListSolver.Models;
using static Google.OrTools.LinearSolver.Solver;

namespace OrToolsPickListSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var pickListQuantities = new short[] { 10 };
            var containerQuantities = new short[] { 100, 200, 300, 400, 500 };

            foreach (var plq in pickListQuantities)
            {
                foreach (var cq in containerQuantities)
                {
                    var (gotSolution, elapsed) = RunDiagnostic(plq, cq);
                    Console.WriteLine($"# pick lists: {plq}; # containers: {cq}; got solution: {gotSolution}; elapsed: {elapsed}");
                }
            }
        }

        private static (bool, TimeSpan) RunDiagnostic(short numPickLists, short numContainers)
        {
            var generator = new TestDataGenerator(new TestDataGeneratorOptions
            {
                NumPickLists = numPickLists,
                MaxItemsPerPickList = 50,
                MaxPickListItemQuantity = 200,
                NumContainers = numContainers,
                MaxItemsPerContainer = 25,
                MaxContainerItemQuantity = 1000,
                TotalItemTypes = 50
            });

            var testData = generator.Generate();

            var sw = new Stopwatch();

            sw.Start();

            PickListSolverResult result = null;
            
            var retries = 0;
            do 
            {
                var solver = new PickListSolver(testData.PickLists, testData.Containers, 30);
                result = solver.Solve();
                retries++;
            } while (result.ResultStatus != ResultStatus.OPTIMAL
                && result.ResultStatus != ResultStatus.FEASIBLE
                && retries < 3);

            sw.Stop();

            return (
                result.ResultStatus == ResultStatus.OPTIMAL || result.ResultStatus == ResultStatus.FEASIBLE,
                sw.Elapsed
            );
        }
    }
}
