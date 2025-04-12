using System;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x0200037F RID: 895
	public class TMP_ExampleScript_01 : MonoBehaviour
	{
		// Token: 0x060027BA RID: 10170 RVA: 0x00123BA8 File Offset: 0x00121FA8
		private void Awake()
		{
			if (this.ObjectType == TMP_ExampleScript_01.objectType.TextMeshPro)
			{
				this.m_text = (base.GetComponent<TextMeshPro>() ?? base.gameObject.AddComponent<TextMeshPro>());
			}
			else
			{
				this.m_text = (base.GetComponent<TextMeshProUGUI>() ?? base.gameObject.AddComponent<TextMeshProUGUI>());
			}
			this.m_text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
			this.m_text.fontSharedMaterial = Resources.Load<Material>("Fonts & Materials/Anton SDF - Drop Shadow");
			this.m_text.fontSize = 120f;
			this.m_text.text = "A <#0080ff>simple</color> line of text.";
			Vector2 preferredValues = this.m_text.GetPreferredValues(float.PositiveInfinity, float.PositiveInfinity);
			this.m_text.rectTransform.sizeDelta = new Vector2(preferredValues.x, preferredValues.y);
		}

		// Token: 0x060027BB RID: 10171 RVA: 0x00123C84 File Offset: 0x00122084
		private void Update()
		{
			if (!this.isStatic)
			{
				this.m_text.SetText("The count is <#0080ff>{0}</color>", (float)(this.count % 1000));
				this.count++;
			}
		}

		// Token: 0x040022B3 RID: 8883
		public TMP_ExampleScript_01.objectType ObjectType;

		// Token: 0x040022B4 RID: 8884
		public bool isStatic;

		// Token: 0x040022B5 RID: 8885
		private TMP_Text m_text;

		// Token: 0x040022B6 RID: 8886
		private const string k_label = "The count is <#0080ff>{0}</color>";

		// Token: 0x040022B7 RID: 8887
		private int count;

		// Token: 0x02000380 RID: 896
		public enum objectType
		{
			// Token: 0x040022B9 RID: 8889
			TextMeshPro,
			// Token: 0x040022BA RID: 8890
			TextMeshProUGUI
		}
	}
}
