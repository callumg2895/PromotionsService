using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promotions.Domain
{
	public class Inventory
	{
		public Inventory()
		{
			Items = new List<InventoryItem>();
		}

		public List<InventoryItem> Items { get; set; }
	}

	public class InventoryItem
	{
		public string Id { get; set; }
		public decimal Price { get; set; }
	}
}
