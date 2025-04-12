using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000374 RID: 884
	public class Benchmark04 : MonoBehaviour
	{
		// Token: 0x0600279B RID: 10139 RVA: 0x00122308 File Offset: 0x00120708
		private void Start()
		{
			this.m_Transform = base.transform;
			float num = 0f;
			float num2 = (float)(Screen.height / 2);
			Camera.main.orthographicSize = num2;
			float num3 = num2;
			float num4 = (float)Screen.width / (float)Screen.height;
			for (int i = this.MinPointSize; i <= this.MaxPointSize; i += this.Steps)
			{
				if (this.SpawnType == 0)
				{
					GameObject gameObject = new GameObject("Text - " + i + " Pts");
					if (num > num3 * 2f)
					{
						return;
					}
					gameObject.transform.position = this.m_Transform.position + new Vector3(num4 * -num3 * 0.975f, num3 * 0.975f - num, 0f);
					TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
					textMeshPro.rectTransform.pivot = new Vector2(0f, 0.5f);
					textMeshPro.enableWordWrapping = false;
					textMeshPro.extraPadding = true;
					textMeshPro.isOrthographic = true;
					textMeshPro.fontSize = (float)i;
					textMeshPro.text = i + " pts - Lorem ipsum dolor sit...";
					textMeshPro.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
					num += (float)i;
				}
			}
		}

		// Token: 0x04002272 RID: 8818
		public int SpawnType;

		// Token: 0x04002273 RID: 8819
		public int MinPointSize = 12;

		// Token: 0x04002274 RID: 8820
		public int MaxPointSize = 64;

		// Token: 0x04002275 RID: 8821
		public int Steps = 4;

		// Token: 0x04002276 RID: 8822
		private Transform m_Transform;
	}
}
