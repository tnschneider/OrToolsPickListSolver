using System;
using System.Collections.Generic;
using or_tools.Models;
using Newtonsoft.Json;

namespace or_tools
{
    class Program
    {
        static void Main(string[] args)
        {
            var pickLists = GetPickLists();
            var containers = GetContainers();

            var solver = new PickListSolver(pickLists, containers);

            solver.Solve();

            Console.WriteLine(JsonConvert.SerializeObject(pickLists, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(containers, Formatting.Indented));
        }


        public static PickList[] GetPickLists()
        {
            return new PickList[]
            {
                new PickList 
                { 
                    ID = "pickListA",
                    Items = new List<PickListItem> 
                    {
                        new PickListItem
                        {
                            ID = "itemA",
                            Quantity = 10
                        },
                        new PickListItem
                        {
                            ID = "itemB",
                            Quantity = 25
                        }
                    }
                },
                new PickList 
                { 
                    ID = "pickListB",
                    Items = new List<PickListItem> 
                    {
                        new PickListItem
                        {
                            ID = "itemA",
                            Quantity = 5
                        },
                        new PickListItem
                        {
                            ID = "itemC",
                            Quantity = 30
                        }
                    }
                }
            };
        }

        public static Container[] GetContainers()
        {
            return new Container[]
            {
                new Container 
                { 
                    LPN = "lpnA",
                    Items = new List<Item> 
                    {
                        new Item
                        {
                            ID = "itemA",
                            Quantity = 115
                        },
                        new Item
                        {
                            ID = "itemB",
                            Quantity = 5
                        }
                    }
                },
                new Container 
                { 
                    LPN = "lpnB",
                    Items = new List<Item>
                    {
                        new Item
                        {
                            ID = "itemC",
                            Quantity = 9
                        }
                    }
                },
                new Container 
                { 
                    LPN = "lpnC",
                    Items = new List<Item>
                    {
                        new Item
                        {
                            ID = "itemC",
                            Quantity = 300
                        }
                    }
                }
            };
        }
    }
}
