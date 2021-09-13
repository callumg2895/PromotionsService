using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promotions.Domain
{
	public class Promotion
	{
		public Promotion()
		{
			Items = new List<PromotionItem>();
			PriceItems = new List<PromotionPriceItem>();
		}

		public List<PromotionItem> Items { get; set; }

		public List<PromotionPriceItem> PriceItems { get; set; }

		public decimal BasePrice { get; set; }
	}

	public class PromotionItem
	{
		public string Id { get; set; }

		public int Quantity { get; set; }
	}

	public class PromotionPriceItem
	{
		public string Id { get; set; }

		public decimal Percentage { get; set; }
	}
}
