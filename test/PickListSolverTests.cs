using System.Collections.Generic;
using FluentAssertions;
using OrToolsPickListSolver.Models;
using Xunit;
using static Google.OrTools.LinearSolver.Solver;

namespace OrToolsPickListSolver.Tests
{
    public class PickListSolverTests
    {
        [Fact]
        public void PickListSolver_Should_Solve_Pick_Lists()
        {
            var pickLists = new PickList[]
            {
                new PickList 
                { 
                    ID = "pickListA",
                    Items = new List<PickListItem> 
                    {
                        new PickListItem { ID = "itemA", Quantity = 10 },
                        new PickListItem { ID = "itemB", Quantity = 25 }
                    }
                },
                new PickList 
                { 
                    ID = "pickListB",
                    Items = new List<PickListItem> 
                    {
                        new PickListItem { ID = "itemA", Quantity = 5 },
                        new PickListItem { ID = "itemC", Quantity = 30 }
                    }
                }
            };

            var expectedPickLists = new PickList[]
            {
                new PickList 
                { 
                    ID = "pickListA",
                    Items = new List<PickListItem> 
                    {
                        new PickListItem { ID = "itemA", Quantity = 10,
                            Orders = new List<PickListOrder> { 
                                new PickListOrder { LPN = "lpnA", Quantity = 10 } } },
                        new PickListItem { ID = "itemB", Quantity = 25,
                            Orders = new List<PickListOrder> { 
                                new PickListOrder { LPN = "lpnA", Quantity = 5 } } }
                    }
                },
                new PickList 
                { 
                    ID = "pickListB",
                    Items = new List<PickListItem> 
                    {
                        new PickListItem { ID = "itemA", Quantity = 5,
                            Orders = new List<PickListOrder> { 
                                new PickListOrder { LPN = "lpnA", Quantity = 5 } } },
                        new PickListItem { ID = "itemC", Quantity = 30,
                            Orders = new List<PickListOrder> { 
                                new PickListOrder { LPN = "lpnC", Quantity = 30 } } },
                    }
                }
            };

            var containers = new Container[]
            {
                new Container 
                { 
                    LPN = "lpnA",
                    Items = new List<Item> 
                    {
                        new Item { ID = "itemA", Quantity = 115 },
                        new Item { ID = "itemB", Quantity = 5 }
                    }
                },
                new Container 
                { 
                    LPN = "lpnB",
                    Items = new List<Item>
                    {
                        new Item { ID = "itemC", Quantity = 9 }
                    }
                },
                new Container 
                { 
                    LPN = "lpnC",
                    Items = new List<Item>
                    {
                        new Item { ID = "itemC", Quantity = 300 }
                    }
                }
            };

            var solver = new PickListSolver(pickLists, containers, 10);

            var result = solver.Solve();

            result.ResultStatus.Should().Be(ResultStatus.OPTIMAL);

            result.PickLists.Should().BeEquivalentTo(expectedPickLists);
        }
    }
}
