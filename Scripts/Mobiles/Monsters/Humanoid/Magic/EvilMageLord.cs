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

/* Scripts/Mobiles/Monsters/Humanoid/Magic/EvilMageLord.cs
 * ChangeLog
 *  12/03/06 Taran Kain
 *      Set Female = false. No trannies!
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *  7/02/06, Kit
 *		InitBody/InitOutfit additions
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	5/10/05, Adam
 *		Return AI to old AI AI_Mage
 *	5/05/05, Kit
 *		Added in first generation evil mage ai.
 *	4/13/05, Kit
 *		Switch to new region specific loot model
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
using Server.Items;
using Server.Engines.IOBSystem;

namespace Server.Mobiles 
{ 
	[CorpseName( "an evil mage lord corpse" )] 
	public class EvilMageLord : BaseCreature 
	{ 
		[Constructable] 
		public EvilMageLord() : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.25, 0.5 ) 
		{ 
			Hue = Utility.RandomSkinHue();
			IOBAlignment = IOBAlignment.Council;
			ControlSlots = 3;
		
			SetStr( 81, 105 );
			SetDex( 191, 215 );
			SetInt( 126, 150 );

			SetHits( 49, 63 );

			SetDamage( 5, 10 );

			SetSkill( SkillName.EvalInt, 100.0 );
			SetSkill( SkillName.Magery, 95.1, 100.0 );
			SetSkill( SkillName.Meditation, 93, 100.0 );
			SetSkill( SkillName.MagicResist, 77.5, 100.0 );
			SetSkill( SkillName.Tactics, 65.0, 87.5 );
			SetSkill( SkillName.Wrestling, 20.3, 80.0 );

			Fame = 10500;
			Karma = -10500;
			
			InitBody();
			InitOutfit();

			VirtualArmor = 16;
			
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override int Meat{ get{ return 1; } }

		public EvilMageLord( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void InitBody()
		{
			Name = NameList.RandomName( "evil mage lord" );
            Female = false;
			Body = 0x190; //No More Todd Mages! Old line was "Body = Utility.RandomList( 125, 126 );"
		}
		public override void InitOutfit()
		{
			WipeLayers();
			if ( Utility.RandomBool() )
				AddItem( new Shoes( Utility.RandomBlueHue() ) );
			else
				AddItem( new Sandals( Utility.RandomBlueHue() ) );

			//New Fall Fashions!

			Item EvilMageRobe = new Robe();
			EvilMageRobe.Hue = 0x1;
			EvilMageRobe.LootType = LootType.Newbied;
			AddItem( EvilMageRobe );

			Item EvilWizHat = new WizardsHat();
			EvilWizHat.Hue = 0x1;
			EvilWizHat.LootType = LootType.Newbied;
			AddItem( EvilWizHat );

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

			Item beard = new MediumLongBeard();
			beard.Hue = 0x47E;
			beard.Movable = false;
			beard.Layer = Layer.FacialHair;
			AddItem(beard);
			
			//End Fashion Statement
			
		}
		public override void GenerateLoot()
		{
			PackReg( 23 );
			PackScroll( 2, 7 );
			PackScroll( 2, 7 );
			PackItem( new Robe( Utility.RandomMetalHue() ) ); // Former AddItem moved to the loot section
			PackItem( new WizardsHat( Utility.RandomMetalHue() ) ); // Former AddItem moved to the loot section

			// pack the gold
			PackGold(100, 130);

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
