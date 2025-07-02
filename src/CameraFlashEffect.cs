using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFlashEffect : MonoBehaviour
{
	public Color color = Color.white;

	public float time;

	public float duration = 0.2f;

	public bool autoDestroy;

	public Shader shaderRGB;

	private Material materialRGB;

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

	protected void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		shaderRGB = Shader.Find("Blocksworld/CameraFlashEffect");
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

	protected void CleanupMaterial()
	{
		if (materialRGB != null)
		{
			Object.DestroyImmediate(materialRGB);
			materialRGB = null;
		}
	}

	protected void OnDisable()
	{
		CleanupMaterial();
	}

	protected void OnDestroy()
	{
		CleanupMaterial();
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (time > duration && autoDestroy)
		{
			autoDestroy = false;
			Blocksworld.capturingScreenshot = false;
			Object.Destroy(this);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Material material = this.material;
		material.SetColor("_Color", color);
		material.SetFloat("_EffectTime", time);
		material.SetFloat("_EffectDuration", duration);
		Graphics.Blit(source, destination, material);
	}
}
