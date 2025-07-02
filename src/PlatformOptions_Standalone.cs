public class PlatformOptions_Standalone : PlatformOptionsInterface
{
	public override bool useMouse()
	{
		return true;
	}

	public override bool useScarcity()
	{
		return true;
	}

	public override bool interpolateRigidbodies()
	{
		return Options.InterpolateRigidBodies;
	}

	public override bool saveOnWorldExit()
	{
		return true;
	}

	public override float GetScreenScale()
	{
		return 1f;
	}

	public override string UIPrefabPath()
	{
		return "UnityUI/UIMain_Standalone";
	}
}
