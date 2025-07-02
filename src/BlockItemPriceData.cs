using SimpleJSON;

public class BlockItemPriceData
{
	public int blockItemID { get; private set; }

	public int stackPrice { get; private set; }

	public int stackSize { get; private set; }

	public int goldPennies { get; private set; }

	public bool isCoinsValueFlatRate { get; private set; }

	public BlockItemPriceData(JObject json)
	{
		blockItemID = BWJsonHelpers.PropertyIfExists(blockItemID, "block_item_id", json);
		stackPrice = BWJsonHelpers.PropertyIfExists(stackPrice, "a_la_carte_stack_price", json);
		stackSize = BWJsonHelpers.PropertyIfExists(stackSize, "a_la_carte_stack_size", json);
		goldPennies = BWJsonHelpers.PropertyIfExists(goldPennies, "gold_pennies", json);
		isCoinsValueFlatRate = BWJsonHelpers.PropertyIfExists(isCoinsValueFlatRate, "is_coins_value_flat_rate", json);
	}
}
