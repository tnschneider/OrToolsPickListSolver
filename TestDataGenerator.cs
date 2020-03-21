using System;
using System.Collections.Generic;
using System.Linq;
using OrToolsPickListSolver.Models;
using OrToolsPickListSolver.RandomNumbers;

namespace OrToolsPickListSolver
{
    public class TestDataGenerator
    {
        private readonly static Random _uniformRandom = new Random();
        private readonly static string[] _allItems = TestDataGenerator.AllItems();
        private readonly string[] _availableItems;
        private readonly TestDataGeneratorOptions _options;
        private readonly RandomKumaraswamy _pickListItemCountRandom;
        private readonly RandomKumaraswamy _pickListQuantityRandom;
        private readonly RandomKumaraswamy _containerItemCountRandom;
        private readonly RandomKumaraswamy _containerQuantityRandom;
        

        public TestDataGenerator(TestDataGeneratorOptions options)
        {
            _options = options;
            _availableItems = GetRandomItems(_allItems, _options.TotalItemTypes);
            
            _pickListItemCountRandom = new RandomKumaraswamy(1, options.MaxItemsPerPickList);
            _pickListQuantityRandom = new RandomKumaraswamy(1, options.MaxPickListItemQuantity);

            _containerItemCountRandom = new RandomKumaraswamy(1, options.MaxItemsPerContainer);
            _containerQuantityRandom = new RandomKumaraswamy(1, options.MaxContainerItemQuantity);
        }

        public TestDataGeneratorResults Generate()
        {
            return new TestDataGeneratorResults
            {
                PickLists = GeneratePickLists(),
                Containers = GenerateContainers()
            };
        }

        private T[] GetRandomItems<T>(T[] array, int n)
        {
            return array.OrderBy(x => _uniformRandom.Next()).Take(n).ToArray();
        }

        private PickList[] GeneratePickLists()
        {
            var pickLists = new List<PickList>();

            for (var i = 0; i < _options.NumPickLists; i++)
            {
                var numItems = _pickListItemCountRandom.Next();
                var itemTypes = GetRandomItems(_availableItems, numItems);
                
                var items = itemTypes.Select(x => {
                    return new PickListItem
                    {
                        ID = x,
                        Quantity = _pickListQuantityRandom.Next()
                    };
                });

                var pickList = new PickList
                {
                    ID = $"pickList_{i}",
                    Items = items.ToList()
                };

                pickLists.Add(pickList);
            }

            return pickLists.ToArray();
        }

        private Container[] GenerateContainers()
        {
            var containers = new List<Container>();

            for (var i = 0; i < _options.NumContainers; i++)
            {
                var numItems = _containerItemCountRandom.Next();
                var itemTypes = GetRandomItems(_availableItems, numItems);
                
                var items = itemTypes.Select(x => {
                    return new Item
                    {
                        ID = x,
                        Quantity = _containerItemCountRandom.Next()
                    };
                });

                var container = new Container
                {
                    LPN = $"container_{i}",
                    Items = items.ToList()
                };

                containers.Add(container);
            }

            return containers.ToArray();
        }

        
        private static string[] AllItems()
        {
            var allItems = new List<string>();

            var wordsA = new[] 
            {
                "large", "small", "oblong", "bulbous", "smelly",
                "horrible", "nondescript", "flagrant", "circular", "incredible"
            };

            var wordsB = new[] 
            {
                "pink", "blue", "red", "transparent", "golden",
                "white", "black", "green", "coral", "orange"
            };

            var wordsC = new[] 
            {
                "shelf", "bracket", "box", "peg", "sprocket",
                "bucket", "plaque", "cart", "sign", "tub"
            };

            foreach (var wordA in wordsA)
            foreach (var wordB in wordsB)
            foreach (var wordC in wordsC)
            {
                allItems.Add($"{wordA} {wordB} {wordC}");
            }

            return allItems.ToArray();
        }
    }
}