using SimpleJSON;

public class IntModelFeatureType : ModelFeatureType
{
	protected int value;

	public int GetValue()
	{
		return value;
	}

	protected override void EncodeParameters(JSONStreamEncoder encoder)
	{
		encoder.WriteNumber(value);
	}

	public override void Reset()
	{
		value = 0;
	}

	public override void SetContextValues(ModelCategorizerContext modelCategorizerContext)
	{
		modelCategorizerContext.SetInt(name, value);
	}
}
