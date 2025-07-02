public class PlatformOptionsInterface : IPlatformOptions
{
	public virtual bool useTouch()
	{
		return false;
	}

	public virtual bool useMouse()
	{
		return false;
	}

	public virtual bool useScarcity()
	{
		return true;
	}

	public virtual bool interpolateRigidbodies()
	{
		return false;
	}

	public virtual bool saveOnApplicationQuit()
	{
		return false;
	}

	public virtual bool saveOnWorldExit()
	{
		return false;
	}

	public virtual bool fixedScreenSize()
	{
		return false;
	}

	public virtual float GetScreenScale()
	{
		return 1f;
	}

	public virtual bool ShouldHideTileButton(TILE_BUTTON button)
	{
		return false;
	}

	public virtual string UIPrefabPath()
	{
		return "UnityUI/UIMain_SD";
	}
}
