namespace OrToolsPickListSolver.Models
{
    public class TestDataGeneratorOptions
    {
        public short NumPickLists { get; set; }
        public short MaxItemsPerPickList { get; set; }
        public short NumContainers { get; set; }
        public short MaxItemsPerContainer { get; set; }
        public short MaxPickListItemQuantity { get; set; }
        public short MaxContainerItemQuantity { get; set; }
        public short TotalItemTypes { get; set; }
        public double ReplenishmentPct { get; set; }
    }

    public class TestDataGeneratorResults
    {
        public PickList[] PickLists { get; set; }
        public Container[] Containers { get; set; }
    }
}