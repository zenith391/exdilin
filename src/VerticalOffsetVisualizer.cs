using Blocks;
using UnityEngine;

public class VerticalOffsetVisualizer : ParameterValueVisualizer
{
	protected static GameObject visualizerPrefab;

	public override void Update()
	{
		Block block = Blocksworld.selectedBlock;
		bool flag = false;
		bool flag2 = false;
		if (block is BlockMaster)
		{
			block = Blocksworld.worldOceanBlock;
			flag = true;
		}
		if (block is BlockPiston)
		{
			flag2 = true;
		}
		if (block == null)
		{
			return;
		}
		if (visualizerGo == null)
		{
			if (visualizerPrefab == null)
			{
				visualizerPrefab = Resources.Load("GUI/Vertical Offset Visualizer") as GameObject;
			}
			visualizerGo = Object.Instantiate(visualizerPrefab);
		}
		EditableTileParameter parameter = Blocksworld.bw.tileParameterEditor.parameter;
		float num = (float)parameter.objectValue;
		GameObject go = block.go;
		Transform transform = go.transform;
		Vector3 position = transform.position;
		Bounds bounds = go.GetComponent<Collider>().bounds;
		Vector3 vector = parameter.settings.verticalOffsetVisualizerDirection.VectorValue();
		if (flag)
		{
			float y = bounds.extents.y;
			Vector3 localScale = visualizerGo.transform.localScale;
			visualizerGo.transform.position = position + Vector3.up * y + vector * num;
			visualizerGo.transform.localScale = new Vector3(bounds.size.x + 2f, localScale.y, bounds.size.z + 2f);
		}
		else if (flag2)
		{
			visualizerGo.transform.rotation = transform.rotation;
			Vector3 vector2 = block.Scale();
			for (int i = 0; i < 3; i++)
			{
				if (vector2[i] + vector[i] * num < 0f)
				{
					num = vector2[i];
				}
			}
			Vector3 vector3 = block.Scale() + vector * num;
			Vector3 vector4 = Vector3.one - Util.Abs(vector);
			visualizerGo.transform.localScale = vector3 - vector4 * 0.2f;
			visualizerGo.transform.position = position + num * visualizerGo.transform.TransformDirection(vector) * 0.5f;
		}
		else
		{
			Vector3 size = bounds.size;
			size.y = Mathf.Max(0.05f, size.y + num * vector.y);
			size.x -= Mathf.Clamp(size.x * 0.1f, 0.1f, 0.2f);
			size.z -= Mathf.Clamp(size.y * 0.1f, 0.1f, 0.2f);
			visualizerGo.transform.localScale = size;
			visualizerGo.transform.position = position + Mathf.Max(num * vector.y, 0f - bounds.size.y) * 0.5f * Vector3.up;
		}
	}
}
