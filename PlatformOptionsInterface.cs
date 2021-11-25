using System;

// Token: 0x02000255 RID: 597
public class PlatformOptionsInterface : IPlatformOptions
{
	// Token: 0x06001B2D RID: 6957 RVA: 0x000C666B File Offset: 0x000C4A6B
	public virtual bool useTouch()
	{
		return false;
	}

	// Token: 0x06001B2E RID: 6958 RVA: 0x000C666E File Offset: 0x000C4A6E
	public virtual bool useMouse()
	{
		return false;
	}

	// Token: 0x06001B2F RID: 6959 RVA: 0x000C6671 File Offset: 0x000C4A71
	public virtual bool useScarcity()
	{
		return true;
	}

	// Token: 0x06001B30 RID: 6960 RVA: 0x000C6674 File Offset: 0x000C4A74
	public virtual bool interpolateRigidbodies()
	{
		return false;
	}

	// Token: 0x06001B31 RID: 6961 RVA: 0x000C6677 File Offset: 0x000C4A77
	public virtual bool saveOnApplicationQuit()
	{
		return false;
	}

	// Token: 0x06001B32 RID: 6962 RVA: 0x000C667A File Offset: 0x000C4A7A
	public virtual bool saveOnWorldExit()
	{
		return false;
	}

	// Token: 0x06001B33 RID: 6963 RVA: 0x000C667D File Offset: 0x000C4A7D
	public virtual bool fixedScreenSize()
	{
		return false;
	}

	// Token: 0x06001B34 RID: 6964 RVA: 0x000C6680 File Offset: 0x000C4A80
	public virtual float GetScreenScale()
	{
		return 1f;
	}

	// Token: 0x06001B35 RID: 6965 RVA: 0x000C6687 File Offset: 0x000C4A87
	public virtual bool ShouldHideTileButton(TILE_BUTTON button)
	{
		return false;
	}

	// Token: 0x06001B36 RID: 6966 RVA: 0x000C668A File Offset: 0x000C4A8A
	public virtual string UIPrefabPath()
	{
		return "UnityUI/UIMain_SD";
	}
}
