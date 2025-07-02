using SimpleJSON;

public class FloatModelFeatureType : ModelFeatureType
{
	protected float value;

	protected override void EncodeParameters(JSONStreamEncoder encoder)
	{
		encoder.WriteNumber(value);
	}

	public override void Reset()
	{
		value = 0f;
	}

	public override void SetContextValues(ModelCategorizerContext modelCategorizerContext)
	{
		modelCategorizerContext.SetFloat(name, value);
	}
}
