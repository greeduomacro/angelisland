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

/* Scripts/Mobiles/Monsters/Humanoid/Magic/Balron.cs
 * ChangeLog
 *	4/12/09, Adam
 *		Update special armor drop to not use SDrop system
 *  7/08/06, Kit
 *		Added hellish bustier/shorts/skirt
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 8 lines removed.
 * 	4/11/05, Adam
 *		Update to use new version of Loot.ImbueWeaponOrArmor()
 *	3/28/05, Adam
 *		Use weighted table selection code for weapon/armor attr in Loot.cs
 *	3/21/05, Adam
 *		Cleaned up weighted table selection code for weapon/armor attr
 *  9/14/04, Pigpen
 *		Removed treasure map as loot.
 *  9/11/04, Pigpen
 *		add Armor type of Hellish to random drop.
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
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a balron corpse" )]
	public class Balron : BaseCreature
	{
		[Constructable]
		public Balron () : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = NameList.RandomName( "balron" );
			Body = 40;
			BaseSoundID = 357;

			SetStr( 986, 1185 );
			SetDex( 177, 255 );
			SetInt( 151, 250 );

			SetHits( 592, 711 );

			SetDamage( 22, 29 );



			SetSkill( SkillName.Anatomy, 25.1, 50.0 );
			SetSkill( SkillName.EvalInt, 90.1, 100.0 );
			SetSkill( SkillName.Magery, 95.5, 100.0 );
			SetSkill( SkillName.Meditation, 25.1, 50.0 );
			SetSkill( SkillName.MagicResist, 100.5, 150.0 );
			SetSkill( SkillName.Tactics, 90.1, 100.0 );
			SetSkill( SkillName.Wrestling, 90.1, 100.0 );

			Fame = 24000;
			Karma = -24000;

			VirtualArmor = 90;

		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override int Meat{ get{ return 1; } }
		public override bool AutoDispel{ get{ return true; } }


		public Balron( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackReg( 3 );
			PackItem( new Longsword() );
			PackGold( 2500, 3500 );
			PackScroll( 6, 8 );
			PackScroll( 6, 8 );
			//PackMagicEquipment( 2, 3, 0.80, 0.80 );
			//PackMagicEquipment( 2, 3, 0.20, 0.20 );

			// adam: add 25% chance to get a Random Slayer Instrument
			PackSlayerInstrument(.25);

			// Pigpen: Add the Arctic Storm (rare) armor to this mini-boss.
			if (Utility.RandomDouble() < 0.10)
			{
				switch ( Utility.Random( 10 ) )
				{
					case 0: PackItem(new HellishArmor(), false); break;		// female chest
					case 1: PackItem(new HellishArms(), false); break;		// arms
					case 2: PackItem(new HellishTunic(), false); break;		// male chest
					case 3: PackItem(new HellishGloves(), false); break;		// gloves
					case 4: PackItem(new HellishGorget(), false); break;		// gorget
					case 5: PackItem(new HellishLeggings(), false); break;	// legs
					case 6: PackItem(new HellishHelmet(), false); break;		// helm
					case 7: PackItem(new HellishBustier(), false); break;	//bustier
					case 8: PackItem(new HellishShorts(), false); break;	//shorts
					case 9: PackItem(new HellishSkirt(), false); break;	//skirt
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
