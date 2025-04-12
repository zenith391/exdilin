using System;
using SimpleJSON;

// Token: 0x020001CD RID: 461
public class IntModelFeatureType : ModelFeatureType
{
	// Token: 0x06001841 RID: 6209 RVA: 0x000AA9BA File Offset: 0x000A8DBA
	public int GetValue()
	{
		return this.value;
	}

	// Token: 0x06001842 RID: 6210 RVA: 0x000AA9C2 File Offset: 0x000A8DC2
	protected override void EncodeParameters(JSONStreamEncoder encoder)
	{
		encoder.WriteNumber((long)this.value);
	}

	// Token: 0x06001843 RID: 6211 RVA: 0x000AA9D1 File Offset: 0x000A8DD1
	public override void Reset()
	{
		this.value = 0;
	}

	// Token: 0x06001844 RID: 6212 RVA: 0x000AA9DA File Offset: 0x000A8DDA
	public override void SetContextValues(ModelCategorizerContext modelCategorizerContext)
	{
		modelCategorizerContext.SetInt(this.name, this.value);
	}

	// Token: 0x0400134C RID: 4940
	protected int value;
}
