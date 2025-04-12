using System;
using Blocks;
using UnityEngine;

// Token: 0x020002D9 RID: 729
public class BlockHighlighter : HelpTextHighlighter
{
	// Token: 0x06002144 RID: 8516 RVA: 0x000F3DD8 File Offset: 0x000F21D8
	public BlockHighlighter(Block b)
	{
		this.block = b;
	}

	// Token: 0x06002145 RID: 8517 RVA: 0x000F3DE8 File Offset: 0x000F21E8
	public override void Show()
	{
		this.go = UnityEngine.Object.Instantiate<GameObject>(this.block.go);
		MeshRenderer[] componentsInChildren = this.go.GetComponentsInChildren<MeshRenderer>();
		string text = "Pulsate Glow";
		if (!ResourceLoader.loadedTextures.Contains(text))
		{
			ResourceLoader.LoadTexture(text, "Textures");
		}
		Material material = Materials.GetMaterial("Yellow", text, ShaderType.PulsateGlow);
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			meshRenderer.material = material;
		}
		this.go.transform.position = this.block.go.transform.position;
		this.go.transform.localScale = Vector3.one * 1.1f;
	}

	// Token: 0x04001C3E RID: 7230
	public Block block;
}
