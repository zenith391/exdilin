using UnityEngine;
using UnityEngine.UI;

public class HighlightableText : Text
{
	public Color highlightColor;

	private Color _defaultColor;

	private new void Start()
	{
		_defaultColor = base.color;
	}

	public void Highlight(bool highlight)
	{
		base.color = ((!highlight) ? _defaultColor : highlightColor);
	}
}
