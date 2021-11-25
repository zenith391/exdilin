using System;
using System.Collections;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x02000370 RID: 880
	public class Benchmark01 : MonoBehaviour
	{
		// Token: 0x06002792 RID: 10130 RVA: 0x00121770 File Offset: 0x0011FB70
		private IEnumerator Start()
		{
			if (this.BenchmarkType == 0)
			{
				this.m_textMeshPro = base.gameObject.AddComponent<TextMeshPro>();
				this.m_textMeshPro.autoSizeTextContainer = true;
				if (this.TMProFont != null)
				{
					this.m_textMeshPro.font = this.TMProFont;
				}
				this.m_textMeshPro.fontSize = 48f;
				this.m_textMeshPro.alignment = TextAlignmentOptions.Center;
				this.m_textMeshPro.extraPadding = true;
				this.m_textMeshPro.enableWordWrapping = false;
				this.m_material01 = this.m_textMeshPro.font.material;
				this.m_material02 = (Resources.Load("Fonts & Materials/LiberationSans SDF - Drop Shadow", typeof(Material)) as Material);
			}
			else if (this.BenchmarkType == 1)
			{
				this.m_textMesh = base.gameObject.AddComponent<TextMesh>();
				if (this.TextMeshFont != null)
				{
					this.m_textMesh.font = this.TextMeshFont;
					this.m_textMesh.GetComponent<Renderer>().sharedMaterial = this.m_textMesh.font.material;
				}
				else
				{
					this.m_textMesh.font = (Resources.Load("Fonts/ARIAL", typeof(Font)) as Font);
					this.m_textMesh.GetComponent<Renderer>().sharedMaterial = this.m_textMesh.font.material;
				}
				this.m_textMesh.fontSize = 48;
				this.m_textMesh.anchor = TextAnchor.MiddleCenter;
			}
			for (int i = 0; i <= 1000000; i++)
			{
				if (this.BenchmarkType == 0)
				{
					this.m_textMeshPro.SetText("The <#0050FF>count is: </color>{0}", (float)(i % 1000));
					if (i % 1000 == 999)
					{
						TMP_Text textMeshPro = this.m_textMeshPro;
						Material fontSharedMaterial;
						if (this.m_textMeshPro.fontSharedMaterial == this.m_material01)
						{
							Material material = this.m_material02;
							this.m_textMeshPro.fontSharedMaterial = material;
							fontSharedMaterial = material;
						}
						else
						{
							Material material = this.m_material01;
							this.m_textMeshPro.fontSharedMaterial = material;
							fontSharedMaterial = material;
						}
						textMeshPro.fontSharedMaterial = fontSharedMaterial;
					}
				}
				else if (this.BenchmarkType == 1)
				{
					this.m_textMesh.text = "The <color=#0050FF>count is: </color>" + (i % 1000).ToString();
				}
				yield return null;
			}
			yield return null;
			yield break;
		}

		// Token: 0x04002258 RID: 8792
		public int BenchmarkType;

		// Token: 0x04002259 RID: 8793
		public TMP_FontAsset TMProFont;

		// Token: 0x0400225A RID: 8794
		public Font TextMeshFont;

		// Token: 0x0400225B RID: 8795
		private TextMeshPro m_textMeshPro;

		// Token: 0x0400225C RID: 8796
		private TextContainer m_textContainer;

		// Token: 0x0400225D RID: 8797
		private TextMesh m_textMesh;

		// Token: 0x0400225E RID: 8798
		private const string label01 = "The <#0050FF>count is: </color>{0}";

		// Token: 0x0400225F RID: 8799
		private const string label02 = "The <color=#0050FF>count is: </color>";

		// Token: 0x04002260 RID: 8800
		private Material m_material01;

		// Token: 0x04002261 RID: 8801
		private Material m_material02;
	}
}
