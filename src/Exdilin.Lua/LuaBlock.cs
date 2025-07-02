using System;

namespace Exdilin.Lua;

public class LuaBlock : BlockEntry
{
	public void register()
	{
		BlockItemsRegistry.AddBlock(this);
	}

	public LuaBlockItem default_block_item(string iconName, string rarity = "common")
	{
		LuaBlockItem luaBlockItem = new LuaBlockItem();
		RarityLevelEnum rarity2 = (RarityLevelEnum)Enum.Parse(typeof(RarityLevelEnum), rarity, ignoreCase: true);
		BlockItem item = new BlockItem(ExdilinAPI.allocate_block_item_id(), id, "", id, "Block.Create", new object[1] { id }, iconName, "White", rarity2);
		luaBlockItem.buildPaneTab = "Blocks";
		luaBlockItem.item = item;
		luaBlockItem.argumentPatterns = new string[1] { id };
		return luaBlockItem;
	}
}
