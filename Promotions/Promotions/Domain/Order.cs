using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promotions.Domain
{
	public class Order
	{
		public Order()
		{
			Items = new List<OrderItem>();
		}

		public List<OrderItem> Items { get; set; }
	}

	public class OrderItem
	{
		public string Id { get; set; }
		public int Quantity { get; set; }
	}
}
