using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Token: 0x02000378 RID: 888
public class EnvMapAnimator : MonoBehaviour
{
	// Token: 0x060027A6 RID: 10150 RVA: 0x00122DCB File Offset: 0x001211CB
	private void Awake()
	{
		this.m_textMeshPro = base.GetComponent<TMP_Text>();
		this.m_material = this.m_textMeshPro.fontSharedMaterial;
	}

	// Token: 0x060027A7 RID: 10151 RVA: 0x00122DEC File Offset: 0x001211EC
	private IEnumerator Start()
	{
		Matrix4x4 matrix = default(Matrix4x4);
		for (;;)
		{
			matrix.SetTRS(Vector3.zero, Quaternion.Euler(Time.time * this.RotationSpeeds.x, Time.time * this.RotationSpeeds.y, Time.time * this.RotationSpeeds.z), Vector3.one);
			this.m_material.SetMatrix("_EnvMatrix", matrix);
			yield return null;
		}
		yield break;
	}

	// Token: 0x04002297 RID: 8855
	public Vector3 RotationSpeeds;

	// Token: 0x04002298 RID: 8856
	private TMP_Text m_textMeshPro;

	// Token: 0x04002299 RID: 8857
	private Material m_material;
}
