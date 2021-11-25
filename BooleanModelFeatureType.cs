using System;
using SimpleJSON;

// Token: 0x020001C4 RID: 452
public class BooleanModelFeatureType : ModelFeatureType
{
	// Token: 0x0600181B RID: 6171 RVA: 0x000AAA45 File Offset: 0x000A8E45
	public override void Reset()
	{
		this.triggered = false;
	}

	// Token: 0x0600181C RID: 6172 RVA: 0x000AAA4E File Offset: 0x000A8E4E
	public bool IsTriggered()
	{
		return this.triggered;
	}

	// Token: 0x0600181D RID: 6173 RVA: 0x000AAA56 File Offset: 0x000A8E56
	public override void ToJSON(JSONStreamEncoder encoder)
	{
		if (this.triggered)
		{
			base.ToJSON(encoder);
		}
	}

	// Token: 0x0600181E RID: 6174 RVA: 0x000AAA6A File Offset: 0x000A8E6A
	public override void SetContextValues(ModelCategorizerContext modelCategorizerContext)
	{
		modelCategorizerContext.SetBool(this.name, this.triggered);
	}

	// Token: 0x0400130C RID: 4876
	protected bool triggered;
}
