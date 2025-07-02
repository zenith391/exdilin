using System;

[Serializable]
public class TextureApplicationChangeRule
{
	public string blockType;

	public string texture;

	public int meshIndex;

	public bool negateCondition;

	public bool setScarcityEquivalent = true;
}
