using System;

// Token: 0x02000254 RID: 596
public interface IPlatformOptions
{
	// Token: 0x06001B22 RID: 6946
	bool useTouch();

	// Token: 0x06001B23 RID: 6947
	bool useMouse();

	// Token: 0x06001B24 RID: 6948
	bool useScarcity();

	// Token: 0x06001B25 RID: 6949
	bool interpolateRigidbodies();

	// Token: 0x06001B26 RID: 6950
	bool saveOnApplicationQuit();

	// Token: 0x06001B27 RID: 6951
	bool saveOnWorldExit();

	// Token: 0x06001B28 RID: 6952
	bool fixedScreenSize();

	// Token: 0x06001B29 RID: 6953
	float GetScreenScale();

	// Token: 0x06001B2A RID: 6954
	bool ShouldHideTileButton(TILE_BUTTON button);

	// Token: 0x06001B2B RID: 6955
	string UIPrefabPath();
}
