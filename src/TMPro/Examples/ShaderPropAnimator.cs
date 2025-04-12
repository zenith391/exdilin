using System;
using System.Collections;
using UnityEngine;

namespace TMPro.Examples
{
	// Token: 0x0200037B RID: 891
	public class ShaderPropAnimator : MonoBehaviour
	{
		// Token: 0x060027AC RID: 10156 RVA: 0x001230D3 File Offset: 0x001214D3
		private void Awake()
		{
			this.m_Renderer = base.GetComponent<Renderer>();
			this.m_Material = this.m_Renderer.material;
		}

		// Token: 0x060027AD RID: 10157 RVA: 0x001230F2 File Offset: 0x001214F2
		private void Start()
		{
			base.StartCoroutine(this.AnimateProperties());
		}

		// Token: 0x060027AE RID: 10158 RVA: 0x00123104 File Offset: 0x00121504
		private IEnumerator AnimateProperties()
		{
			this.m_frame = UnityEngine.Random.Range(0f, 1f);
			for (;;)
			{
				float glowPower = this.GlowCurve.Evaluate(this.m_frame);
				this.m_Material.SetFloat(ShaderUtilities.ID_GlowPower, glowPower);
				this.m_frame += Time.deltaTime * UnityEngine.Random.Range(0.2f, 0.3f);
				yield return new WaitForEndOfFrame();
			}
			yield break;
		}

		// Token: 0x040022A8 RID: 8872
		private Renderer m_Renderer;

		// Token: 0x040022A9 RID: 8873
		private Material m_Material;

		// Token: 0x040022AA RID: 8874
		public AnimationCurve GlowCurve;

		// Token: 0x040022AB RID: 8875
		public float m_frame;
	}
}
