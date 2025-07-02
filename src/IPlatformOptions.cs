public interface IPlatformOptions
{
	bool useTouch();

	bool useMouse();

	bool useScarcity();

	bool interpolateRigidbodies();

	bool saveOnApplicationQuit();

	bool saveOnWorldExit();

	bool fixedScreenSize();

	float GetScreenScale();

	bool ShouldHideTileButton(TILE_BUTTON button);

	string UIPrefabPath();
}
