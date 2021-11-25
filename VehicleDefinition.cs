using System;

// Token: 0x02000338 RID: 824
[Serializable]
public class VehicleDefinition
{
	// Token: 0x04001FCB RID: 8139
	public string name;

	// Token: 0x04001FCC RID: 8140
	public string wheelDefinitionName;

	// Token: 0x04001FCD RID: 8141
	public string frontWheelDefinitionName;

	// Token: 0x04001FCE RID: 8142
	public string backWheelDefinitionName;

	// Token: 0x04001FCF RID: 8143
	public VehicleDriveMode driveMode;

	// Token: 0x04001FD0 RID: 8144
	public VehicleTurnMode turnMode;

	// Token: 0x04001FD1 RID: 8145
	public float ballastFraction = 0.5f;
}
