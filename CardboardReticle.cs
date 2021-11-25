using System;
using UnityEngine;

// Token: 0x02000029 RID: 41
[AddComponentMenu("Cardboard/UI/CardboardReticle")]
[RequireComponent(typeof(Renderer))]
public class CardboardReticle : MonoBehaviour, ICardboardPointer
{
	// Token: 0x06000154 RID: 340 RVA: 0x00008F9B File Offset: 0x0000739B
	private void Start()
	{
		this.CreateReticleVertices();
		this.materialComp = base.gameObject.GetComponent<Renderer>().material;
	}

	// Token: 0x06000155 RID: 341 RVA: 0x00008FB9 File Offset: 0x000073B9
	private void OnEnable()
	{
		GazeInputModule.cardboardPointer = this;
	}

	// Token: 0x06000156 RID: 342 RVA: 0x00008FC1 File Offset: 0x000073C1
	private void OnDisable()
	{
		if (GazeInputModule.cardboardPointer == this)
		{
			GazeInputModule.cardboardPointer = null;
		}
	}

	// Token: 0x06000157 RID: 343 RVA: 0x00008FD4 File Offset: 0x000073D4
	private void Update()
	{
		this.UpdateDiameters();
	}

	// Token: 0x06000158 RID: 344 RVA: 0x00008FDC File Offset: 0x000073DC
	public void OnGazeEnabled()
	{
	}

	// Token: 0x06000159 RID: 345 RVA: 0x00008FDE File Offset: 0x000073DE
	public void OnGazeDisabled()
	{
	}

	// Token: 0x0600015A RID: 346 RVA: 0x00008FE0 File Offset: 0x000073E0
	public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition)
	{
		this.SetGazeTarget(intersectionPosition);
	}

	// Token: 0x0600015B RID: 347 RVA: 0x00008FE9 File Offset: 0x000073E9
	public void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition)
	{
		this.SetGazeTarget(intersectionPosition);
	}

	// Token: 0x0600015C RID: 348 RVA: 0x00008FF2 File Offset: 0x000073F2
	public void OnGazeExit(Camera camera, GameObject targetObject)
	{
		this.reticleDistanceInMeters = 10f;
		this.reticleInnerAngle = 0f;
		this.reticleOuterAngle = 0.5f;
	}

	// Token: 0x0600015D RID: 349 RVA: 0x00009015 File Offset: 0x00007415
	public void OnGazeTriggerStart(Camera camera)
	{
	}

	// Token: 0x0600015E RID: 350 RVA: 0x00009017 File Offset: 0x00007417
	public void OnGazeTriggerEnd(Camera camera)
	{
	}

	// Token: 0x0600015F RID: 351 RVA: 0x0000901C File Offset: 0x0000741C
	private void CreateReticleVertices()
	{
		Mesh mesh = new Mesh();
		base.gameObject.AddComponent<MeshFilter>();
		base.GetComponent<MeshFilter>().mesh = mesh;
		int num = this.reticleSegments;
		int num2 = (num + 1) * 2;
		Vector3[] array = new Vector3[num2];
		int num3 = 0;
		for (int i = 0; i <= num; i++)
		{
			float f = (float)i / (float)num * 6.28318548f;
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

	// Token: 0x06000160 RID: 352 RVA: 0x00009170 File Offset: 0x00007570
	private void UpdateDiameters()
	{
		this.reticleDistanceInMeters = Mathf.Clamp(this.reticleDistanceInMeters, 0.75f, 10f);
		if (this.reticleInnerAngle < 0f)
		{
			this.reticleInnerAngle = 0f;
		}
		if (this.reticleOuterAngle < 0.5f)
		{
			this.reticleOuterAngle = 0.5f;
		}
		float f = 0.0174532924f * this.reticleInnerAngle * 0.5f;
		float f2 = 0.0174532924f * this.reticleOuterAngle * 0.5f;
		float b = 2f * Mathf.Tan(f);
		float b2 = 2f * Mathf.Tan(f2);
		this.reticleInnerDiameter = Mathf.Lerp(this.reticleInnerDiameter, b, Time.deltaTime * this.reticleGrowthSpeed);
		this.reticleOuterDiameter = Mathf.Lerp(this.reticleOuterDiameter, b2, Time.deltaTime * this.reticleGrowthSpeed);
		this.materialComp.SetFloat("_InnerDiameter", this.reticleInnerDiameter * this.reticleDistanceInMeters);
		this.materialComp.SetFloat("_OuterDiameter", this.reticleOuterDiameter * this.reticleDistanceInMeters);
		this.materialComp.SetFloat("_DistanceInMeters", this.reticleDistanceInMeters);
	}

	// Token: 0x06000161 RID: 353 RVA: 0x0000929C File Offset: 0x0000769C
	private void SetGazeTarget(Vector3 target)
	{
		this.reticleDistanceInMeters = Mathf.Clamp(base.transform.parent.InverseTransformPoint(target).z, 0.75f, 10f);
		this.reticleInnerAngle = 1.5f;
		this.reticleOuterAngle = 2f;
	}

	// Token: 0x0400018C RID: 396
	public int reticleSegments = 20;

	// Token: 0x0400018D RID: 397
	public float reticleGrowthSpeed = 8f;

	// Token: 0x0400018E RID: 398
	private Material materialComp;

	// Token: 0x0400018F RID: 399
	private GameObject targetObj;

	// Token: 0x04000190 RID: 400
	private float reticleInnerAngle;

	// Token: 0x04000191 RID: 401
	private float reticleOuterAngle = 0.5f;

	// Token: 0x04000192 RID: 402
	private float reticleDistanceInMeters = 10f;

	// Token: 0x04000193 RID: 403
	private const float kReticleMinInnerAngle = 0f;

	// Token: 0x04000194 RID: 404
	private const float kReticleMinOuterAngle = 0.5f;

	// Token: 0x04000195 RID: 405
	private const float kReticleGrowthAngle = 1.5f;

	// Token: 0x04000196 RID: 406
	private const float kReticleDistanceMin = 0.75f;

	// Token: 0x04000197 RID: 407
	private const float kReticleDistanceMax = 10f;

	// Token: 0x04000198 RID: 408
	private float reticleInnerDiameter;

	// Token: 0x04000199 RID: 409
	private float reticleOuterDiameter;
}
