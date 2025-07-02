using UnityEngine;

public class LetterboxVisualEffect : VisualEffect
{
	private bool moveIn = true;

	private LetterboxingEffect theEffect;

	public LetterboxVisualEffect(string name, bool moveIn)
		: base(name)
	{
		this.moveIn = moveIn;
	}

	public override void Begin()
	{
		base.Begin();
		GameObject gameObject = Blocksworld.mainCamera.gameObject;
		LetterboxingEffect letterboxingEffect = gameObject.GetComponent<LetterboxingEffect>();
		if (letterboxingEffect == null)
		{
			letterboxingEffect = gameObject.AddComponent<LetterboxingEffect>();
		}
		theEffect = letterboxingEffect;
	}

	public override void End()
	{
		base.End();
		if (!moveIn)
		{
			Destroy();
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		Object.Destroy(theEffect);
		theEffect = null;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (theEffect != null && !HasEnded())
		{
			float num = currentTime / timeLength;
			if (!moveIn)
			{
				num = 1f - num;
			}
			float fraction = num * 0.05f;
			theEffect.SetFraction(fraction);
		}
	}
}
