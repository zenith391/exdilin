using Blocks;

public class SkyBoxRotationVisualizer : ParameterValueVisualizer
{
	public override void Update()
	{
		EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
		float angle = (float)parameter.objectValue;
		WorldEnvironmentManager.OverrideSkyBoxRotationTemporarily(angle);
	}

	public override void Destroy()
	{
		WorldEnvironmentManager.RevertToAssignedSkyBoxRotation();
		base.Destroy();
	}
}
