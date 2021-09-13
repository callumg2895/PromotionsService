using Promotions.Domain;
using NUnit.Framework;
using System.Collections.Generic;

namespace Promotions.Tests
{
	[TestFixture]
	public class PromotionServiceTests
	{
		private Inventory _inventory;
		private List<Promotion> _promotions;

		public PromotionServiceTests()
		{

		}

		[SetUp]
		public void Setup()
		{
			_inventory = new Inventory();

			_inventory.Items.Add(new InventoryItem() { Id = "A", Price = 50m });
			_inventory.Items.Add(new InventoryItem() { Id = "B", Price = 30m });
			_inventory.Items.Add(new InventoryItem() { Id = "C", Price = 20m });
			_inventory.Items.Add(new InventoryItem() { Id = "D", Price = 15m });

			_promotions = new List<Promotion>();

			_promotions.Add(new Promotion()
			{
				BasePrice = 130m,
				Items = new List<PromotionItem>()
				{
					new PromotionItem() { Id = "A", Quantity = 3 }
				}
			});

			_promotions.Add(new Promotion()
			{
				BasePrice = 45m,
				Items = new List<PromotionItem>()
				{
					new PromotionItem() { Id = "B", Quantity = 2 }
				}
			});

			_promotions.Add(new Promotion()
			{
				BasePrice = 30m,
				Items = new List<PromotionItem>()
				{
					new PromotionItem() { Id = "C", Quantity = 1 },
					new PromotionItem() { Id = "D", Quantity = 1 },
				}
			});
		}

		[TearDown]
		public void TearDown()
		{
			_promotions.Clear();

			_promotions = null;
			_inventory = null;
		}

		[Test]
		public void ScenarioA()
		{
			// Arrange
			var promotionService = new PromotionService(_inventory, _promotions);
			var order = new Order()
			{
				Items = new List<OrderItem>()
				{
					new OrderItem() { Id = "A", Quantity = 1 },
					new OrderItem() { Id = "B", Quantity = 1 },
					new OrderItem() { Id = "C", Quantity = 1 }
				}
			};

			// Act
			var orderTotal = promotionService.CalculateTotal(order);

			// Assert
			Assert.AreEqual(100m, orderTotal);
		}

		[Test]
		public void ScenarioB()
		{
			// Arrange
			var promotionService = new PromotionService(_inventory, _promotions);
			var order = new Order()
			{
				Items = new List<OrderItem>()
				{
					new OrderItem() { Id = "A", Quantity = 1 },
					new OrderItem() { Id = "B", Quantity = 1 },
					new OrderItem() { Id = "C", Quantity = 1 }
				}
			};

			// Act
			var orderTotal = promotionService.CalculateTotal(order);

			// Assert
			Assert.AreEqual(100m, orderTotal);
		}

		[Test]
		public void ScenarioC()
		{
			// Arrange
			var promotionService = new PromotionService(_inventory, _promotions);
			var order = new Order()
			{
				Items = new List<OrderItem>()
				{
					new OrderItem() { Id = "A", Quantity = 1 },
					new OrderItem() { Id = "B", Quantity = 1 },
					new OrderItem() { Id = "C", Quantity = 1 }
				}
			};

			// Act
			var orderTotal = promotionService.CalculateTotal(order);

			// Assert
			Assert.AreEqual(100m, orderTotal);
		}

		/// <summary>
		/// Bonus - testing the ability to define a promotion with a price that is x% of a SKU unit price
		/// </summary>
		[Test]
		public void ScenarioD()
		{
			// Arrange

			// Effectively, this is a 'buy 3 for the price of two' on SKU 'C'
			_promotions.Add(new Promotion()
			{
				BasePrice = 0,
				Items = new List<PromotionItem>()
				{
					new PromotionItem() { Id = "C", Quantity = 3 }
				},
				PriceItems = new List<PromotionPriceItem>()
				{
					// Define a price value that is 50% of the cost of SKU 'C'
					new PromotionPriceItem() { Id = "C", Percentage = 200 }
				}
			});

			var promotionService = new PromotionService(_inventory, _promotions);
			var order = new Order()
			{
				Items = new List<OrderItem>()
				{
					new OrderItem() { Id = "C", Quantity = 4 }
				}
			};

			// Act
			var orderTotal = promotionService.CalculateTotal(order);

			// Assert
			Assert.AreEqual(60m, orderTotal);
		}
	}
}