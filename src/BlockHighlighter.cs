using Blocks;
using UnityEngine;

public class BlockHighlighter : HelpTextHighlighter
{
	public Block block;

	public BlockHighlighter(Block b)
	{
		block = b;
	}

	public override void Show()
	{
		go = Object.Instantiate(block.go);
		MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
		string text = "Pulsate Glow";
		if (!ResourceLoader.loadedTextures.Contains(text))
		{
			ResourceLoader.LoadTexture(text);
		}
		Material material = Materials.GetMaterial("Yellow", text, ShaderType.PulsateGlow);
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer meshRenderer in array)
		{
			meshRenderer.material = material;
		}
		go.transform.position = block.go.transform.position;
		go.transform.localScale = Vector3.one * 1.1f;
	}
}
