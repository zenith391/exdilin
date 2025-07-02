using System;
using UnityEngine;

[Serializable]
public class TileParameterSetting
{
	public string descriptorText = string.Empty;

	public bool activated = true;

	public bool autoOpenOnNewTiles;

	public int parameterIndex;

	public bool useRegExp;

	public string predicateName;

	public string predicateExpression;

	public string[] matchingPredicateNames;

	public TileParameterType type;

	public bool hideOnLeftSide;

	public bool setGafArgumentIfNotExists;

	public bool overwriteGafArgumentInBuildPanel;

	public int intDefaultValue;

	public float floatDefaultValue;

	public string prefixValueString = string.Empty;

	public string postfixValueString = string.Empty;

	public FloatTileParameterMode floatSliderMode;

	public FloatSliderRelativeBracket[] floatSliderRelativeBrackets = new FloatSliderRelativeBracket[1]
	{
		new FloatSliderRelativeBracket()
	};

	public float sliderSensitivity = 25f;

	public int intMinValue = -10;

	public int intMaxValue = 10;

	public int intStep = 1;

	public bool intOnlyShowPositive;

	public float floatMinValue = -10f;

	public float floatMaxValue = 10f;

	public float floatStep = 1f;

	public bool floatOnlyShowPositive;

	public float floatPresentMultiplier = 1f;

	public BidiIntFloatConverterType bidiIntFloatConverterType;

	public float affineConverterBias;

	public float affineConverterMultiplier = 1f;

	public float rangeConverterFrom;

	public float rangeConverterTo = 1f;

	public float[] tableConverterFloatValues = new float[0];

	public float[] piecewiseLinearConverterFloatValues = new float[2] { 0f, 1f };

	public int[] piecewiseLinearConverterIntValues = new int[2] { 0, 1 };

	public string[] tableConverterStringValues = new string[0];

	public TileParameterVisualizer visualizer;

	public SerializableVector3 verticalOffsetVisualizerDirection = new SerializableVector3(Vector3.up);

	public bool stringAcceptAny = true;

	public string stringAcceptAnyHint = "Enter some text!";

	public StringSliderColor[] stringSliderColor = new StringSliderColor[1]
	{
		new StringSliderColor()
	};
}
