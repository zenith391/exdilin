using System;
using SimpleJSON;

// Token: 0x020001C8 RID: 456
public class FloatModelFeatureType : ModelFeatureType
{
	// Token: 0x06001835 RID: 6197 RVA: 0x000AB003 File Offset: 0x000A9403
	protected override void EncodeParameters(JSONStreamEncoder encoder)
	{
		encoder.WriteNumber(this.value);
	}

	// Token: 0x06001836 RID: 6198 RVA: 0x000AB011 File Offset: 0x000A9411
	public override void Reset()
	{
		this.value = 0f;
	}

	// Token: 0x06001837 RID: 6199 RVA: 0x000AB01E File Offset: 0x000A941E
	public override void SetContextValues(ModelCategorizerContext modelCategorizerContext)
	{
		modelCategorizerContext.SetFloat(this.name, this.value);
	}

	// Token: 0x04001313 RID: 4883
	protected float value;
}
