using UnityEngine;

[AddComponentMenu("Cardboard/Audio/CardboardAudioRoom")]
public class CardboardAudioRoom : MonoBehaviour
{
	public enum SurfaceMaterial
	{
		Transparent,
		AcousticCeilingTiles,
		BrickBare,
		BrickPainted,
		ConcreteBlockCoarse,
		ConcreteBlockPainted,
		CurtainHeavy,
		FiberglassInsulation,
		GlassThin,
		GlassThick,
		Grass,
		LinoleumOnConcrete,
		Marble,
		ParquetOnConcrete,
		PlasterRough,
		PlasterSmooth,
		PlywoodPanel,
		PolishedConcreteOrTile,
		Sheetrock,
		WaterOrIceSurface,
		WoodCeiling,
		WoodPanel
	}

	public SurfaceMaterial leftWall = SurfaceMaterial.ConcreteBlockCoarse;

	public SurfaceMaterial rightWall = SurfaceMaterial.ConcreteBlockCoarse;

	public SurfaceMaterial floor = SurfaceMaterial.ParquetOnConcrete;

	public SurfaceMaterial ceiling = SurfaceMaterial.PlasterRough;

	public SurfaceMaterial backWall = SurfaceMaterial.ConcreteBlockCoarse;

	public SurfaceMaterial frontWall = SurfaceMaterial.ConcreteBlockCoarse;

	public float reflectivity = 1f;

	public float reverbGainDb;

	public float reverbBrightness;

	public float reverbTime = 1f;

	public Vector3 size = Vector3.one;

	private int id = -1;

	private SurfaceMaterial[] surfaceMaterials;

	private void Awake()
	{
		surfaceMaterials = new SurfaceMaterial[6];
	}

	private void OnEnable()
	{
		InitializeRoom();
	}

	private void Start()
	{
		InitializeRoom();
	}

	private void OnDisable()
	{
		ShutdownRoom();
	}

	private void Update()
	{
		CardboardAudio.UpdateAudioRoom(id, base.transform, GetSurfaceMaterials(), reflectivity, reverbGainDb, reverbBrightness, reverbTime, size);
	}

	public SurfaceMaterial[] GetSurfaceMaterials()
	{
		surfaceMaterials[0] = leftWall;
		surfaceMaterials[1] = rightWall;
		surfaceMaterials[2] = floor;
		surfaceMaterials[3] = ceiling;
		surfaceMaterials[4] = backWall;
		surfaceMaterials[5] = frontWall;
		return surfaceMaterials;
	}

	private void InitializeRoom()
	{
		if (id < 0)
		{
			id = CardboardAudio.CreateAudioRoom();
			CardboardAudio.UpdateAudioRoom(id, base.transform, GetSurfaceMaterials(), reflectivity, reverbGainDb, reverbBrightness, reverbTime, size);
		}
	}

	private void ShutdownRoom()
	{
		if (id >= 0)
		{
			CardboardAudio.DestroyAudioRoom(id);
			id = -1;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, size);
	}
}
