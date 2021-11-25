using System;

// Token: 0x02000256 RID: 598
public class PlatformOptions_Standalone : PlatformOptionsInterface
{
	// Token: 0x06001B38 RID: 6968 RVA: 0x000C6699 File Offset: 0x000C4A99
	public override bool useMouse()
	{
		return true;
	}

	// Token: 0x06001B39 RID: 6969 RVA: 0x000C669C File Offset: 0x000C4A9C
	public override bool useScarcity()
	{
		return true;
	}

	// Token: 0x06001B3A RID: 6970 RVA: 0x000C669F File Offset: 0x000C4A9F
	public override bool interpolateRigidbodies()
	{
		return Options.InterpolateRigidBodies;
	}

	// Token: 0x06001B3B RID: 6971 RVA: 0x000C66A6 File Offset: 0x000C4AA6
	public override bool saveOnWorldExit()
	{
		return true;
	}

	// Token: 0x06001B3C RID: 6972 RVA: 0x000C66A9 File Offset: 0x000C4AA9
	public override float GetScreenScale()
	{
		return 1f;
	}

	// Token: 0x06001B3D RID: 6973 RVA: 0x000C66B0 File Offset: 0x000C4AB0
	public override string UIPrefabPath()
	{
		return "UnityUI/UIMain_Standalone";
	}
}
