using System;

// Token: 0x020001B8 RID: 440
public class TextureInfo
{
	// Token: 0x060017C3 RID: 6083 RVA: 0x000A6F4E File Offset: 0x000A534E
	public TextureInfo(string name, ShaderType shader, Mapping mapping)
	{
		this.name = name;
		this.shader = shader;
		this.mapping = mapping;
	}

	// Token: 0x040012CD RID: 4813
	public string name;

	// Token: 0x040012CE RID: 4814
	public ShaderType shader;

	// Token: 0x040012CF RID: 4815
	public Mapping mapping;
}
