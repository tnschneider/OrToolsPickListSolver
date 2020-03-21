using System.Collections.Generic;

namespace OrToolsPickListSolver.Models
{
    public class PickList
    {
        public string ID { get; set; }
        public List<PickListItem> Items { get; set; }
    }

    public class PickListItem
    {
        public string ID { get; set; }
        public long Quantity { get; set; }
        public List<PickListOrder> Orders { get; set; }
    }

    public class PickListOrder
    {
        public string LPN { get; set; }
        public long Quantity { get; set; }
    }

    public class PickItem
    {
        public PickList PickList { get; set; }
        public PickListItem Item { get; set; }
    }
}