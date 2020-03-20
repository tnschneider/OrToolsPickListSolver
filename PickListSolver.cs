using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;
using or_tools.Models;
using static Google.OrTools.LinearSolver.Solver;

namespace or_tools
{
    public class PickListSolver
    {
        private readonly PickList[] _pickLists;
        private readonly Container[] _containers;
        private readonly PickItem[] _pickItems;
        private readonly long _pickItemsCount;
        private readonly ContainerItem[] _containerItems;
        private readonly long _containerItemsCount;
        private readonly string[] _distinctItemIds;
        private readonly long _distinctItemIdsCount;
        private readonly Solver _solver;
        private readonly Objective _objective;

        public PickListSolver(PickList[] pickLists, Container[] containers)
        {
            _containers = containers;
            _pickLists = pickLists;

            _pickItems = pickLists.SelectMany(x => x.Items, (pl, item) => new PickItem { PickList = pl, Item = item }).ToArray();
            _pickItemsCount = _pickItems.Length;
            
            _containerItems = containers.SelectMany(x => x.Items, (pl, item) => new ContainerItem { Container = pl, Item = item }).ToArray();
            _containerItemsCount = _containerItems.Length;

            _distinctItemIds = _pickItems.Select(x => x.Item.ID).Distinct().ToArray();
            _distinctItemIdsCount = _distinctItemIds.Length;

            _solver = new Solver(nameof(PickListSolver), OptimizationProblemType.CBC_MIXED_INTEGER_PROGRAMMING);
            _objective = _solver.Objective();
            _objective.SetMinimization();
        }


        public PickListSolverResult Solve()
        {
            var variables = CreateMainVariables();
            
            CreatePickListTotalConstraints(variables);
            CreateContainerTotalConstraints(variables);
            CreateInventoryTotalConstraints(variables);

            CreateUsageVariables(variables);

            var resultStatus = _solver.Solve();

            ApplyChanges(variables);

            return new PickListSolverResult
            {
                ResultStatus = resultStatus, 
                PickLists = _pickLists, 
                Containers = _containers
            };
        }

        private PickListSolverVariable[,] CreateMainVariables()
        {
            var variables = new PickListSolverVariable[_pickItemsCount, _containerItemsCount];
            var i = 0;
            foreach (var pickItem in _pickItems)
            {
                var j = 0;
                foreach (var containerItem in _containerItems)
                {
                    var isMatch = containerItem.Item.ID == pickItem.Item.ID;
                    var variable = isMatch
                        ? _solver.MakeIntVar(0, pickItem.Item.ID == containerItem.Item.ID ? Math.Min(pickItem.Item.Quantity, containerItem.Item.Quantity) : 0, 
                            $"qty_{pickItem.PickList.ID}_{pickItem.Item.ID}_{containerItem.Container.LPN}_{containerItem.Item.ID}")
                        : null;
                    
                    variables[i, j] = new PickListSolverVariable
                    {
                        PickItem = pickItem,
                        ContainerItem = containerItem,
                        Variable = variable
                    };
                    j++;
                }
                i++;
            }
            return variables;
        }

        private void CreatePickListTotalConstraints(PickListSolverVariable[,] variables)
        {
            for (var i = 0; i < _pickItemsCount; i++)
            {
                var constraint = _solver.MakeConstraint(0.0, variables[i, 0].PickItem.Item.Quantity);
                for (var j = 0; j < _containerItemsCount; j++)
                {
                    constraint.SetCoefficient(variables[i,j].Variable, 1);
                }
            }
        }

        private void CreateContainerTotalConstraints(PickListSolverVariable[,] variables)
        {
            for (var j = 0; j < _containerItemsCount; j++)
            {
                var constraint = _solver.MakeConstraint(0.0, variables[0, j].ContainerItem.Item.Quantity);
                for (var i = 0; i < _containerItemsCount; i++)
                {
                    constraint.SetCoefficient(variables[i,j].Variable, 1);
                }
            }
        }

        private void CreateInventoryTotalConstraints(PickListSolverVariable[,] variables)
        {
            foreach (var id in _distinctItemIds)
            {
                var pickListRequestedQty = _pickItems.Where(x => x.Item.ID == id).Sum(x => x.Item.Quantity);
                var inventoryQty = _containerItems.Where(x => x.Item.ID == id).Sum(x => x.Item.Quantity);
                var constraint = _solver.MakeConstraint(Math.Min(inventoryQty, pickListRequestedQty), double.PositiveInfinity);
                foreach (var v in variables)
                {
                    if (v.ContainerItem.Item.ID == id)
                    {
                        constraint.SetCoefficient(v.Variable, 1);
                    }
                }
            }
        }

        private void CreateUsageVariables(PickListSolverVariable[,] variables)
        {
            for (var i = 0; i < _containers.Length; i++)
            {
                var lpn = _containers[i].LPN;

                var variable = _solver.MakeBoolVar($"usage_{lpn}");
                _objective.SetCoefficient(variable, 1);
                var M = 10000000;
                var constraint = _solver.MakeConstraint(1 - M, 0);
                constraint.SetCoefficient(variable, -M);

                foreach (var v in variables)
                {
                    if (v.ContainerItem.Container.LPN == lpn && v.Variable != null)
                    {
                        constraint.SetCoefficient(v.Variable, 1);
                    }
                }
            }
        }

        private void ApplyChanges(PickListSolverVariable[,] variables)
        {
            foreach (var v in variables)
            {
                if (v.Variable != null && v.Variable.SolutionValue() > 0)
                {
                    var val = (long)v.Variable.SolutionValue();
                    
                    if (v.PickItem.Item.Orders == null)
                        v.PickItem.Item.Orders = new List<PickListOrder>();

                    v.PickItem.Item.Orders.Add(new PickListOrder
                    {
                        LPN = v.ContainerItem.Container.LPN,
                        Quantity = val
                    });
                    v.ContainerItem.Item.Quantity -= val;
                }
            }
        }
    }
}