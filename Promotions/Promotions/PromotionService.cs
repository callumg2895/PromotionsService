using System;
using System.Collections.Generic;
using System.Linq;
using Promotions.Domain;

namespace Promotions
{
	/// <summary>
	/// Responsible for calculating the total value of a given order, based on the currently active promotions
	/// in the system and the price of each item in the order as defined by the inventory
	/// </summary>
	public class PromotionService
	{
		// Contains the list of SKU ids contained within the inventory
		private readonly List<Promotion> _promotions;

		private readonly Dictionary<string, InventoryItem> _inventoryLookup;

		public PromotionService(	Inventory inventory,
									List<Promotion> promotions)
		{
			/*
			 * In a normal web application, this class should really sit in the service layer, and we'd inject
			 * some sort of database context to allow us to grab information about the inventory, current promotions,
			 * and the order.
			 * 
			 * We don't know anything about the external system though, so for now we'll take the simple approach
			 * of providing the inventory and promotions via arguments in the constructor.
			 */

			_promotions = promotions;

			_inventoryLookup = inventory.Items.ToDictionary(i => i.Id, i => i);
		}

		public decimal CalculateTotal(Order order)
		{
			var orderTotal = 0m;
			
			/*
			 * We want to create a copy of the order, so we don't affect the original reference with any changes. This
			 * prevents unwanted side effects for the caller.
			 * 
			 * The idea is to loop through the promotions. If a promotion applies, make a note of it, and remove the
			 * associated items from the order. At the end we should be left with a list of applied promotions, and a
			 * list of order items that could not be assigned to a promotion. Then we just sum up the prices of these.
			 */

			var orderCopy = CreateOrderCopy(order);
			var appliedPromotions = new List<Promotion>();

			// Apply the promotions where possible, and remove affected items (if any) from the order copy.
			foreach (var promotion in _promotions)
			{
				// The promotion might apply more than once, so keep going until it can't be applied anymore.
				// Question: do we want to define some sort of 'priority' for promotions in the future?
				while (TryApplyPromotion(promotion, orderCopy))
				{
					appliedPromotions.Add(promotion);
				}
			}

			// Update the total with the price of all applied promotions
			foreach (var promotion in appliedPromotions)
			{
				orderTotal += promotion.BasePrice;
				orderTotal += promotion.PriceItems.Sum((i) =>
				{
					if (!_inventoryLookup.TryGetValue(i.Id, out var inventoryItem))
					{
						throw new ArgumentException($"SKU id '{i.Id}' not recognised in inventory");
					}

					var price = inventoryItem.Price;
					var priceModifier = i.Percentage / 100;

					return price * priceModifier;
				});
			}

			// Update the total with the price of all remaining items in the order
			foreach (var orderItem in orderCopy.Items)
			{
				if (_inventoryLookup.TryGetValue(orderItem.Id, out var inventoryItem))
				{
					orderTotal += (inventoryItem.Price * orderItem.Quantity);
				}
				else
				{
					throw new ArgumentException($"SKU id '{orderItem.Id}' not recognised in inventory");
				}
			}

			return orderTotal;
		}

		private Order CreateOrderCopy(Order order)
		{
			var orderCopy = new Order();

			orderCopy.Items = new List<OrderItem>();

			foreach (var item in order.Items)
			{
				var itemCopy = new OrderItem();

				itemCopy.Id = item.Id;
				itemCopy.Quantity = item.Quantity;

				orderCopy.Items.Add(itemCopy);
			}

			return orderCopy;
		}

		private bool TryApplyPromotion(Promotion promotion, Order order)
		{
			var orderLookup = order.Items.ToDictionary(i => i.Id, i => i);

			// Check that the order meets the requirements of the promotion
			foreach (var promotionItem in promotion.Items)
			{
				if (orderLookup.TryGetValue(promotionItem.Id, out var orderItem) && orderItem.Quantity >= promotionItem.Quantity)
				{
					// An item of the correct type and quantity exists and meets the requirements of the promotion, so keep going 
					continue;
				}
				else
				{
					// At least one of the promotion requirements hasn't been met, so we can break out early.
					return false;
				}
			}

			// At this point, we know the promotion can be applied, so apply it
			foreach (var promotionItem in promotion.Items)
			{
				var orderItem = orderLookup[promotionItem.Id];

				orderItem.Quantity -= promotionItem.Quantity;
			}

			return true;
		}
	}
}
