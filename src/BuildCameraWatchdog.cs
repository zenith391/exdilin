using System;
using UnityEngine;

// Token: 0x02000100 RID: 256
public class BuildCameraWatchdog
{
	// Token: 0x0600128D RID: 4749 RVA: 0x00081AA9 File Offset: 0x0007FEA9
	public BuildCameraWatchdog()
	{
		this.worldBounds = this.GetDefaultWorldBounds();
	}

	// Token: 0x0600128E RID: 4750 RVA: 0x00081ABD File Offset: 0x0007FEBD
	public void SetWorldBounds(Bounds b)
	{
		this.worldBounds = b;
		this.UpdateTestBounds();
	}

	// Token: 0x0600128F RID: 4751 RVA: 0x00081ACC File Offset: 0x0007FECC
	private void UpdateTestBounds()
	{
		this.testBounds = this.worldBounds;
		this.testBounds.Expand(400f);
	}

	// Token: 0x06001290 RID: 4752 RVA: 0x00081AEA File Offset: 0x0007FEEA
	public void EncapsulateWorldBounds(Bounds b)
	{
		this.worldBounds.Encapsulate(b);
		this.UpdateTestBounds();
	}

	// Token: 0x06001291 RID: 4753 RVA: 0x00081AFE File Offset: 0x0007FEFE
	public Bounds GetWorldBounds()
	{
		return this.worldBounds;
	}

	// Token: 0x06001292 RID: 4754 RVA: 0x00081B06 File Offset: 0x0007FF06
	public Bounds GetDefaultWorldBounds()
	{
		return new Bounds(Vector3.zero, Vector3.one * 150f);
	}

	// Token: 0x06001293 RID: 4755 RVA: 0x00081B21 File Offset: 0x0007FF21
	public void Reset(bool boundsToo = true)
	{
		this.timeWithinTerrain = 0f;
		this.cameraResetVisible = false;
		if (boundsToo)
		{
			this.worldBounds = this.GetDefaultWorldBounds();
		}
	}

	// Token: 0x06001294 RID: 4756 RVA: 0x00081B48 File Offset: 0x0007FF48
	public void Update()
	{
		if (Blocksworld.CurrentState == State.Build && Blocksworld.IsStarted() && WorldSession.current != null && WorldSession.current.worldLoadComplete)
		{
			Vector3 position = Blocksworld.cameraTransform.position;
			if (Util.PointWithinTerrain(position, false))
			{
				this.timeWithinTerrain += Time.deltaTime;
			}
			else
			{
				this.timeWithinTerrain = 0f;
			}
			this.cameraResetVisible = (this.timeWithinTerrain > 5f || !this.testBounds.Contains(position));
		}
		else
		{
			this.cameraResetVisible = false;
		}
		if (this.cameraResetVisible)
		{
			float scale = NormalizedScreen.scale;
			float num = 200f * scale;
			float num2 = 50f * scale;
			Rect rect = new Rect((float)Screen.width * 0.5f - num * 0.5f, (float)Screen.height - num2 - 10f * scale, num, num2);
			if (this.resetButtonStyle == null)
			{
				this.resetButtonStyle = HudMeshOnGUI.dataSource.GetStyle("Build Mode Dialog Button");
			}
			if (HudMeshOnGUI.Button(ref this.resetButton, rect, "Reset Camera", this.resetButtonStyle))
			{
				BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
				Transform cameraTransform = Blocksworld.cameraTransform;
				blocksworldCamera.Unfollow();
				Bounds bounds = this.GetWorldBounds();
				Vector3 vector = bounds.center + bounds.size.y * 0.5f * Vector3.up;
				Vector3 vector2 = new Vector3(0f, -1f, 1f);
				Vector3 a = vector2.normalized;
				foreach (NamedPose namedPose in Blocksworld.cameraPoses)
				{
					if (namedPose.name == "Camera Reset Pose")
					{
						vector = namedPose.position;
						a = namedPose.direction;
						break;
					}
				}
				cameraTransform.position = vector;
				Vector3 vector3 = vector + a * 15f;
				cameraTransform.LookAt(vector3);
				blocksworldCamera.SetTargetPosition(vector3);
				blocksworldCamera.SetTargetDistance(15f);
				this.Reset(false);
			}
		}
	}

	// Token: 0x06001295 RID: 4757 RVA: 0x00081DA0 File Offset: 0x000801A0
	public bool CameraResetVisible()
	{
		return this.cameraResetVisible;
	}

	// Token: 0x04000EFB RID: 3835
	private bool cameraResetVisible;

	// Token: 0x04000EFC RID: 3836
	private float timeWithinTerrain;

	// Token: 0x04000EFD RID: 3837
	private Bounds worldBounds;

	// Token: 0x04000EFE RID: 3838
	private Bounds testBounds;

	// Token: 0x04000EFF RID: 3839
	private HudMeshButton resetButton;

	// Token: 0x04000F00 RID: 3840
	private HudMeshStyle resetButtonStyle;

	// Token: 0x04000F01 RID: 3841
	private const float MAX_TIME_WITHIN_TERRAIN = 5f;

	// Token: 0x04000F02 RID: 3842
	private const float DEFAULT_WORLD_BOUNDS_SIZE = 150f;
}
