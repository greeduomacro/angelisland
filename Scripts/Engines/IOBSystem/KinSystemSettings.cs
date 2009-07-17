/***************************************************************************
 *                               CREDITS
 *                         -------------------
 *                         : (C) 2004-2009 Luke Tomasello (AKA Adam Ant)
 *                         :   and the Angel Island Software Team
 *                         :   luke@tomasello.com
 *                         :   Official Documentation:
 *                         :   www.game-master.net, wiki.game-master.net
 *                         :   Official Source Code (SVN Repository):
 *                         :   http://game-master.net:8050/svn/angelisland
 *                         : 
 *                         : (C) May 1, 2002 The RunUO Software Team
 *                         :   info@runuo.com
 *
 *   Give credit where credit is due!
 *   Even though this is 'free software', you are encouraged to give
 *    credit to the individuals that spent many hundreds of hours
 *    developing this software.
 *   Many of the ideas you will find in this Angel Island version of 
 *   Ultima Online are unique and one-of-a-kind in the gaming industry! 
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/

/* Scripts/Engines/IOBSystem/KinSystemSettings.cs
 * CHANGELOG:
 *  5/14/09, plasma
 *		Added v11, activity stuff
 *  04/09/09, plasma
 *		Added v10, activity stuff en masse
 *	01/14/09, plasma
 *		v8 & 9, lots of guard settings
 *	12/5/08, Adam
 *		Added KinAwards (version 7)
 *	09/29/08, plasma
 *			Added OutputCaptureData
 *	02/13/08, plasma
 *			Added city capture stuff
 *  12/3/07, Pix
 *      Added IOBAlignement.Healer for kin-healers instead of overloading Outcast
 *  9/12/07 - Pix
 *      Initial Version
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Server;
using Server.Items;

namespace Server.Engines.IOBSystem
{
	class KinSystemSettings
	{
		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler(OnLoad);
			EventSink.WorldSave += new WorldSaveEventHandler(OnSave);
		}

		#region Save/Load

		public static void OnSave(WorldSaveEventArgs e)
		{
			try
			{
				Console.WriteLine("KinSystemSettings Saving...");
				if (!Directory.Exists("Saves/AngelIsland"))
					Directory.CreateDirectory("Saves/AngelIsland");

				string filePath = Path.Combine("Saves/AngelIsland", "KinSystemSettings.bin");

				GenericWriter bin;
				bin = new BinaryFileWriter(filePath, true);

				bin.Write(11); //version
				//v11
				bin.Write(A_MaxShoppers);
				//v10 below
				bin.Write(A_F_Visitor);
				bin.Write(A_Visitor);
				bin.Write(A_F_Sale);
				bin.Write(A_Sale);
				bin.Write(A_GPMaint);
				bin.Write(A_GPHire);
				bin.Write(A_GDeath);
				bin.Write(A_F_Death);
				bin.Write(A_Death);
				bin.Write(A_GCChampLevel);
				bin.Write(A_GCDeath);
				bin.Write(A_MaxVisitors);
				//v9 below
				bin.Write(GuardChangeTimeHours);
				//v8 below
				bin.Write(CityGuardSlots);
				bin.Write(GuardMaintMinutes);
				bin.Write(GuardTypeLowSilverCost);
				bin.Write(GuardTypeMediumSilverCost);
				bin.Write(GuardTypeHighSilverCost);
				bin.Write(GuardTypeLowSilverMaintCost);
				bin.Write(GuardTypeMediumSilverMaintCost);
				bin.Write(GuardTypeHighSilverMaintCost);
				//v7 below
				bin.Write(KinAwards);
				//v6 below
				bin.Write(OutputCaptureData);
				//v5 below
				bin.Write(CityCaptureEnabled);
				bin.Write(VortexCaptureProportion);
				bin.Write(VortexMinDamagePercentage);
				bin.Write(BeneficiaryQualifyPercentage);
				bin.Write(BeneficiaryCap);
				bin.Write(CaptureDefenseRange);
				bin.Write(VortexExpireMinutes);
				bin.Write(BaseCaptureMinutes);
				//v4 below
				bin.Write(KinNameHueEnabled);
				//v3 below
				bin.Write(ShowStatloss);
				bin.Write(ShowKinSingleClick);
				//v2 below:
				bin.Write(KinAggressionMinutes);
				bin.Write(KinBeneficialMinutes);
				bin.Write(KinHealerModifier);
				//v1 below
				bin.Write(PointsEnabled);
				bin.Write(StatLossEnabled);
				bin.Write(StatLossPercentageSkills);
				bin.Write(StatLossPercentageStats);
				bin.Write(StatLossDurationMinutes);


				bin.Close();
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}

		public static void OnLoad()
		{
			try
			{
				Console.WriteLine("KinSystemSettings Loading...");
				string filePath = Path.Combine("Saves/AngelIsland", "KinSystemSettings.bin");

				if (!File.Exists(filePath))
					return;

				BinaryFileReader datreader = new BinaryFileReader(new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)));
				int version = datreader.ReadInt();

				switch (version)
				{
					case 11:
						A_MaxShoppers = datreader.ReadInt();
						goto case 10;
					case 10:
						A_F_Visitor = datreader.ReadInt();
						A_Visitor = datreader.ReadInt();
						A_F_Sale = datreader.ReadInt();
						A_Sale = datreader.ReadInt();
						A_GPMaint = datreader.ReadInt();
						A_GPHire = datreader.ReadInt();
						A_GDeath = datreader.ReadInt();
						A_F_Death = datreader.ReadInt();
						A_Death = datreader.ReadInt();
						A_GCChampLevel = datreader.ReadInt();
						A_GCDeath = datreader.ReadInt();
						A_MaxVisitors = datreader.ReadInt();
						goto case 9;
					case 9:
						GuardChangeTimeHours = datreader.ReadInt();
						goto case 8;
					case 8:
						CityGuardSlots = datreader.ReadInt();
						GuardMaintMinutes = datreader.ReadInt();
						GuardTypeLowSilverCost = datreader.ReadInt();
						GuardTypeMediumSilverCost = datreader.ReadInt();
						GuardTypeHighSilverCost = datreader.ReadInt();
						GuardTypeLowSilverMaintCost = datreader.ReadInt();
						GuardTypeMediumSilverMaintCost = datreader.ReadInt();
						GuardTypeHighSilverMaintCost = datreader.ReadInt();
						goto case 7;
					case 7:
						KinAwards = datreader.ReadBool();
						goto case 6;
					case 6:
						OutputCaptureData = datreader.ReadBool();
						goto case 5;
					case 5:
						CityCaptureEnabled = datreader.ReadBool();
						VortexCaptureProportion = datreader.ReadDouble();
						VortexMinDamagePercentage = datreader.ReadDouble();
						BeneficiaryQualifyPercentage = datreader.ReadDouble();
						BeneficiaryCap = datreader.ReadInt();
						CaptureDefenseRange = datreader.ReadInt();
						VortexExpireMinutes = datreader.ReadInt();
						BaseCaptureMinutes = datreader.ReadInt();
						goto case 4;
					case 4:
						KinNameHueEnabled = datreader.ReadBool();
						goto case 3;
					case 3:
						ShowStatloss = datreader.ReadBool();
						ShowKinSingleClick = datreader.ReadBool();
						goto case 2;
					case 2:
						KinAggressionMinutes = datreader.ReadDouble();
						KinBeneficialMinutes = datreader.ReadDouble();
						KinHealerModifier = datreader.ReadDouble();
						goto case 1;
					case 1:
						PointsEnabled = datreader.ReadBool();
						StatLossEnabled = datreader.ReadBool();
						StatLossPercentageSkills = datreader.ReadDouble();
						StatLossPercentageStats = datreader.ReadDouble();
						StatLossDurationMinutes = datreader.ReadDouble();
						break;
				}

				datreader.Close();
			}
			catch (Exception re)
			{
				System.Console.WriteLine("ERROR LOADING KinSystemSettings!");
				Scripts.Commands.LogHelper.LogException(re);
			}
		}
		#endregion

		//The guts... with initial values before load
		public static bool PointsEnabled = false;
		public static bool StatLossEnabled = false;
		public static double StatLossPercentageSkills = 0.33;
		public static double StatLossPercentageStats = 0.33;
		public static double StatLossDurationMinutes = 5.0;
		//v2 additions
		public static double KinAggressionMinutes = 5.0;
		public static double KinBeneficialMinutes = 1440.0; //default: one day (24 * 60)
		public static double KinHealerModifier = 3.0;
		//v3 additions
		public static bool ShowStatloss = false;
		public static bool ShowKinSingleClick = true;
		//v4 additions																		yeah
		public static bool KinNameHueEnabled = false;
		//v5 additions		// City Capture stuff
		public static bool CityCaptureEnabled = false;
		public static double VortexCaptureProportion = 0.75;			//killing vortex is worth 75% of the total capture points
		public static double VortexMinDamagePercentage = 0.25;		//kin must do at least 25% of the vortex damage to even be considered
		public static double BeneficiaryQualifyPercentage = 0.25;	//eligible beneficiaries are the top 25% of the winners
		public static int BeneficiaryCap = 20;										//beneficiary cap
		public static int CaptureDefenseRange = 20;								//max range from vortex where defense points are awarded for pvp kills
		public static int VortexExpireMinutes = 60;								//
		public static int BaseCaptureMinutes = 10080;							//base time before next capture (a week?)
		//v6 additions
		public static bool OutputCaptureData = false;							//Outputs verbose capture calculation data to GMs near the relevant sigil
		//v7 additions
		public static bool KinAwards = false;											//Enable Silver drops (and gold reductions)
		//v8 additions
		public static int CityGuardSlots = 10;
		public static int GuardMaintMinutes = 60;
		public static int GuardTypeLowSilverCost = 100;
		public static int GuardTypeMediumSilverCost = 200;
		public static int GuardTypeHighSilverCost = 300;
		public static int GuardTypeLowSilverMaintCost = 10;
		public static int GuardTypeMediumSilverMaintCost = 20;
		public static int GuardTypeHighSilverMaintCost = 30;
		//v9 additions
		public static int GuardChangeTimeHours = 24;
		//v10 additions
		public static int A_F_Visitor = 0;
		public static int A_Visitor = 0;
		public static int A_F_Sale = 0;
		public static int A_Sale = 0;
		public static int A_GPMaint = 0;
		public static int A_GPHire = 0;
		public static int A_GDeath = 0;
		public static int A_F_Death = 0;
		public static int A_Death = 0;
		public static int A_GCChampLevel = 0;
		public static int A_GCDeath = 0;
		public static int A_MaxVisitors = 10;
		//v11
		public static int A_MaxShoppers= 10;

	}



	[FlipableAttribute(0x1f14, 0x1f15, 0x1f16, 0x1f17)]
	public class KinSystemConsole : Item
	{
		[Constructable]
		public KinSystemConsole()
			: base(0x1F14)
		{
			Weight = 1.0;
			Name = "KinSystem Settings Console";
			Hue = 170;
		}

		public KinSystemConsole(Serial serial)
			: base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.AccessLevel > AccessLevel.Player)
			{
				from.SendGump(new Server.Gumps.PropertiesGump(from, this));
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool KinNameHueEnabled
		{
			get
			{
				return KinSystemSettings.KinNameHueEnabled;
			}
			set
			{
				KinSystemSettings.KinNameHueEnabled = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool ShowKinSingleClick
		{
			get
			{
				return KinSystemSettings.ShowKinSingleClick;
			}
			set
			{
				KinSystemSettings.ShowKinSingleClick = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool ShowStatloss
		{
			get
			{
				return KinSystemSettings.ShowStatloss;
			}
			set
			{
				KinSystemSettings.ShowStatloss = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double KinAggressionMinutes
		{
			get
			{
				return KinSystemSettings.KinAggressionMinutes;
			}
			set
			{
				KinSystemSettings.KinAggressionMinutes = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double KinBeneficialMinutes
		{
			get
			{
				return KinSystemSettings.KinBeneficialMinutes;
			}
			set
			{
				KinSystemSettings.KinBeneficialMinutes = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double KinHealerModifier
		{
			get
			{
				return KinSystemSettings.KinHealerModifier;
			}
			set
			{
				KinSystemSettings.KinHealerModifier = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool PointsEnabled
		{
			get
			{
				return KinSystemSettings.PointsEnabled;
			}
			set
			{
				KinSystemSettings.PointsEnabled = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool StatLossEnabled
		{
			get
			{
				return KinSystemSettings.StatLossEnabled;
			}
			set
			{
				KinSystemSettings.StatLossEnabled = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double SkillLossPercentage
		{
			get
			{
				return KinSystemSettings.StatLossPercentageSkills;
			}
			set
			{
				KinSystemSettings.StatLossPercentageSkills = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double StatLossPercentage
		{
			get
			{
				return KinSystemSettings.StatLossPercentageStats;
			}
			set
			{
				KinSystemSettings.StatLossPercentageStats = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double StatLossDuration
		{
			get
			{
				return KinSystemSettings.StatLossDurationMinutes;
			}
			set
			{
				KinSystemSettings.StatLossDurationMinutes = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool CityCaptureEnabled
		{
			get
			{
				return KinSystemSettings.CityCaptureEnabled;
			}
			set
			{
				KinSystemSettings.CityCaptureEnabled = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int CaptureDefenseRange
		{
			get
			{
				return KinSystemSettings.CaptureDefenseRange;
			}
			set
			{
				KinSystemSettings.CaptureDefenseRange = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double VortexCaptureProportion
		{
			get
			{
				return KinSystemSettings.VortexCaptureProportion;
			}
			set
			{
				KinSystemSettings.VortexCaptureProportion = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int BeneficiaryCap
		{
			get
			{
				return KinSystemSettings.BeneficiaryCap;
			}
			set
			{
				KinSystemSettings.BeneficiaryCap = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double BeneficiaryQualifyPercentage
		{
			get
			{
				return KinSystemSettings.BeneficiaryQualifyPercentage;
			}
			set
			{
				KinSystemSettings.BeneficiaryQualifyPercentage = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public double VortexMinDamagePercentage
		{
			get
			{
				return KinSystemSettings.VortexMinDamagePercentage;
			}
			set
			{
				KinSystemSettings.VortexMinDamagePercentage = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int VortexExpireMinutes
		{
			get
			{
				return KinSystemSettings.VortexExpireMinutes;
			}
			set
			{
				KinSystemSettings.VortexExpireMinutes = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int BaseCaptureMinutes
		{
			get
			{
				return KinSystemSettings.BaseCaptureMinutes;
			}
			set
			{
				KinSystemSettings.BaseCaptureMinutes = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool OutputCaptureData
		{
			get
			{
				return KinSystemSettings.OutputCaptureData;
			}
			set
			{
				KinSystemSettings.OutputCaptureData = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int CityGuardSlots
		{
			get
			{
				return KinSystemSettings.CityGuardSlots;
			}
			set
			{
				KinSystemSettings.CityGuardSlots = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int GuardTypeASilverCost
		{
			get
			{
				return KinSystemSettings.GuardTypeLowSilverCost;
			}
			set
			{
				KinSystemSettings.GuardTypeLowSilverCost = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int GuardTypeBSilverCost
		{
			get
			{
				return KinSystemSettings.GuardTypeMediumSilverCost;
			}
			set
			{
				KinSystemSettings.GuardTypeMediumSilverCost = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int GuardTypeCSilverCost
		{
			get
			{
				return KinSystemSettings.GuardTypeHighSilverCost;
			}
			set
			{
				KinSystemSettings.GuardTypeHighSilverCost = value;
			}
		}
		//


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int GuardTypeASilverMaintCost
		{
			get
			{
				return KinSystemSettings.GuardTypeLowSilverMaintCost;
			}
			set
			{
				KinSystemSettings.GuardTypeLowSilverMaintCost = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int GuardTypeBSilverMaintCost
		{
			get
			{
				return KinSystemSettings.GuardTypeMediumSilverMaintCost;
			}
			set
			{
				KinSystemSettings.GuardTypeMediumSilverMaintCost = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int GuardTypeCSilverMaintCost
		{
			get
			{
				return KinSystemSettings.GuardTypeHighSilverMaintCost;
			}
			set
			{
				KinSystemSettings.GuardTypeHighSilverMaintCost = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int GuardChangeTimeHours
		{
			get
			{
				return KinSystemSettings.GuardChangeTimeHours;
			}
			set
			{
				KinSystemSettings.GuardChangeTimeHours = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool KinAwards
		{
			get
			{
				return KinSystemSettings.KinAwards;
			}
			set
			{
				KinSystemSettings.KinAwards = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_F_Visitor
		{
			get
			{
				return KinSystemSettings.A_F_Visitor;
			}
			set
			{
				KinSystemSettings.A_F_Visitor = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_Visitor
		{
			get
			{
				return KinSystemSettings.A_Visitor;
			}
			set
			{
				KinSystemSettings.A_Visitor = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_F_Sale
		{
			get
			{
				return KinSystemSettings.A_F_Sale;
			}
			set
			{
				KinSystemSettings.A_F_Sale = value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_Sale
		{
			get
			{
				return KinSystemSettings.A_Sale;
			}
			set
			{
				KinSystemSettings.A_Sale= value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_GPMaint
		{
			get
			{
				return KinSystemSettings.A_GPMaint;
			}
			set
			{
				KinSystemSettings.A_GPMaint= value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_GPHire
		{
			get
			{
				return KinSystemSettings.A_GPHire;
			}
			set
			{
				KinSystemSettings.A_GPHire= value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_GDeath
		{
			get
			{
				return KinSystemSettings.A_GDeath;
			}
			set
			{
				KinSystemSettings.A_GDeath = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_F_Death
		{
			get
			{
				return KinSystemSettings.A_F_Death;
			}
			set
			{
				KinSystemSettings.A_F_Death= value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_Death
		{
			get
			{
				return KinSystemSettings.A_Death;
			}
			set
			{
				KinSystemSettings.A_Death= value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_GCChamplevel
		{
			get
			{
				return KinSystemSettings.A_GCChampLevel;
			}
			set
			{
				KinSystemSettings.A_GCChampLevel= value;
			}
		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_GCDeath
		{
			get
			{
				return KinSystemSettings.A_GCDeath;
			}
			set
			{
				KinSystemSettings.A_GCDeath= value;
			}


		}


		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_MaxVisitors
		{
			get
			{
				return KinSystemSettings.A_MaxVisitors;
			}
			set
			{
				KinSystemSettings.A_MaxVisitors= value;
			}


		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public int A_MaxShoppers
		{
			get
			{
				return KinSystemSettings.A_MaxShoppers;
			}
			set
			{
				KinSystemSettings.A_MaxShoppers= value;
			}


		}



		#region Serialize/Deserialize - nothing to do
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
		#endregion
	}


}
