using System;
using UnityEngine;

// Token: 0x020001FC RID: 508
[RequireComponent(typeof(Camera))]
public class CameraFlashEffect : MonoBehaviour
{
	// Token: 0x06001A13 RID: 6675 RVA: 0x000C1124 File Offset: 0x000BF524
	protected void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		this.shaderRGB = Shader.Find("Blocksworld/CameraFlashEffect");
		if (this.shaderRGB == null)
		{
			BWLog.Info("Shader is not set up!");
			base.enabled = false;
		}
		else if (!this.shaderRGB.isSupported)
		{
			BWLog.Info("Shader not supported!");
			base.enabled = false;
		}
	}

	// Token: 0x1700007A RID: 122
	// (get) Token: 0x06001A14 RID: 6676 RVA: 0x000C119B File Offset: 0x000BF59B
	protected Material material
	{
		get
		{
			if (this.materialRGB == null)
			{
				this.materialRGB = new Material(this.shaderRGB);
				this.materialRGB.hideFlags = HideFlags.HideAndDontSave;
			}
			return this.materialRGB;
		}
	}

	// Token: 0x06001A15 RID: 6677 RVA: 0x000C11D2 File Offset: 0x000BF5D2
	protected void CleanupMaterial()
	{
		if (this.materialRGB != null)
		{
			UnityEngine.Object.DestroyImmediate(this.materialRGB);
			this.materialRGB = null;
		}
	}

	// Token: 0x06001A16 RID: 6678 RVA: 0x000C11F7 File Offset: 0x000BF5F7
	protected void OnDisable()
	{
		this.CleanupMaterial();
	}

	// Token: 0x06001A17 RID: 6679 RVA: 0x000C11FF File Offset: 0x000BF5FF
	protected void OnDestroy()
	{
		this.CleanupMaterial();
	}

	// Token: 0x06001A18 RID: 6680 RVA: 0x000C1208 File Offset: 0x000BF608
	private void Update()
	{
		this.time += Time.deltaTime;
		if (this.time > this.duration && this.autoDestroy)
		{
			this.autoDestroy = false;
			Blocksworld.capturingScreenshot = false;
			UnityEngine.Object.Destroy(this);
		}
	}

	// Token: 0x06001A19 RID: 6681 RVA: 0x000C1258 File Offset: 0x000BF658
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material material = this.material;
		material.SetColor("_Color", this.color);
		material.SetFloat("_EffectTime", this.time);
		material.SetFloat("_EffectDuration", this.duration);
		Graphics.Blit(source, destination, material);
	}

	// Token: 0x0400159B RID: 5531
	public Color color = Color.white;

	// Token: 0x0400159C RID: 5532
	public float time;

	// Token: 0x0400159D RID: 5533
	public float duration = 0.2f;

	// Token: 0x0400159E RID: 5534
	public bool autoDestroy;

	// Token: 0x0400159F RID: 5535
	public Shader shaderRGB;

	// Token: 0x040015A0 RID: 5536
	private Material materialRGB;
}
