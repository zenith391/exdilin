using UnityEngine;

public class BillboardParameters : MonoBehaviour
{
	public bool snapHorizon;

	public bool mirrorInWater = true;

	public bool ignoreFog;

	public float realDistance = 300f;

	public float apparentDistance = 300f;

	public bool parallax;

	public Vector3 parallaxMin = Vector3.zero;

	public Vector3 parallaxMax = Vector3.zero;

	public bool showLensflare;

	public float lensFlareBrightness = 0.5f;

	public Color lensFlareColor = Color.white;

	public bool blendLensFlareWithLight = true;
}
