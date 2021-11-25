using System;
using Blocks;

// Token: 0x0200023C RID: 572
public class SkyBoxRotationVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AF4 RID: 6900 RVA: 0x000C5A64 File Offset: 0x000C3E64
	public override void Update()
	{
		EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
		float angle = (float)parameter.objectValue;
		WorldEnvironmentManager.OverrideSkyBoxRotationTemporarily(angle);
	}

	// Token: 0x06001AF5 RID: 6901 RVA: 0x000C5A93 File Offset: 0x000C3E93
	public override void Destroy()
	{
		WorldEnvironmentManager.RevertToAssignedSkyBoxRotation();
		base.Destroy();
	}
}
