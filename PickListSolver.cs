using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;
using OrToolsPickListSolver.Models;
using static Google.OrTools.LinearSolver.Solver;

namespace OrToolsPickListSolver
{
    public class PickListSolver
    {
        private readonly PickList[] _pickLists;
        private readonly Container[] _containers;
        private readonly PickItem[] _pickItems;
        private readonly ContainerItem[] _containerItems;
        private readonly Solver _solver;
        private readonly Objective _objective;

        public PickListSolver(PickList[] pickLists, Container[] containers, long timeLimitSeconds)
        {
            _containers = containers;
            _pickLists = pickLists;

            _pickItems = pickLists
                .SelectMany(x => x.Items, (pl, item) => new PickItem { PickList = pl, Item = item })
                .ToArray();
            
            _containerItems = containers
                .SelectMany(x => x.Items, (pl, item) => new ContainerItem { Container = pl, Item = item })
                .ToArray();

            _solver = new Solver(nameof(PickListSolver), 
                OptimizationProblemType.CBC_MIXED_INTEGER_PROGRAMMING);

            _solver.SetTimeLimit(timeLimitSeconds * 1000);
            _solver.SuppressOutput();

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

        private List<PickListSolverVariable> CreateMainVariables()
        {
            var variables = new List<PickListSolverVariable>();
            foreach (var pickItem in _pickItems)
            foreach (var containerItem in _containerItems.Where(x => x.Item.ID == pickItem.Item.ID))
            {
                variables.Add(new PickListSolverVariable
                {
                    PickItem = pickItem,
                    ContainerItem = containerItem,
                    Variable = _solver.MakeIntVar(0, pickItem.Item.ID == containerItem.Item.ID 
                            ? Math.Min(pickItem.Item.Quantity, containerItem.Item.Quantity) 
                            : 0, 
                        $"qty_{pickItem.PickList.ID}_{pickItem.Item.ID}_{containerItem.Container.LPN}_{containerItem.Item.ID}")
                });
            }
            return variables;
        }

        private void CreatePickListTotalConstraints(List<PickListSolverVariable> variables)
        {
            foreach (var pickItem in _pickItems)
            {
                var constraint = _solver.MakeConstraint(0.0, pickItem.Item.Quantity);
                foreach (var variable in variables.Where(x => x.PickItem == pickItem))
                {
                    constraint.SetCoefficient(variable.Variable, 1);
                }
            }
        }

        private void CreateContainerTotalConstraints(List<PickListSolverVariable> variables)
        {
            foreach (var containerItem in _containerItems)
            {
                var constraint = _solver.MakeConstraint(0.0, containerItem.Item.Quantity);
                foreach (var variable in variables.Where(x => x.ContainerItem == containerItem))
                {
                    constraint.SetCoefficient(variable.Variable, 1);
                }
            }
        }

        private void CreateInventoryTotalConstraints(List<PickListSolverVariable> variables)
        {
            foreach (var id in _pickItems.Select(x => x.Item.ID).Distinct().ToArray())
            {
                var pickListRequestedQty = _pickItems.Where(x => x.Item.ID == id).Sum(x => x.Item.Quantity);
                var inventoryQty = _containerItems.Where(x => x.Item.ID == id).Sum(x => x.Item.Quantity);

                var constraint = _solver.MakeConstraint(Math.Min(inventoryQty, pickListRequestedQty), double.PositiveInfinity);
                
                foreach (var v in variables.Where(x => x.ContainerItem.Item.ID == id))
                {
                    constraint.SetCoefficient(v.Variable, 1);
                }
            }
        }

        private void CreateUsageVariables(List<PickListSolverVariable> variables)
        {
            const int M = 10000000;

            foreach (var container in _containers)
            {
                var variable = _solver.MakeBoolVar($"usage_{container.LPN}");
                _objective.SetCoefficient(variable, 1);
                
                var constraint = _solver.MakeConstraint(1 - M, 0);
                constraint.SetCoefficient(variable, -M);

                foreach (var v in variables.Where(x => x.ContainerItem.Container.LPN == container.LPN))
                {
                    constraint.SetCoefficient(v.Variable, container.Type == ContainerType.Replenishment ? 3 : 1);
                }
            }
        }

        private void ApplyChanges(List<PickListSolverVariable> variables)
        {
            foreach (var v in variables.Where(x => x.Variable.SolutionValue() > 0))
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