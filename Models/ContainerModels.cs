using System.Collections.Generic;

namespace OrToolsPickListSolver.Models
{
    public class Container
    {
        public string LPN { get; set; }
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        public string ID { get; set; }
        public long Quantity { get; set; }
    }

    public class ContainerItem
    {
        public Container Container { get; set; }
        public Item Item { get; set; }
    }
}