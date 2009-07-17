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

/* Scripts/Mobiles/Monsters/Humanoid/Melee/Savage.cs
 *	ChangeLog:
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *  7/02/06, Kit
 *		InitBody/InitOutfit additions
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 1 lines removed.
 *	4/13/05, Kit
 *		Switch to new region specific loot model
 *	1/15/05, Adam
 *		Remove drop of 'plain' mask
 *	12/15/04, Pix
 *		Removed damage mod to big pets.
 *	12/11/04, Pix
 *		Changed ControlSlots for IOBF.
 *  11/10/04, Froste
 *      Implemented new random IOB drop system and changed drop change to 12%
 *	11/05/04, Pigpen
 *		Made changes for Implementation of IOBSystem. Changes include:
 *		Removed IsEnemy and Aggressive Action Checks. These are now handled in BaseCreature.cs
 *		Set Creature IOBAlignment to Savage.
 *	10/1/04, Adam
 *		Change BandageDelay from 10 to 10-13
 *	0/19/04, Adam
 *		Have Savages Rummage Corpses
 *	8/27/04, Adam
 *		Have all savages wear masks, but only 5% drop
 *	8/23/04, Adam
 *		Increase gold to 100-150
 *		Increase berry drop to 20% OR bola to 5%
 *		Decrease tribal mask drop to 5% OR orkish mask 5%
 *	6/11/04, mith
 *		Moved the equippable combat items out of OnBeforeDeath()
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *	5/23/04 smerX
 *		Enabled healing
 *
 */

using System;
using Server;
using Server.Misc;
using Server.Items;
using Server.Engines.IOBSystem;

namespace Server.Mobiles
{
	[CorpseName( "a savage corpse" )]
	public class Savage : BaseCreature
	{
		[Constructable]
		public Savage() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.25, 0.5 )
		{
						
			IOBAlignment = IOBAlignment.Savage;
			ControlSlots = 2;

			SetStr( 96, 115 );
			SetDex( 86, 105 );
			SetInt( 51, 65 );

			SetDamage( 23, 27 );


			SetSkill( SkillName.Fencing, 60.0, 82.5 );
			SetSkill( SkillName.Macing, 60.0, 82.5 );
			SetSkill( SkillName.Poisoning, 60.0, 82.5 );
			SetSkill( SkillName.MagicResist, 57.5, 80.0 );
			SetSkill( SkillName.Swords, 60.0, 82.5 );
			SetSkill( SkillName.Tactics, 60.0, 82.5 );

			Fame = 1000;
			Karma = -1000;
			
			InitBody();
			InitOutfit();

			PackItem( new Bandage( Utility.RandomMinMax( 1, 15 ) ) );
			
		}

		public override int Meat{ get{ return 1; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool CanRummageCorpses{ get{ return true; } }

		public override bool CanBandage{ get{ return true; } }
		public override TimeSpan BandageDelay{ get{ return TimeSpan.FromSeconds( Utility.RandomMinMax( 10, 13 ) ); } }

		public override void InitBody()
		{
			Name = NameList.RandomName( "savage" );

			if ( Female = Utility.RandomBool() )
				Body = 184;
			else
				Body = 183;
		}
		public override void InitOutfit()
		{
			WipeLayers();
			AddItem( new Spear() );
			AddItem( new BoneArms() );
			AddItem( new BoneLegs() );

			// all savages wear a mask, but won't drop this one
			//	see OnBeforeDeath for the mask they do drop
			SavageMask mask = new SavageMask();
			mask.LootType = LootType.Newbied;
			AddItem( mask );
			
		}
		public override void AlterMeleeDamageTo( Mobile to, ref int damage )
		{
			//if ( to is Dragon || to is WhiteWyrm || to is SwampDragon || to is Drake || to is Nightmare || to is Daemon )
			//	damage *= 5;
		}

		public Savage( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGold( 100, 150 );

			// berry or bola
			if ( Female )
			{
				if (Utility.RandomDouble() < 0.30)
					PackItem( new TribalBerry() );
			}
			else
			{
				if (Utility.RandomDouble() < 0.05)
					PackItem( new BolaBall() );
			}

			// Froste: 12% random IOB drop
            		if (0.12 > Utility.RandomDouble())
            		{
                		Item iob = Loot.RandomIOB();
                		PackItem(iob);
            		}

			if (IOBRegions.GetIOBStronghold(this) == IOBAlignment)
			{
				// 30% boost to gold
				PackGold( base.GetGold()/3 );
			}
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
