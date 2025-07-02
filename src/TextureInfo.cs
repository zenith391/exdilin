public class TextureInfo
{
	public string name;

	public ShaderType shader;

	public Mapping mapping;

	public TextureInfo(string name, ShaderType shader, Mapping mapping)
	{
		this.name = name;
		this.shader = shader;
		this.mapping = mapping;
	}
}
