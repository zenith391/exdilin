using System.Collections.Generic;
using SimpleJSON;

public abstract class ModelFeatureType
{
	public string name;

	public virtual void Reset()
	{
	}

	public virtual void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
	}

	public virtual int ParameterCount()
	{
		return 1;
	}

	public abstract void SetContextValues(ModelCategorizerContext modelCategorizerContext);

	public virtual void ToJSON(JSONStreamEncoder encoder)
	{
		encoder.WriteKey(name);
		if (ParameterCount() > 1)
		{
			encoder.BeginArray();
			EncodeParameters(encoder);
			encoder.EndArray();
		}
		else
		{
			EncodeParameters(encoder);
		}
	}

	protected virtual void EncodeParameters(JSONStreamEncoder encoder)
	{
		encoder.WriteNumber(1L);
	}
}
