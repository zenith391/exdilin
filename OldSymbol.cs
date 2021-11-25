using System;

// Token: 0x020002B4 RID: 692
public enum OldSymbol
{
	// Token: 0x04001A0F RID: 6671
	None,
	// Token: 0x04001A10 RID: 6672
	CreateCube1x1x1,
	// Token: 0x04001A11 RID: 6673
	CreateWedge1x1x1,
	// Token: 0x04001A12 RID: 6674
	Deprecated_CreateWheel1x2x2,
	// Token: 0x04001A13 RID: 6675
	CreateRocket1x1x1,
	// Token: 0x04001A14 RID: 6676
	CreateCube3x3x3,
	// Token: 0x04001A15 RID: 6677
	CreateWedge1x1x3,
	// Token: 0x04001A16 RID: 6678
	CreateWheel2x3x3,
	// Token: 0x04001A17 RID: 6679
	CreateMotor1x1x1,
	// Token: 0x04001A18 RID: 6680
	CreateBody1x1x1,
	// Token: 0x04001A19 RID: 6681
	PaintBrown,
	// Token: 0x04001A1A RID: 6682
	PaintOrange,
	// Token: 0x04001A1B RID: 6683
	PaintRed,
	// Token: 0x04001A1C RID: 6684
	PaintBlack,
	// Token: 0x04001A1D RID: 6685
	PaintBeige,
	// Token: 0x04001A1E RID: 6686
	PaintYellow,
	// Token: 0x04001A1F RID: 6687
	PaintWhite,
	// Token: 0x04001A20 RID: 6688
	PaintGrey,
	// Token: 0x04001A21 RID: 6689
	PaintGreen,
	// Token: 0x04001A22 RID: 6690
	PaintMagenta,
	// Token: 0x04001A23 RID: 6691
	PaintPurple,
	// Token: 0x04001A24 RID: 6692
	PaintBlue,
	// Token: 0x04001A25 RID: 6693
	PaintLime,
	// Token: 0x04001A26 RID: 6694
	PaintPink,
	// Token: 0x04001A27 RID: 6695
	PaintLavender,
	// Token: 0x04001A28 RID: 6696
	TextureGlass,
	// Token: 0x04001A29 RID: 6697
	TextureFaceHappy,
	// Token: 0x04001A2A RID: 6698
	TextureFaceSurprised,
	// Token: 0x04001A2B RID: 6699
	TextureCat,
	// Token: 0x04001A2C RID: 6700
	TextureEyes,
	// Token: 0x04001A2D RID: 6701
	TexturePant,
	// Token: 0x04001A2E RID: 6702
	TextureSpace,
	// Token: 0x04001A2F RID: 6703
	TextureJacket,
	// Token: 0x04001A30 RID: 6704
	TextureSuit,
	// Token: 0x04001A31 RID: 6705
	TextureCheckered,
	// Token: 0x04001A32 RID: 6706
	TextureBolts,
	// Token: 0x04001A33 RID: 6707
	TextureWood,
	// Token: 0x04001A34 RID: 6708
	TextureGrill,
	// Token: 0x04001A35 RID: 6709
	TexturePlain,
	// Token: 0x04001A36 RID: 6710
	PlayDone,
	// Token: 0x04001A37 RID: 6711
	PlayGreat,
	// Token: 0x04001A38 RID: 6712
	PlayBW,
	// Token: 0x04001A39 RID: 6713
	PlaySF,
	// Token: 0x04001A3A RID: 6714
	Stop,
	// Token: 0x04001A3B RID: 6715
	Play,
	// Token: 0x04001A3C RID: 6716
	ButtonMenu,
	// Token: 0x04001A3D RID: 6717
	ButtonPlay,
	// Token: 0x04001A3E RID: 6718
	ButtonStop,
	// Token: 0x04001A3F RID: 6719
	MoveXZ,
	// Token: 0x04001A40 RID: 6720
	MoveY,
	// Token: 0x04001A41 RID: 6721
	MoveTo,
	// Token: 0x04001A42 RID: 6722
	RotateTo,
	// Token: 0x04001A43 RID: 6723
	FireRocket,
	// Token: 0x04001A44 RID: 6724
	Drive,
	// Token: 0x04001A45 RID: 6725
	Break,
	// Token: 0x04001A46 RID: 6726
	SteerLeft,
	// Token: 0x04001A47 RID: 6727
	SteerRight,
	// Token: 0x04001A48 RID: 6728
	Walk,
	// Token: 0x04001A49 RID: 6729
	Back,
	// Token: 0x04001A4A RID: 6730
	TurnLeft,
	// Token: 0x04001A4B RID: 6731
	TurnRight,
	// Token: 0x04001A4C RID: 6732
	Jump,
	// Token: 0x04001A4D RID: 6733
	Turn,
	// Token: 0x04001A4E RID: 6734
	Reverse,
	// Token: 0x04001A4F RID: 6735
	Fixed,
	// Token: 0x04001A50 RID: 6736
	Explode,
	// Token: 0x04001A51 RID: 6737
	TimerStart,
	// Token: 0x04001A52 RID: 6738
	TimerStop,
	// Token: 0x04001A53 RID: 6739
	Deprecated_Tap,
	// Token: 0x04001A54 RID: 6740
	InputR1,
	// Token: 0x04001A55 RID: 6741
	InputL1,
	// Token: 0x04001A56 RID: 6742
	InputL2,
	// Token: 0x04001A57 RID: 6743
	InputR2,
	// Token: 0x04001A58 RID: 6744
	TiltLeft,
	// Token: 0x04001A59 RID: 6745
	TiltRight,
	// Token: 0x04001A5A RID: 6746
	GameStart,
	// Token: 0x04001A5B RID: 6747
	BumpObject,
	// Token: 0x04001A5C RID: 6748
	BumpGround,
	// Token: 0x04001A5D RID: 6749
	Wave,
	// Token: 0x04001A5E RID: 6750
	AngleLeft,
	// Token: 0x04001A5F RID: 6751
	AngleRight,
	// Token: 0x04001A60 RID: 6752
	Wait,
	// Token: 0x04001A61 RID: 6753
	Deprecated_Tutorial,
	// Token: 0x04001A62 RID: 6754
	Locked,
	// Token: 0x04001A63 RID: 6755
	Inventory,
	// Token: 0x04001A64 RID: 6756
	Win,
	// Token: 0x04001A65 RID: 6757
	Smoke,
	// Token: 0x04001A66 RID: 6758
	FreeSpin,
	// Token: 0x04001A67 RID: 6759
	Target,
	// Token: 0x04001A68 RID: 6760
	BumpTarget,
	// Token: 0x04001A69 RID: 6761
	ButtonCapture,
	// Token: 0x04001A6A RID: 6762
	TextureStripes,
	// Token: 0x04001A6B RID: 6763
	TextureArrow,
	// Token: 0x04001A6C RID: 6764
	TextureTeeth,
	// Token: 0x04001A6D RID: 6765
	TextureNose,
	// Token: 0x04001A6E RID: 6766
	CreateWheel1x1x1,
	// Token: 0x04001A6F RID: 6767
	ControlTiltLeft,
	// Token: 0x04001A70 RID: 6768
	ControlTiltRight,
	// Token: 0x04001A71 RID: 6769
	ControlJoystick1,
	// Token: 0x04001A72 RID: 6770
	ControlJoystickRange1,
	// Token: 0x04001A73 RID: 6771
	ControlJoystick2,
	// Token: 0x04001A74 RID: 6772
	ControlJoystickRange2,
	// Token: 0x04001A75 RID: 6773
	ControlL1,
	// Token: 0x04001A76 RID: 6774
	ControlR1,
	// Token: 0x04001A77 RID: 6775
	ControlL2,
	// Token: 0x04001A78 RID: 6776
	ControlR2,
	// Token: 0x04001A79 RID: 6777
	InputJoystick1Up,
	// Token: 0x04001A7A RID: 6778
	InputJoystick1Down,
	// Token: 0x04001A7B RID: 6779
	InputJoystick1Left,
	// Token: 0x04001A7C RID: 6780
	InputJoystick1Right,
	// Token: 0x04001A7D RID: 6781
	InputJoystick2Up,
	// Token: 0x04001A7E RID: 6782
	InputJoystick2Down,
	// Token: 0x04001A7F RID: 6783
	InputJoystick2Left,
	// Token: 0x04001A80 RID: 6784
	InputJoystick2Right,
	// Token: 0x04001A81 RID: 6785
	CameraFollow,
	// Token: 0x04001A82 RID: 6786
	KeyL1,
	// Token: 0x04001A83 RID: 6787
	KeyR1,
	// Token: 0x04001A84 RID: 6788
	KeyL2,
	// Token: 0x04001A85 RID: 6789
	KeyR2,
	// Token: 0x04001A86 RID: 6790
	KeyTiltLeft,
	// Token: 0x04001A87 RID: 6791
	KeyTiltRight,
	// Token: 0x04001A88 RID: 6792
	KeyJoystick1,
	// Token: 0x04001A89 RID: 6793
	KeyJoystick2,
	// Token: 0x04001A8A RID: 6794
	CreatePie1x1x1,
	// Token: 0x04001A8B RID: 6795
	CreateArc1x2x2,
	// Token: 0x04001A8C RID: 6796
	Speak,
	// Token: 0x04001A8D RID: 6797
	CreateCylinder1x1x1,
	// Token: 0x04001A8E RID: 6798
	CreateHead1x1x1,
	// Token: 0x04001A8F RID: 6799
	HandRight,
	// Token: 0x04001A90 RID: 6800
	HandRightTap,
	// Token: 0x04001A91 RID: 6801
	CreateRoadStraight11x1x11,
	// Token: 0x04001A92 RID: 6802
	CreateRoadCurve11x1x11,
	// Token: 0x04001A93 RID: 6803
	ButtonRestart,
	// Token: 0x04001A94 RID: 6804
	Deprecated_CreateSlicks1x1x1,
	// Token: 0x04001A95 RID: 6805
	CreateCoreCube1x1x1,
	// Token: 0x04001A96 RID: 6806
	CreateCrustCube1x1x1,
	// Token: 0x04001A97 RID: 6807
	CreateCoreWedge1x1x1,
	// Token: 0x04001A98 RID: 6808
	CreateCrustWedge1x1x1,
	// Token: 0x04001A99 RID: 6809
	ScaleTo,
	// Token: 0x04001A9A RID: 6810
	CreateSky,
	// Token: 0x04001A9B RID: 6811
	TextureClouds,
	// Token: 0x04001A9C RID: 6812
	PaintCeleste,
	// Token: 0x04001A9D RID: 6813
	ButtonMoveX,
	// Token: 0x04001A9E RID: 6814
	ButtonMoveY,
	// Token: 0x04001A9F RID: 6815
	ButtonMoveZ,
	// Token: 0x04001AA0 RID: 6816
	ButtonRotateX,
	// Token: 0x04001AA1 RID: 6817
	ButtonMenuSmall,
	// Token: 0x04001AA2 RID: 6818
	ButtonMenuLarge,
	// Token: 0x04001AA3 RID: 6819
	BreakOff,
	// Token: 0x04001AA4 RID: 6820
	HandLeft,
	// Token: 0x04001AA5 RID: 6821
	HandLeftTap,
	// Token: 0x04001AA6 RID: 6822
	CreateLegs1x1x1,
	// Token: 0x04001AA7 RID: 6823
	CreateWedge,
	// Token: 0x04001AA8 RID: 6824
	CreateDecal1x1x0,
	// Token: 0x04001AA9 RID: 6825
	ButtonMove,
	// Token: 0x04001AAA RID: 6826
	ButtonRotate,
	// Token: 0x04001AAB RID: 6827
	ButtonScale,
	// Token: 0x04001AAC RID: 6828
	ButtonCurrentMode,
	// Token: 0x04001AAD RID: 6829
	ButtonRed,
	// Token: 0x04001AAE RID: 6830
	ButtonGreen,
	// Token: 0x04001AAF RID: 6831
	ButtonBlue,
	// Token: 0x04001AB0 RID: 6832
	CreateButton1x1x0,
	// Token: 0x04001AB1 RID: 6833
	TapBlock,
	// Token: 0x04001AB2 RID: 6834
	ControlL3,
	// Token: 0x04001AB3 RID: 6835
	ControlR3,
	// Token: 0x04001AB4 RID: 6836
	ControlL4,
	// Token: 0x04001AB5 RID: 6837
	ControlR4,
	// Token: 0x04001AB6 RID: 6838
	InputL3,
	// Token: 0x04001AB7 RID: 6839
	InputR3,
	// Token: 0x04001AB8 RID: 6840
	InputL4,
	// Token: 0x04001AB9 RID: 6841
	InputR4,
	// Token: 0x04001ABA RID: 6842
	KeyL3,
	// Token: 0x04001ABB RID: 6843
	KeyR3,
	// Token: 0x04001ABC RID: 6844
	KeyL4,
	// Token: 0x04001ABD RID: 6845
	KeyR4,
	// Token: 0x04001ABE RID: 6846
	SendA,
	// Token: 0x04001ABF RID: 6847
	SendB,
	// Token: 0x04001AC0 RID: 6848
	SendC,
	// Token: 0x04001AC1 RID: 6849
	SendD,
	// Token: 0x04001AC2 RID: 6850
	SendE,
	// Token: 0x04001AC3 RID: 6851
	SendF,
	// Token: 0x04001AC4 RID: 6852
	SendG,
	// Token: 0x04001AC5 RID: 6853
	SendH,
	// Token: 0x04001AC6 RID: 6854
	SendI,
	// Token: 0x04001AC7 RID: 6855
	ButtonPause,
	// Token: 0x04001AC8 RID: 6856
	CreateLaser1x1x1,
	// Token: 0x04001AC9 RID: 6857
	LaserPulse,
	// Token: 0x04001ACA RID: 6858
	LaserBeam,
	// Token: 0x04001ACB RID: 6859
	HitByLaserPulse,
	// Token: 0x04001ACC RID: 6860
	HitByLaserBeam,
	// Token: 0x04001ACD RID: 6861
	CreateCat1x1x1,
	// Token: 0x04001ACE RID: 6862
	ExecuteAnimalBehavior,
	// Token: 0x04001ACF RID: 6863
	CreateCorner,
	// Token: 0x04001AD0 RID: 6864
	CreateRamp,
	// Token: 0x04001AD1 RID: 6865
	PaintGrass,
	// Token: 0x04001AD2 RID: 6866
	CreateWedgeInnerCorner,
	// Token: 0x04001AD3 RID: 6867
	CreateWedgeInnerEdge,
	// Token: 0x04001AD4 RID: 6868
	CreateEmitter,
	// Token: 0x04001AD5 RID: 6869
	EmitterEmit,
	// Token: 0x04001AD6 RID: 6870
	EmitterEmitSmoke,
	// Token: 0x04001AD7 RID: 6871
	EmitterEmitFast,
	// Token: 0x04001AD8 RID: 6872
	EmitterParticleHit,
	// Token: 0x04001AD9 RID: 6873
	CreateStabilizer,
	// Token: 0x04001ADA RID: 6874
	StabilizerStop,
	// Token: 0x04001ADB RID: 6875
	StabilizerStart,
	// Token: 0x04001ADC RID: 6876
	CreateWater,
	// Token: 0x04001ADD RID: 6877
	WaterWithin,
	// Token: 0x04001ADE RID: 6878
	WaterStreamNorth,
	// Token: 0x04001ADF RID: 6879
	WaterStreamSouth,
	// Token: 0x04001AE0 RID: 6880
	WaterStreamWest,
	// Token: 0x04001AE1 RID: 6881
	WaterStreamEast,
	// Token: 0x04001AE2 RID: 6882
	SymbolCount
}
