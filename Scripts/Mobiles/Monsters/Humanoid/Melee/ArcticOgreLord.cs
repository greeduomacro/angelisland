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

/* Scripts/Mobiles/Monsters/Humanoid/Melee/ArticOgreLord.cs
 * ChangeLog
 *	4/12/09, Adam
 *		Update special armor drop to not use SDrop system
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 * 	4/11/05, Adam
 *		Update to use new version of Loot.ImbueWeaponOrArmor()
 *	3/28/05, Adam
 *		Use weighted table selection code for weapon/armor attr in Loot.cs
 *	3/21/05, Adam
 *		Cleaned up weighted table selection code for weapon/armor attr
 *	9/14/04, Pigpen
 *		Remove Treasure map from loot.
 *	9/11/04, Adam
 *		Replace lvl 3 Treasure Map with lvl 5
 *		Change helm drop to 2%
 *  9/10/04, Pigpen
 *  	add Armor type of Arctic Storm to Random Drop
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
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Mobiles
{
	[CorpseName( "a frozen ogre lord's corpse" )]
	[TypeAlias( "Server.Mobiles.ArticOgreLord" )]
	public class ArcticOgreLord : BaseCreature
	{
		[Constructable]
		public ArcticOgreLord() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.3, 0.6 )
		{
			Name = "an arctic ogre lord";
			Body = 135;
			BaseSoundID = 427;

			SetStr( 1100, 1200 );
			SetDex( 66, 75 );
			SetInt( 46, 70 );

			SetHits( 1100, 1200 );

			SetDamage( 20, 25 );



			SetSkill( SkillName.MagicResist, 125.1, 140.0 );
			SetSkill( SkillName.Tactics, 90.1, 100.0 );
			SetSkill( SkillName.Wrestling, 90.1, 100.0 );

			Fame = 15000;
			Karma = -15000;

			VirtualArmor = 50;
		}

		public override int Meat{ get{ return 2; } }
		public override Poison PoisonImmune{ get{ return Poison.Regular; } }
		public override AuraType MyAura{ get{ return AuraType.Ice; } }
		public override bool AutoDispel{ get{ return true; } }

		public ArcticOgreLord( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGold( 2500, 3500 );
			PackItem( new Club() );
			//PackMagicEquipment( 1, 3, 0.30, 0.30 );

			// adam: add 25% chance to get a Random Slayer Instrument
			PackSlayerInstrument(.25);
			
			// Pigpen: Add the Arctic Storm (rare) armor to this mini-boss.
			if (Utility.RandomDouble() < 0.10)
			{
				switch ( Utility.Random( 7 ) )
				{
					case 0: PackItem( new ArcticStormArmor(), false ); break;	// female chest
					case 1: PackItem(new ArcticStormArms(), false); break;	// arms
					case 2: PackItem(new ArcticStormTunic(), false); break;	// male chest
					case 3: PackItem(new ArcticStormGloves(), false); break;	// gloves
					case 4: PackItem(new ArcticStormGorget(), false); break;	// gorget
					case 5: PackItem(new ArcticStormLeggings(), false); break;// legs
					case 6: PackItem(new ArcticStormHelm(), false); break;	// helm
				}
			} 
			
			
			// Use our unevenly weighted table for chance resolution
			Item item;
			item = Loot.RandomArmorOrShieldOrWeapon();
			PackItem( Loot.ImbueWeaponOrArmor (item, 6, 0, false) );
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
