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

/* Engines/Township/TSNPC.cs
 * CHANGELOG:
 * 10/10/08, Pix
 *		Added #regions so I can keep my sanity.
 *		Added Wall tools to TSProvisioner.
 *	5/11/08, Adam
 *		Performance Conversion: Regions now use HashTables instead of ArrayLists
 *	12/11/07 Pix
 *		Now lookouts report to allies of the township.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Server;
using Server.Items;
using Server.Multis;
using Server.Regions;
using Server.Township;

namespace Server.Mobiles
{
	#region TownshipHelper class

	public class TownshipHelper
	{
		public static bool IsInSameHouse(Mobile a, Mobile b)
		{
			bool bReturn = false;

			if (a != null && b != null)
			{
				BaseHouse ha = BaseHouse.FindHouseAt(a);
				BaseHouse hb = BaseHouse.FindHouseAt(b);
				if (ha != null && hb != null)
				{
					if ( ha == hb )
					{
						bReturn = true;
					}
				}
			}

			return bReturn;
		}

		public static bool IsTownshipNPCOwner(Mobile a, Mobile vendor)
		{
			if (a != null)
			{
				BaseHouse house = BaseHouse.FindHouseAt(vendor);
				if( house != null )
				{
					TownshipRegion tr = TownshipRegion.GetTownshipAt(vendor);
					if (tr != null && tr.TStone != null)
					{
						if (house.IsOwner(a)
							&& a.Guild != null
							&& tr.TStone.Guild != null
							&& a.Guild == tr.TStone.Guild)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static bool IsEnemyOfTownship(Mobile vendor, Mobile b)
		{
			if( b == null || vendor == null || !(b is PlayerMobile) )
			{
				return true;
			}

			TownshipRegion tr = TownshipRegion.GetTownshipAt(vendor);
			if (tr != null)
			{
				if (tr.TStone != null)
				{
					return tr.TStone.IsEnemy(b as PlayerMobile);
				}
			}
			return true; //if we can't find the township, don't interact with anyone
		}


		public static bool CheckMovement(BaseCreature vendor, Direction d)
		{
			BaseHouse housePlacedIn = BaseHouse.FindHouseAt(vendor.Home, vendor.Map, 20);
			if (housePlacedIn != null)
			{
				//next, get the new point and see if it's in the same house
				Point3D newLocation = new Point3D(vendor.Location);
				switch (d & Direction.Mask)
				{
					case Direction.North: newLocation.Y--; break;
					case Direction.South: newLocation.Y++; break;
					case Direction.West: newLocation.X--; break;
					case Direction.East: newLocation.X++; break;
					case Direction.Right: newLocation.X++; newLocation.Y--; break;
					case Direction.Left: newLocation.X--; newLocation.Y++; break;
					case Direction.Down: newLocation.X++; newLocation.Y++; break;
					case Direction.Up: newLocation.X--; newLocation.Y--; break;
				}
				BaseHouse houseAtNewLoc = BaseHouse.FindHouseAt(newLocation, vendor.Map, 20);

				if (houseAtNewLoc == null || houseAtNewLoc != housePlacedIn)
				{
					return false;
				}
			}

			return true;
		}

		public static bool IsRestrictedTownshipNPC(Mobile m)
		{
			if (m == null) return false;

			if (m is TSBanker || m is TSInnKeeper)
			{
				return true;
			}
			return false;
		}
		public static bool IsRestrictedTownshipNPCDeed(Item d)
		{
			if (d is TSBankerDeed || d is TSInnkeeperDeed)
			{
				return true;
			}
			return false;
		}
	}

	#endregion


	#region Trainers
	[TownshipNPC()]
	public class TSMageTrainer : Server.Mobiles.BaseVendor
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos { get { return m_SBInfos; } }

		public override void InitSBInfo()
		{
		}

		[Constructable]
		public TSMageTrainer()
			: base("the master arcane trainer")
		{
			SetSkill(SkillName.EvalInt, 85.0, 100.0);
			SetSkill(SkillName.Inscribe, 65.0, 100.0);
			SetSkill(SkillName.MagicResist, 64.0, 100.0);
			SetSkill(SkillName.Magery, 90.0, 100.0);
			SetSkill(SkillName.Wrestling, 60.0, 100.0);
			SetSkill(SkillName.Meditation, 85.0, 100.0);
		}

		public TSMageTrainer(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled 
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if( TownshipHelper.IsEnemyOfTownship(this, e.Mobile) )
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		//SMD: this works, but it sucks.
		//protected override void OnLocationChange(Point3D oldLocation)
		//{
		//	BaseHouse h1 = BaseHouse.FindHouseAt(this);
		//	BaseHouse h2 = BaseHouse.FindHouseAt(this.Home, this.Map, 20);
		//	if (h2 != null && h1 != h2)
		//	{
		//		Location = oldLocation;
		//		return;
		//	}
		//	base.OnLocationChange(oldLocation);
		//}

		//This is better :D
		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override VendorShoeType ShoeType
		{
			get { return Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals; }
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new Server.Items.Robe(Utility.RandomBlueHue()));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}


	[TownshipNPC()]
	public class TSArmsTrainer : Server.Mobiles.BaseVendor
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos { get { return m_SBInfos; } }

		public override void InitSBInfo()
		{
		}

		[Constructable]
		public TSArmsTrainer()
			: base("the master at-arms trainer")
		{
			SetSkill(SkillName.Fencing, 85.0, 100.0);
			SetSkill(SkillName.Swords, 65.0, 100.0);
			SetSkill(SkillName.Macing, 64.0, 100.0);
			SetSkill(SkillName.Parry, 90.0, 100.0);
			SetSkill(SkillName.Tactics, 60.0, 100.0);
			SetSkill(SkillName.Anatomy, 85.0, 100.0);
		}

		public TSArmsTrainer(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override VendorShoeType ShoeType
		{
			get { return Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals; }
		}

		public override void InitOutfit()
		{
			base.InitOutfit();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}



	[TownshipNPC()]
	public class TSRogueTrainer : Server.Mobiles.BaseVendor
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos { get { return m_SBInfos; } }

		public override void InitSBInfo()
		{
		}

		[Constructable]
		public TSRogueTrainer()
			: base("the master rogue trainer")
		{
			SetSkill(SkillName.Stealing, 85.0, 100.0);
			SetSkill(SkillName.Hiding, 65.0, 100.0);
			SetSkill(SkillName.Stealth, 64.0, 100.0);
			SetSkill(SkillName.Snooping, 90.0, 100.0);
			SetSkill(SkillName.Poisoning, 60.0, 100.0);
			SetSkill(SkillName.Lockpicking, 85.0, 100.0);
			SetSkill(SkillName.DetectHidden, 85.0, 100.0);
			SetSkill(SkillName.RemoveTrap, 85.0, 100.0);
		}

		public TSRogueTrainer(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override VendorShoeType ShoeType
		{
			get { return Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals; }
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			if (Utility.RandomBool())
				AddItem(new Server.Items.Kryss());
			else
				AddItem(new Server.Items.Dagger());
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}


	[TownshipNPC()]
	public class TSEmissary : Server.Mobiles.BaseVendor
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos { get { return m_SBInfos; } }

		public override void InitSBInfo()
		{
		}

		[Constructable]
		public TSEmissary()
			: base("the emissary of Lord British")
		{
		}

		public TSEmissary(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override VendorShoeType ShoeType
		{
			get { return VendorShoeType.ThighBoots; }
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			//AddItem(new Server.Items.Robe(Utility.RandomBlueHue()));
			int hue = 0x0;
			AddItem(new LongPants(hue));
			AddItem(new FancyShirt(hue));
			AddItem(new GoldRing());
			//Runebook runebook = new Runebook();
			//runebook.Hue = 0x47E;
			//runebook.Name = "Rules of Engagement";
			//AddItem(runebook);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}


	[TownshipNPC()]
	public class TSEvocator : Server.Mobiles.BaseVendor
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos { get { return m_SBInfos; } }

		public override void InitSBInfo()
		{
		}

		[Constructable]
		public TSEvocator()
			: base("the evocator")
		{
		}

		public TSEvocator(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override VendorShoeType ShoeType
		{
			get { return VendorShoeType.ThighBoots; }
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			int hue = 0x0;
			AddItem(new Robe(hue));
			AddItem(new LongPants(hue));
			AddItem(new FancyShirt(hue));
			AddItem(new WizardsHat(hue));
			AddItem(new GoldRing());
			//Runebook runebook = new Runebook();
			//runebook.Hue = 0x47E;
			//runebook.Name = "Writ of Travel";
			//AddItem(runebook);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}
	#endregion


	#region TSBanker

	[TownshipNPC()]
	public class TSBanker : Server.Mobiles.Banker
	{
		[Constructable]
		public TSBanker() 
			: base()
		{
		}

		public TSBanker(Serial serial)
			: base(serial)
		{
		}

		public override void OnDelete()
		{
			//get mobiles in area and close their bankboxes
			try
			{
				Region r = this.Region;
				foreach (Mobile m in r.Mobiles.Values)
				{
					try
					{
						if (m != null && m.BankBox != null)
						{
							m.BankBox.Close();
							m.Send(new Network.MobileUpdate(m)); //send a update packet to let client know BB is closed.
						}
					}
					catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
				}
			}
			catch(Exception ex)
			{
				Scripts.Commands.LogHelper.LogException(ex);
			}
			base.OnDelete();
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

	}

	#endregion

	#region TSAnimalTrainer

	[TownshipNPC()]
	public class TSAnimalTrainer : Server.Mobiles.AnimalTrainer
	{
		[Constructable]
		public TSAnimalTrainer()
			: base()
		{
		}

		public TSAnimalTrainer(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}

	}

	#endregion

	#region TSMage

	[TownshipNPC()]
	public class TSMage : Server.Mobiles.Mage
	{
		[Constructable]
		public TSMage()
			: base()
		{
		}

		public TSMage(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}

	#endregion

	#region TSAlchemist

	[TownshipNPC()]
	public class TSAlchemist : Server.Mobiles.Alchemist
	{
		[Constructable]
		public TSAlchemist()
			: base()
		{
		}

		public TSAlchemist(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}

	#endregion

	#region TSPrivisioner

	public class TSSBProvisioner : SBProvisioner
	{
		private ArrayList m_BuyInfo = null;

		public TSSBProvisioner()
		{
			m_BuyInfo = new ArrayList();
			m_BuyInfo.Add(new GenericBuyInfo(typeof(StoneWallCreationTool), 1000, 20, 0x13E3, 819));
			m_BuyInfo.Add(new GenericBuyInfo(typeof(WoodenWallCreationTool), 1000, 20, 0x1035, 802));
			m_BuyInfo.Add(new GenericBuyInfo(typeof(WallCustomizationTool), 10000, 20, 0xFC1, 803));
			m_BuyInfo.AddRange(base.BuyInfo);
		}

		public override IShopSellInfo SellInfo 
		{ 
			get 
			{
				return base.SellInfo;
			} 
		}
		public override ArrayList BuyInfo 
		{
			get
			{
				return m_BuyInfo;
			}
		}
	}

	[TownshipNPC()]
	public class TSProvisioner : Server.Mobiles.Provisioner
	{
		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos { get { return m_SBInfos; } }

		[Constructable]
		public TSProvisioner()
			: base()
		{
		}

		public TSProvisioner(Serial serial)
			: base(serial)
		{
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new TSSBProvisioner());
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}

	#endregion

	#region TSInnKeeper

	[TownshipNPC()]
	public class TSInnKeeper : Server.Mobiles.InnKeeper
	{
		[Constructable]
		public TSInnKeeper()
			: base()
		{
		}

		public TSInnKeeper(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}

	#endregion

	#region TSTownCrier

	[TownshipNPC()]
	public class TSTownCrier : TownCrier
	{
		[Constructable]
		public TSTownCrier()
			: base()
		{
		}

		public TSTownCrier(Serial serial)
			: base(serial)
		{
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

	}

	#endregion

	#region TSLookout

	[TownshipNPC()]
	public class TSLookout : BaseVendor
	{
		private const int LOOKOUTRANGE = 20;

		private ArrayList m_SBInfos = new ArrayList();
		protected override ArrayList SBInfos { get { return m_SBInfos; } }

		public override void InitSBInfo()
		{
		}

		[Constructable]
		public TSLookout()
			: base("the lookout")
		{
			this.CantWalk = true;
		}

		public TSLookout(Serial serial)
			: base(serial)
		{
			this.CantWalk = true;
		}

		private Hashtable m_LatestReported = new Hashtable();
		private const double SecondsBetweenReports = 30.0;

		private bool HasntBeenReportedLately(Mobile m)
		{
			bool bCanReport = true;
			try
			{
				if (m_LatestReported.Contains(m))
				{
					if (DateTime.Now > ((DateTime)m_LatestReported[m]).AddSeconds(SecondsBetweenReports))
					{
						bCanReport = true;
						m_LatestReported[m] = DateTime.Now;
					}
					else
					{
						bCanReport = false;
					}
				}
				else
				{
					bCanReport = true;
					AddToReported(m);
				}
			}
			catch (Exception e)
			{
				Scripts.Commands.LogHelper.LogException(e);
			}
			return bCanReport;
		}
		private void AddToReported(Mobile m)
		{
			m_LatestReported.Add(m, DateTime.Now);
			try
			{
				ArrayList toRemove = new ArrayList();
				foreach (object key in m_LatestReported.Keys)
				{
					if (DateTime.Now > ((DateTime)m_LatestReported[key]).AddSeconds(SecondsBetweenReports))
					{
						toRemove.Add(key);
					}
				}
				foreach (object o in toRemove)
				{
					m_LatestReported.Remove(o);
				}
			}
			catch (Exception e)
			{
				Scripts.Commands.LogHelper.LogException(e);
			}
		}

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			if (m is PlayerMobile)
			{
				Regions.TownshipRegion tr = Server.Regions.TownshipRegion.GetTownshipAt(this);
				if (tr != null && tr.TStone != null)
				{
					if (CanSee(m) && InLOS(m) && tr.TStone.IsEnemy(m as PlayerMobile))
					{
						if (InRange(m, LOOKOUTRANGE))
						{
							if (HasntBeenReportedLately(m)) //hasn't been reported lately
							{
								if (tr.TStone.Guild != null)
								{
									string message = string.Format("The lookout {0} reports that an enemy named {1} is near {2}.", this.Name, m.Name, this.Female?"her":"him");
									tr.TStone.Guild.GuildMessage(message);
									try
									{
										message = "[" + TownshipStone.GetTownshipSizeDesc(tr.TStone.ActivityLevel) + " of the " + tr.TStone.GuildName + "]: " + message;
										foreach (Server.Guilds.Guild g in tr.TStone.Guild.Allies)
										{
											g.GuildMessage(message);
										}
									}
									catch (Exception exc)
									{
										Scripts.Commands.LogHelper.LogException(exc);
									}
								}
							}
						}
					}
				}
			}
			base.OnMovement(m, oldLocation);
		}

		public override bool CanOpenDoors
		{
			get
			{
				return false;
			}
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override bool AllowEquipFrom(Mobile from)
		{
			if (TownshipHelper.IsTownshipNPCOwner(from, this)
				&& from.InRange(this, 3)
				&& from.InLOS(this))
			{
				return true;
			}

			return base.AllowEquipFrom(from);
		}

		public override void AddCustomContextEntries(Mobile from, ArrayList list)
		{
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Handled
				&& TownshipHelper.IsInSameHouse(this, e.Mobile)
				)
			{
				if (TownshipHelper.IsEnemyOfTownship(this, e.Mobile))
				{
					e.Mobile.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
				}
				else
				{
					base.OnSpeech(e);
				}
			}
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			if (TownshipHelper.CheckMovement(this, d) == false)
			{
				newZ = this.Z;
				return false;
			}
			return base.CheckMovement(d, out newZ);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();


			//safety :-)
			this.CantWalk = true;
		}

	}

	#endregion

}
