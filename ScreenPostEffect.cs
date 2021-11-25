using System;
using UnityEngine;

// Token: 0x02000222 RID: 546
[RequireComponent(typeof(Camera))]
public class ScreenPostEffect : MonoBehaviour
{
	// Token: 0x06001A8D RID: 6797 RVA: 0x0000A49A File Offset: 0x0000889A
	protected virtual string GetShaderName()
	{
		return string.Empty;
	}

	// Token: 0x06001A8E RID: 6798 RVA: 0x0000A4A4 File Offset: 0x000088A4
	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		this.shaderRGB = Shader.Find(this.GetShaderName());
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

	// Token: 0x1700007C RID: 124
	// (get) Token: 0x06001A8F RID: 6799 RVA: 0x0000A51C File Offset: 0x0000891C
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

	// Token: 0x06001A90 RID: 6800 RVA: 0x0000A553 File Offset: 0x00008953
	protected void OnDisable()
	{
		if (this.materialRGB)
		{
			UnityEngine.Object.DestroyImmediate(this.materialRGB);
		}
	}

	// Token: 0x06001A91 RID: 6801 RVA: 0x0000A570 File Offset: 0x00008970
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material material = this.material;
		Graphics.Blit(source, destination, material);
	}

	// Token: 0x04001633 RID: 5683
	public Shader shaderRGB;

	// Token: 0x04001634 RID: 5684
	protected Material materialRGB;
}
