using Google.OrTools.LinearSolver;
using static Google.OrTools.LinearSolver.Solver;

namespace OrToolsPickListSolver.Models
{
    public class PickListSolverVariable
    {
        public PickItem PickItem { get; set; }
        public ContainerItem ContainerItem  { get; set; }
        public Variable Variable { get; set; }
    }

    public class PickListSolverResult
    {
        public ResultStatus ResultStatus { get; set; }
        public PickList[] PickLists { get; set; } 
        public Container[] Containers { get; set; } 
    }
}