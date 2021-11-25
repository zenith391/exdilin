using System;
using UnityEngine;

// Token: 0x02000248 RID: 584
[Serializable]
public class TileParameterSetting
{
	// Token: 0x040016D9 RID: 5849
	public string descriptorText = string.Empty;

	// Token: 0x040016DA RID: 5850
	public bool activated = true;

	// Token: 0x040016DB RID: 5851
	public bool autoOpenOnNewTiles;

	// Token: 0x040016DC RID: 5852
	public int parameterIndex;

	// Token: 0x040016DD RID: 5853
	public bool useRegExp;

	// Token: 0x040016DE RID: 5854
	public string predicateName;

	// Token: 0x040016DF RID: 5855
	public string predicateExpression;

	// Token: 0x040016E0 RID: 5856
	public string[] matchingPredicateNames;

	// Token: 0x040016E1 RID: 5857
	public TileParameterType type;

	// Token: 0x040016E2 RID: 5858
	public bool hideOnLeftSide;

	// Token: 0x040016E3 RID: 5859
	public bool setGafArgumentIfNotExists;

	// Token: 0x040016E4 RID: 5860
	public bool overwriteGafArgumentInBuildPanel;

	// Token: 0x040016E5 RID: 5861
	public int intDefaultValue;

	// Token: 0x040016E6 RID: 5862
	public float floatDefaultValue;

	// Token: 0x040016E7 RID: 5863
	public string prefixValueString = string.Empty;

	// Token: 0x040016E8 RID: 5864
	public string postfixValueString = string.Empty;

	// Token: 0x040016E9 RID: 5865
	public FloatTileParameterMode floatSliderMode;

	// Token: 0x040016EA RID: 5866
	public FloatSliderRelativeBracket[] floatSliderRelativeBrackets = new FloatSliderRelativeBracket[]
	{
		new FloatSliderRelativeBracket()
	};

	// Token: 0x040016EB RID: 5867
	public float sliderSensitivity = 25f;

	// Token: 0x040016EC RID: 5868
	public int intMinValue = -10;

	// Token: 0x040016ED RID: 5869
	public int intMaxValue = 10;

	// Token: 0x040016EE RID: 5870
	public int intStep = 1;

	// Token: 0x040016EF RID: 5871
	public bool intOnlyShowPositive;

	// Token: 0x040016F0 RID: 5872
	public float floatMinValue = -10f;

	// Token: 0x040016F1 RID: 5873
	public float floatMaxValue = 10f;

	// Token: 0x040016F2 RID: 5874
	public float floatStep = 1f;

	// Token: 0x040016F3 RID: 5875
	public bool floatOnlyShowPositive;

	// Token: 0x040016F4 RID: 5876
	public float floatPresentMultiplier = 1f;

	// Token: 0x040016F5 RID: 5877
	public BidiIntFloatConverterType bidiIntFloatConverterType;

	// Token: 0x040016F6 RID: 5878
	public float affineConverterBias;

	// Token: 0x040016F7 RID: 5879
	public float affineConverterMultiplier = 1f;

	// Token: 0x040016F8 RID: 5880
	public float rangeConverterFrom;

	// Token: 0x040016F9 RID: 5881
	public float rangeConverterTo = 1f;

	// Token: 0x040016FA RID: 5882
	public float[] tableConverterFloatValues = new float[0];

	// Token: 0x040016FB RID: 5883
	public float[] piecewiseLinearConverterFloatValues = new float[]
	{
		0f,
		1f
	};

	// Token: 0x040016FC RID: 5884
	public int[] piecewiseLinearConverterIntValues = new int[]
	{
		0,
		1
	};

	// Token: 0x040016FD RID: 5885
	public string[] tableConverterStringValues = new string[0];

	// Token: 0x040016FE RID: 5886
	public TileParameterVisualizer visualizer;

	// Token: 0x040016FF RID: 5887
	public SerializableVector3 verticalOffsetVisualizerDirection = new SerializableVector3(Vector3.up);

	// Token: 0x04001700 RID: 5888
	public bool stringAcceptAny = true;

	// Token: 0x04001701 RID: 5889
	public string stringAcceptAnyHint = "Enter some text!";

	// Token: 0x04001702 RID: 5890
	public StringSliderColor[] stringSliderColor = new StringSliderColor[]
	{
		new StringSliderColor()
	};
}
