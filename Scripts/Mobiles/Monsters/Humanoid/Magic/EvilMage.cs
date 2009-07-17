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

/* Scripts/Mobiles/Monsters/Humanoid/Magic/EvilMage.cs
 * ChangeLog
 *  12/03/06 Taran Kain
 *      Set Female to false. No trannies!
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *  7/02/06, Kit
 *		InitBody/InitOutfit additions
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 5 lines removed.
 *	4/13/05, Kit
 *		Switch to new region specific loot model
 *	12/25/04, Adam
 *		Change ControlSlots from 3 to 2
 *	12/11/04, Pix
 *		Changed ControlSlots for IOBF.
 *  11/16/04, Froste
 *      Changed IOBAlignment to Council
 *  11/10/04, Froste
 *      Implemented new random IOB drop system and changed drop change to 12%
 *	11/05/04, Pigpen
 *		Made changes for Implementation of IOBSystem. Changes include:
 *		Removed IsEnemy and Aggressive Action Checks. These are now handled in BaseCreature.cs
 *		Set Creature IOBAlignment to Undead.
 *	11/2/04, Adam
 *		Increase gold if this is IOB mobile resides in it's Stronghold (Wind)
 *	9/26/04, Adam
 *		Add 5% IOB drop (BloodDrenchedBandana)
 *	7/21/04, mith
 *		IsEnemy() and AggressiveAction() code added to support Brethren property of BloodDrenchedBandana.
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *  10/6/04, Froste
 *		New Fall Fashions!!
 */

using System;
using Server;
using Server.Misc;
using Server.Items;
using Server.Engines.IOBSystem;

namespace Server.Mobiles 
{ 
	[CorpseName( "an evil mage corpse" )] 
	public class EvilMage : BaseCreature 
	{ 
		[Constructable] 
		public EvilMage() : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.25, 0.5 ) 
		{ 
			
			Title = "the evil mage";
			Hue = Utility.RandomSkinHue();	//new skin color
			
			IOBAlignment = IOBAlignment.Council;
			ControlSlots = 2;	

			SetStr( 81, 105 );
			SetDex( 91, 115 );
			SetInt( 96, 120 );

			SetHits( 49, 63 );

			SetDamage( 5, 10 );

			SetSkill( SkillName.EvalInt, 75.1, 100.0 );
			SetSkill( SkillName.Magery, 75.1, 100.0 );
			SetSkill( SkillName.MagicResist, 75.0, 97.5 );
			SetSkill( SkillName.Tactics, 65.0, 87.5 );
			SetSkill( SkillName.Wrestling, 20.2, 60.0 );

			Fame = 2500;
			Karma = -2500;

			InitBody();
			InitOutfit();

			VirtualArmor = 16;
		
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override int Meat{ get{ return 1; } }

		public EvilMage( Serial serial ) : base( serial )
		{
		}

		public override void InitBody()
		{
			Name = NameList.RandomName( "evil mage" );
            Female = false;
			Body = 0x190; //no more Todd McFarlane mage! Old line was "Body = 124;"
		}
		public override void InitOutfit()
		{
			WipeLayers();
			AddItem( new Sandals() );

			// New Fall Fashions!

			Item EvilMageRobe = new Robe();
			EvilMageRobe.Hue = 0x1;
			EvilMageRobe.LootType = LootType.Newbied;
			AddItem( EvilMageRobe );

			Item BDB = new BloodDrenchedBandana();
			BDB.LootType = LootType.Newbied;
			AddItem( BDB );

			Item Cloak = new Cloak();
			Cloak.Hue = 0x1;
			Cloak.LootType = LootType.Newbied;
			AddItem( Cloak );

			Item Bracelet = new GoldBracelet();
			Bracelet.LootType = LootType.Newbied;
			AddItem( Bracelet );

			Item Ring = new GoldRing();
			Ring.LootType = LootType.Newbied;
			AddItem ( Ring );
	
			Item hair = new LongHair();
			hair.Hue = 0x47E;
			hair.Layer = Layer.Hair;
			hair.Movable = false;
			AddItem( hair );

			Item beard = new Goatee();  
			beard.Hue = 0x47E;
			beard.Movable = false;
			AddItem(beard);

			//  End New Additions
			
		}
		public override void GenerateLoot()
		{
			PackReg( 6 );
			PackScroll( 2, 7 );
			PackItem( new Robe( Utility.RandomPinkHue() ) ); // Former AddItem moved to the loot section

			// pack the gold
			PackGold( 50, 100 );

            		// Froste: 12% random IOB drop
            		if (0.12 > Utility.RandomDouble())
            		{
                		Item iob = Loot.RandomIOB();
                		PackItem(iob);
            		}

            		// Category 2 MID
			PackMagicItem( 1, 1, 0.05 );
			
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
