using System.Collections.Generic;

public class BlockTypeExistsModelFeatureType : SingleBlockTypeBooleanModelFeatureType
{
	public HashSet<string> blockTypes = new HashSet<string>();

	protected override void Update(string blockType)
	{
		triggered = triggered || blockTypes.Contains(blockType);
	}
}
