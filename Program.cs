using System;
using Newtonsoft.Json;

namespace OrToolsPickListSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new TestDataGenerator(new TestDataGeneratorOptions
            {
                NumPickLists = 5,
                MaxItemsPerPickList = 6,
                MaxPickListItemQuantity = 200,
                NumContainers = 20,
                MaxItemsPerContainer = 12,
                MaxContainerItemQuantity = 500,
                TotalItemTypes = 50
            });

            var testData = generator.Generate();

            var solver = new PickListSolver(testData.PickLists, testData.Containers);

            var result = solver.Solve();

            Console.WriteLine(result.ResultStatus);
            Console.WriteLine(JsonConvert.SerializeObject(result.PickLists, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(result.Containers, Formatting.Indented));
        }
    }
}
