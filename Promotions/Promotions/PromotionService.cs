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
			var orderLookup = order.Items.ToDictionary(i => i.Id, i => i.Quantity);
			var appliedPromotions = new List<Promotion>();

			/*
			 * Loop through the promotions. If a promotion applies, make a note of it, and subtract the associated quantity 
			 * of items from the quantity stored in order lookup table. At the end we should be left with a list of 
			 * applied promotions, and the order lookup table will contain the quantity of each item that could not be assigned
			 * to a promotion. Then we just sum up the prices of these.
			 */

			// Apply the promotions where possible, and remove affected items (if any) from the order copy.
			foreach (var promotion in _promotions)
			{
				// The promotion might apply more than once, so keep going until it can't be applied anymore.
				// Question: do we want to define some sort of 'priority' for promotions in the future?
				while (CanApplyPromotion(promotion, orderLookup))
				{
					ApplyPromotion(promotion, orderLookup);

					appliedPromotions.Add(promotion);
				}
			}

			// Update the total with the price of all applied promotions
			orderTotal += SumPromotionPrices(appliedPromotions);

			// Update the total with the price of all remaining items in the order
			orderTotal += SumOrderPrices(orderLookup);

			return orderTotal;
		}

		private bool CanApplyPromotion(Promotion promotion, Dictionary<string, int> orderLookup)
		{
			// Check that the order meets the requirements of the promotion
			foreach (var promotionItem in promotion.Items)
			{
				if (orderLookup.TryGetValue(promotionItem.Id, out var quantity) && quantity >= promotionItem.Quantity)
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

			return true;
		}

		private void ApplyPromotion(Promotion promotion, Dictionary<string, int> orderLookup)
		{
			foreach (var promotionItem in promotion.Items)
			{
				var quantity = orderLookup[promotionItem.Id];

				quantity -= promotionItem.Quantity;

				orderLookup[promotionItem.Id] = quantity;
			}
		}

		private decimal SumPromotionPrices(List<Promotion> promotions)
		{
			var total = 0m;

			foreach (var promotion in promotions)
			{
				total += promotion.BasePrice;
				total += promotion.PriceItems.Sum((i) =>
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

			return total;
		}

		private decimal SumOrderPrices(Dictionary<string, int> orderLookup)
		{
			var total = 0m;

			foreach (var id in orderLookup.Keys)
			{
				if (_inventoryLookup.TryGetValue(id, out var inventoryItem))
				{
					total += (inventoryItem.Price * orderLookup[id]);
				}
				else
				{
					throw new ArgumentException($"SKU id '{id}' not recognised in inventory");
				}
			}

			return total;
		}
	}
}
