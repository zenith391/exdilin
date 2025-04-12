using System;
using Blocks;
using UnityEngine;

// Token: 0x02000239 RID: 569
public class VerticalOffsetVisualizer : ParameterValueVisualizer
{
	// Token: 0x06001AEE RID: 6894 RVA: 0x000C53B4 File Offset: 0x000C37B4
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
		if (this.visualizerGo == null)
		{
			if (VerticalOffsetVisualizer.visualizerPrefab == null)
			{
				VerticalOffsetVisualizer.visualizerPrefab = (Resources.Load("GUI/Vertical Offset Visualizer") as GameObject);
			}
			this.visualizerGo = UnityEngine.Object.Instantiate<GameObject>(VerticalOffsetVisualizer.visualizerPrefab);
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
			Vector3 localScale = this.visualizerGo.transform.localScale;
			this.visualizerGo.transform.position = position + Vector3.up * y + vector * num;
			this.visualizerGo.transform.localScale = new Vector3(bounds.size.x + 2f, localScale.y, bounds.size.z + 2f);
		}
		else if (flag2)
		{
			this.visualizerGo.transform.rotation = transform.rotation;
			Vector3 vector2 = block.Scale();
			for (int i = 0; i < 3; i++)
			{
				if (vector2[i] + vector[i] * num < 0f)
				{
					num = vector2[i];
				}
			}
			Vector3 a = block.Scale() + vector * num;
			Vector3 a2 = Vector3.one - Util.Abs(vector);
			this.visualizerGo.transform.localScale = a - a2 * 0.2f;
			this.visualizerGo.transform.position = position + num * this.visualizerGo.transform.TransformDirection(vector) * 0.5f;
		}
		else
		{
			Vector3 size = bounds.size;
			size.y = Mathf.Max(0.05f, size.y + num * vector.y);
			size.x -= Mathf.Clamp(size.x * 0.1f, 0.1f, 0.2f);
			size.z -= Mathf.Clamp(size.y * 0.1f, 0.1f, 0.2f);
			this.visualizerGo.transform.localScale = size;
			this.visualizerGo.transform.position = position + Mathf.Max(num * vector.y, -bounds.size.y) * 0.5f * Vector3.up;
		}
	}

	// Token: 0x04001690 RID: 5776
	protected static GameObject visualizerPrefab;
}
