using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenPostEffect : MonoBehaviour
{
	public Shader shaderRGB;

	protected Material materialRGB;

	protected Material material
	{
		get
		{
			if (materialRGB == null)
			{
				materialRGB = new Material(shaderRGB);
				materialRGB.hideFlags = HideFlags.HideAndDontSave;
			}
			return materialRGB;
		}
	}

	protected virtual string GetShaderName()
	{
		return string.Empty;
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		shaderRGB = Shader.Find(GetShaderName());
		if (shaderRGB == null)
		{
			BWLog.Info("Shader is not set up!");
			base.enabled = false;
		}
		else if (!shaderRGB.isSupported)
		{
			BWLog.Info("Shader not supported!");
			base.enabled = false;
		}
	}

	protected void OnDisable()
	{
		if ((bool)materialRGB)
		{
			Object.DestroyImmediate(materialRGB);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material mat = material;
		Graphics.Blit(source, destination, mat);
	}
}
