using System;

[Serializable]
public class VehicleDefinition
{
	public string name;

	public string wheelDefinitionName;

	public string frontWheelDefinitionName;

	public string backWheelDefinitionName;

	public VehicleDriveMode driveMode;

	public VehicleTurnMode turnMode;

	public float ballastFraction = 0.5f;
}
