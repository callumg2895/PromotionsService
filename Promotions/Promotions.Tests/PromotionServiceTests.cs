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

		/// <summary>
		/// Scenario A: Promotion Service calculates the correct total price for an order where no promotions apply.
		/// </summary>
		[Test]
		public void ScenarioA_ExpectCorrectPriceWhenNoPromotionsApply()
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
		/// Scenario B: Promotion Service calculates the correct total price for an order where promotions apply only to a subset of the items.
		/// </summary>
		[Test]
		public void ScenarioB_ExpectCorrectPriceWhenPromotionsApplyToSubsetOfOrder()
		{
			// Arrange
			var promotionService = new PromotionService(_inventory, _promotions);
			var order = new Order()
			{
				Items = new List<OrderItem>()
				{
					new OrderItem() { Id = "A", Quantity = 5 },
					new OrderItem() { Id = "B", Quantity = 5 },
					new OrderItem() { Id = "C", Quantity = 1 }
				}
			};

			// Act
			var orderTotal = promotionService.CalculateTotal(order);

			// Assert
			Assert.AreEqual(370m, orderTotal);
		}

		/// <summary>
		/// Scenario C: Promotion Service calculates the correct total price for an order where promotions apply to all the items.
		/// </summary>
		[Test]
		public void ScenarioC_ExpectCorrectPriceWhenPromotionsApplyToAllItemsInTheOrder()
		{
			// Arrange
			var promotionService = new PromotionService(_inventory, _promotions);
			var order = new Order()
			{
				Items = new List<OrderItem>()
				{
					new OrderItem() { Id = "A", Quantity = 3 },
					new OrderItem() { Id = "B", Quantity = 5 },
					new OrderItem() { Id = "C", Quantity = 1 },
					new OrderItem() { Id = "D", Quantity = 1 }
				}
			};

			// Act
			var orderTotal = promotionService.CalculateTotal(order);

			// Assert
			Assert.AreEqual(280m, orderTotal);
		}

		/// <summary>
		/// Scenario D: Promotion Service calculates the correct total price for an order where promotions are defined with prices as a percentage 
		/// of the full price of a SKU.
		/// </summary>
		[Test]
		public void ScenarioD_ExpectCorrectPriceWhenPromotionPricesAreDefinedAsPercentageOfSkuPrice()
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