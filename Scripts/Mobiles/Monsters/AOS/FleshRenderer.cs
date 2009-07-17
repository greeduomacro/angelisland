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

/* Scripts/Mobiles/Monsters/AOS/FleshRenderer.cs
 * ChangeLog
 *	4/12/09, Adam
 *		Update special armor drop to not use SDrop system
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 * 	4/11/05, Adam
 *		Update to use new version of Loot.ImbueWeaponOrArmor()
 *	3/28/05, Adam
 *		Use weighted table selection code for weapon/armor attr in Loot.cs
 *	3/21/05, Adam
 *		Cleaned up weighted table selection code for weapon/armor attr
 *	9/14/04, Pigpen
 *		Remove Treasure Map as possible Loot.
 *	9/11/04, Adam
 *		Add TreasureMapLevel 5
 *  9/11/04, Pigpen
 *		add Armor type of Dread Steel to random drop.
 *		add Weighted system of high end loot. with 5% chance of slayer on wep drops.
 *		Changed gold drop to 2500-3500gp 
 *  7/24/04, Adam
 *		add 25% chance to get a Random Slayer Instrument
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "a fleshrenderer corpse" )]
	public class FleshRenderer : BaseCreature
	{
		public override WeaponAbility GetWeaponAbility()
		{
			return Utility.RandomBool() ? WeaponAbility.Dismount : WeaponAbility.ParalyzingBlow;
		}

		[Constructable]
		public FleshRenderer() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.175, 0.350 )
		{
			Name = "a fleshrenderer";
			Body = 315;
			BardImmune = true;

			SetStr( 401, 460 );
			SetDex( 201, 210 );
			SetInt( 221, 260 );

			SetHits( 2200 );

			SetDamage( 16, 20 );



			SetSkill( SkillName.MagicResist, 155.1, 160.0 );
			SetSkill( SkillName.Tactics, 100.0 );
			SetSkill( SkillName.Wrestling, 90.1, 100.0 );

			Fame = 23000;
			Karma = -23000;

			VirtualArmor = 24;
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

			if ( !Summoned && !NoKillAwards && DemonKnight.CheckArtifactChance( this ) )
				DemonKnight.DistributeArtifact( this );
		}

		public override int Meat{ get{ return 20; } }
		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }
		public override bool AutoDispel{ get{ return true; } }
		public override int TreasureMapLevel{ get{ return 5; } }

		public FleshRenderer( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGem();
			PackGold( 2500, 3500 );
			//PackMagicEquipment( 2, 3, 0.90, 0.90 );
			//PackMagicEquipment( 2, 3, 0.50, 0.50 );

			// adam: add 25% chance to get a Random Slayer Instrument
			PackSlayerInstrument(.25);
			
			// Pigpen: Add the Dread Steel (rare) armor to this mini-boss.
			if (Utility.RandomDouble() < 0.10)
			{
				switch ( Utility.Random( 7 ) )
				{
					case 0: PackItem(new DreadSteelLeggings(), false); break;	// Leggings
					case 1: PackItem(new DreadSteelArms(), false); break;	// arms
					case 2: PackItem(new DreadSteelTunic(), false); break;	// Chest
					case 3: PackItem(new DreadSteelArmor(), false); break;	// Female Chest
					case 4: PackItem(new DreadSteelGorget(), false); break;	// gorget
					case 5: PackItem(new DreadSteelGloves(), false); break;	// gloves
					case 6: PackItem(new DreadSteelHelm(), false); break;	// helm
				}
			}

			// Use our unevenly weighted table for chance resolution
			Item item;
			item = Loot.RandomArmorOrShieldOrWeapon();
			PackItem( Loot.ImbueWeaponOrArmor (item, 6, 0, false) );
		}

		public override int GetAttackSound()
		{
			return 0x34C;
		}

		public override int GetHurtSound()
		{
			return 0x354;
		}

		public override int GetAngerSound()
		{
			return 0x34C;
		}

		public override int GetIdleSound()
		{
			return 0x34C;
		}

		public override int GetDeathSound()
		{
			return 0x354;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
