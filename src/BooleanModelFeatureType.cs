using SimpleJSON;

public class BooleanModelFeatureType : ModelFeatureType
{
	protected bool triggered;

	public override void Reset()
	{
		triggered = false;
	}

	public bool IsTriggered()
	{
		return triggered;
	}

	public override void ToJSON(JSONStreamEncoder encoder)
	{
		if (triggered)
		{
			base.ToJSON(encoder);
		}
	}

	public override void SetContextValues(ModelCategorizerContext modelCategorizerContext)
	{
		modelCategorizerContext.SetBool(name, triggered);
	}
}
