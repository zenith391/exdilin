using System;
using System.Collections.Generic;
using SimpleJSON;

// Token: 0x020001D1 RID: 465
public abstract class ModelFeatureType
{
	// Token: 0x0600185D RID: 6237 RVA: 0x000AA968 File Offset: 0x000A8D68
	public virtual void Reset()
	{
	}

	// Token: 0x0600185E RID: 6238 RVA: 0x000AA96A File Offset: 0x000A8D6A
	public virtual void Update(List<List<List<Tile>>> model, Tile tile, int blockIndex, int rowIndex, int columnIndex, bool beforeThen)
	{
	}

	// Token: 0x0600185F RID: 6239 RVA: 0x000AA96C File Offset: 0x000A8D6C
	public virtual int ParameterCount()
	{
		return 1;
	}

	// Token: 0x06001860 RID: 6240
	public abstract void SetContextValues(ModelCategorizerContext modelCategorizerContext);

	// Token: 0x06001861 RID: 6241 RVA: 0x000AA96F File Offset: 0x000A8D6F
	public virtual void ToJSON(JSONStreamEncoder encoder)
	{
		encoder.WriteKey(this.name);
		if (this.ParameterCount() > 1)
		{
			encoder.BeginArray();
			this.EncodeParameters(encoder);
			encoder.EndArray();
		}
		else
		{
			this.EncodeParameters(encoder);
		}
	}

	// Token: 0x06001862 RID: 6242 RVA: 0x000AA9A8 File Offset: 0x000A8DA8
	protected virtual void EncodeParameters(JSONStreamEncoder encoder)
	{
		encoder.WriteNumber(1L);
	}

	// Token: 0x04001353 RID: 4947
	public string name;
}
