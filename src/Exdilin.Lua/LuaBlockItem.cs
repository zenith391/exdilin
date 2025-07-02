namespace Exdilin.Lua;

public class LuaBlockItem : BlockItemEntry
{
	public void register()
	{
		BlockItemsRegistry.AddBlockItem(this);
	}
}
