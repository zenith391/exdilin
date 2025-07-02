using System;
using UnityEngine;

[AddComponentMenu("Cardboard/UI/CardboardReticle")]
[RequireComponent(typeof(Renderer))]
public class CardboardReticle : MonoBehaviour, ICardboardPointer
{
	public int reticleSegments = 20;

	public float reticleGrowthSpeed = 8f;

	private Material materialComp;

	private GameObject targetObj;

	private float reticleInnerAngle;

	private float reticleOuterAngle = 0.5f;

	private float reticleDistanceInMeters = 10f;

	private const float kReticleMinInnerAngle = 0f;

	private const float kReticleMinOuterAngle = 0.5f;

	private const float kReticleGrowthAngle = 1.5f;

	private const float kReticleDistanceMin = 0.75f;

	private const float kReticleDistanceMax = 10f;

	private float reticleInnerDiameter;

	private float reticleOuterDiameter;

	private void Start()
	{
		CreateReticleVertices();
		materialComp = base.gameObject.GetComponent<Renderer>().material;
	}

	private void OnEnable()
	{
		GazeInputModule.cardboardPointer = this;
	}

	private void OnDisable()
	{
		if (GazeInputModule.cardboardPointer == this)
		{
			GazeInputModule.cardboardPointer = null;
		}
	}

	private void Update()
	{
		UpdateDiameters();
	}

	public void OnGazeEnabled()
	{
	}

	public void OnGazeDisabled()
	{
	}

	public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition)
	{
		SetGazeTarget(intersectionPosition);
	}

	public void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition)
	{
		SetGazeTarget(intersectionPosition);
	}

	public void OnGazeExit(Camera camera, GameObject targetObject)
	{
		reticleDistanceInMeters = 10f;
		reticleInnerAngle = 0f;
		reticleOuterAngle = 0.5f;
	}

	public void OnGazeTriggerStart(Camera camera)
	{
	}

	public void OnGazeTriggerEnd(Camera camera)
	{
	}

	private void CreateReticleVertices()
	{
		Mesh mesh = new Mesh();
		base.gameObject.AddComponent<MeshFilter>();
		GetComponent<MeshFilter>().mesh = mesh;
		int num = reticleSegments;
		int num2 = (num + 1) * 2;
		Vector3[] array = new Vector3[num2];
		int num3 = 0;
		for (int i = 0; i <= num; i++)
		{
			float f = (float)i / (float)num * ((float)Math.PI * 2f);
			float x = Mathf.Sin(f);
			float y = Mathf.Cos(f);
			array[num3++] = new Vector3(x, y, 0f);
			array[num3++] = new Vector3(x, y, 1f);
		}
		int num4 = (num + 1) * 3 * 2;
		int[] array2 = new int[num4];
		int num5 = 0;
		int num6 = 0;
		for (int j = 0; j < num; j++)
		{
			array2[num6++] = num5 + 1;
			array2[num6++] = num5;
			array2[num6++] = num5 + 2;
			array2[num6++] = num5 + 1;
			array2[num6++] = num5 + 2;
			array2[num6++] = num5 + 3;
			num5 += 2;
		}
		mesh.vertices = array;
		mesh.triangles = array2;
		mesh.RecalculateBounds();
	}

	private void UpdateDiameters()
	{
		reticleDistanceInMeters = Mathf.Clamp(reticleDistanceInMeters, 0.75f, 10f);
		if (reticleInnerAngle < 0f)
		{
			reticleInnerAngle = 0f;
		}
		if (reticleOuterAngle < 0.5f)
		{
			reticleOuterAngle = 0.5f;
		}
		float f = (float)Math.PI / 180f * reticleInnerAngle * 0.5f;
		float f2 = (float)Math.PI / 180f * reticleOuterAngle * 0.5f;
		float b = 2f * Mathf.Tan(f);
		float b2 = 2f * Mathf.Tan(f2);
		reticleInnerDiameter = Mathf.Lerp(reticleInnerDiameter, b, Time.deltaTime * reticleGrowthSpeed);
		reticleOuterDiameter = Mathf.Lerp(reticleOuterDiameter, b2, Time.deltaTime * reticleGrowthSpeed);
		materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * reticleDistanceInMeters);
		materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * reticleDistanceInMeters);
		materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
	}

	private void SetGazeTarget(Vector3 target)
	{
		reticleDistanceInMeters = Mathf.Clamp(base.transform.parent.InverseTransformPoint(target).z, 0.75f, 10f);
		reticleInnerAngle = 1.5f;
		reticleOuterAngle = 2f;
	}
}
