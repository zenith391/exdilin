using System;
using System.Collections.Generic;
using Blocks;
using UnityEngine;

// Token: 0x02000131 RID: 305
public class DelayedProfilePictureCaptureCommand : Command
{
	// Token: 0x06001427 RID: 5159 RVA: 0x0008CC08 File Offset: 0x0008B008
	private void CalculateProfileModelBounds()
	{
		if (this.profileModel == null)
		{
			foreach (Block block in BWSceneManager.AllBlocks())
			{
				if (block.IsProfileCharacter())
				{
					block.UpdateConnectedCache();
					this.profileModel = Block.connectedCache[block];
					this.modelBounds = Util.ComputeBoundsDetailed(this.profileModel);
					break;
				}
			}
			if (this.profileModel == null)
			{
				BWLog.Info("Could not find the profile character");
			}
		}
	}

	// Token: 0x06001428 RID: 5160 RVA: 0x0008CCB8 File Offset: 0x0008B0B8
	public override void Execute()
	{
		this.CalculateProfileModelBounds();
		Camera mainCamera = Blocksworld.mainCamera;
		BlocksworldCamera blocksworldCamera = Blocksworld.blocksworldCamera;
		if (!this.modifiedFov)
		{
			this.AdaptCameraToTargetSize(mainCamera, blocksworldCamera);
		}
		if (this.timeCounter == 0)
		{
			this.buttonsShowingBeforeCapture.Clear();
			List<Tile> list = new List<Tile>();
			foreach (Tile tile in list)
			{
				if (tile != null)
				{
					this.buttonsShowingBeforeCapture[tile] = tile.IsShowing();
					tile.Show(false);
				}
			}
		}
		this.timeCounter++;
		float num = (float)this.timeCounter * Blocksworld.fixedDeltaTime;
		if (num < this.flashTime)
		{
			Blocksworld.UI.Overlay.ShowOnScreenMessage("Pose for the camera!");
		}
		else
		{
			Blocksworld.UI.Overlay.HideOnScreenMessage();
		}
		if (num >= this.flashTime && this.prevTime < this.flashTime)
		{
			Sound.PlayOneShotSound("Button Play", 1f);
			this.effect = Blocksworld.guiCamera.gameObject.AddComponent<CameraFlashEffect>();
		}
		else if (num >= this.screenshotTime)
		{
			if (!this.screenshotTaken)
			{
				WorldSession.current.TakeScreenshot();
				this.screenshotTaken = true;
			}
			else if (!Blocksworld.lockInput)
			{
				this.done = true;
				this.RestoreCamera(mainCamera, blocksworldCamera);
				this.CleanUp();
				Blocksworld.stopASAP = true;
			}
		}
		this.prevTime = num;
	}

	// Token: 0x06001429 RID: 5161 RVA: 0x0008CE60 File Offset: 0x0008B260
	private void CleanUp()
	{
		if (this.effect != null)
		{
			UnityEngine.Object.Destroy(this.effect);
			this.effect = null;
		}
		foreach (KeyValuePair<Tile, bool> keyValuePair in this.buttonsShowingBeforeCapture)
		{
			keyValuePair.Key.Show(keyValuePair.Value);
		}
		this.buttonsShowingBeforeCapture.Clear();
	}

	// Token: 0x0600142A RID: 5162 RVA: 0x0008CEF8 File Offset: 0x0008B2F8
	private void RestoreCamera(Camera cam, BlocksworldCamera bwCam)
	{
		if (this.modifiedFov)
		{
			cam.nearClipPlane = this.near;
			cam.farClipPlane = this.far;
			cam.aspect = this.aspect;
			cam.fieldOfView = this.fov;
			cam.projectionMatrix = Matrix4x4.Perspective(this.fov, this.aspect, this.near, this.far);
		}
		bwCam.SetLookAtOffset(Vector3.zero);
		bwCam.SetMoveToOffset(Vector3.zero);
	}

	// Token: 0x0600142B RID: 5163 RVA: 0x0008CF7C File Offset: 0x0008B37C
	private void AdaptCameraToTargetSize(Camera cam, BlocksworldCamera bwCam)
	{
		this.fov = cam.fieldOfView;
		this.near = cam.nearClipPlane;
		this.far = cam.farClipPlane;
		this.aspect = cam.aspect;
		this.newFov = Mathf.Clamp(this.fov + (this.modelBounds.size.y - 3f) * 10f, 45f, 75f);
		cam.projectionMatrix = Matrix4x4.Perspective(this.newFov, this.aspect, 1.5f, this.far);
		float num = Mathf.Clamp((this.modelBounds.size.y - 2.5f) * 0.5f, 0f, 2f);
		bwCam.SetLookAtOffset(Vector3.up * num);
		bwCam.SetMoveToOffset(Vector3.up * Mathf.Max(num * 0.75f, 0f));
		this.modifiedFov = true;
	}

	// Token: 0x0600142C RID: 5164 RVA: 0x0008D07E File Offset: 0x0008B47E
	public override void Removed()
	{
		this.CleanUp();
		base.Removed();
	}

	// Token: 0x04000FC2 RID: 4034
	private float flashTime = 1.2f;

	// Token: 0x04000FC3 RID: 4035
	private float screenshotTime = 1.8f;

	// Token: 0x04000FC4 RID: 4036
	private int timeCounter;

	// Token: 0x04000FC5 RID: 4037
	private bool modifiedFov;

	// Token: 0x04000FC6 RID: 4038
	private float fov;

	// Token: 0x04000FC7 RID: 4039
	private float newFov;

	// Token: 0x04000FC8 RID: 4040
	private float near;

	// Token: 0x04000FC9 RID: 4041
	private float far;

	// Token: 0x04000FCA RID: 4042
	private float aspect;

	// Token: 0x04000FCB RID: 4043
	private bool screenshotTaken;

	// Token: 0x04000FCC RID: 4044
	private CameraFlashEffect effect;

	// Token: 0x04000FCD RID: 4045
	private Bounds modelBounds = new Bounds(Vector3.zero, Vector3.one);

	// Token: 0x04000FCE RID: 4046
	private List<Block> profileModel;

	// Token: 0x04000FCF RID: 4047
	private const float MAX_SIZE_BEFORE_FOV_CHANGE = 3f;

	// Token: 0x04000FD0 RID: 4048
	private const float MAX_SIZE_BEFORE_POS_CHANGE = 2.5f;

	// Token: 0x04000FD1 RID: 4049
	private Dictionary<Tile, bool> buttonsShowingBeforeCapture = new Dictionary<Tile, bool>();

	// Token: 0x04000FD2 RID: 4050
	private float prevTime;
}
