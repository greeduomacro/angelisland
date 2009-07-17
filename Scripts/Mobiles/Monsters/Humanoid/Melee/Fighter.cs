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

/* Scripts/Mobiles/Monsters/Humanoid/Melee/Fighter.cs
 * ChangeLog
 *  7/02/06, Kit
 *		InitBody/InitOutfit additions
 *	9/20/05, Adam
 *		Add the Parry skill
 *  9/20/05, Adam
 *		Make bard immune.
 *  9/19/05, Adam
 *		a. Change Karma loss to that for a 'good' aligned creature
 *		b. remove powder of transloacation
 *  9/16/05, Adam
 *		spawned from Brigand.cs.
 */

using System;
using System.Collections;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;
using Server.Network;
using Server.Engines.IOBSystem;

namespace Server.Mobiles
{
	public class Fighter : BaseCreature
	{
		[Constructable]
		public Fighter() : base( AIType.AI_Melee, FightMode.Aggressor | FightMode.Criminal, 10, 1, 0.2, 0.4 )
		{
			SpeechHue = Utility.RandomDyedHue();
			Title = "the fighter";
			Hue = Utility.RandomSkinHue();
			IOBAlignment = IOBAlignment.Good;
			ControlSlots = 2;
			BardImmune = true;

			SetStr( 96, 115 );
			SetDex( 86, 105 );
			SetInt( 51, 65 );

			SetDamage( 23, 27 );

			SetSkill( SkillName.Fencing, 60.0, 82.5 );
			SetSkill( SkillName.Macing, 60.0, 82.5 );
			SetSkill( SkillName.Parry, 80.0, 98.5 );
			SetSkill( SkillName.MagicResist, 57.5, 80.0 );
			SetSkill( SkillName.Swords, 60.0, 82.5 );
			SetSkill( SkillName.Tactics, 60.0, 82.5 );

			InitBody();
			InitOutfit();

			Fame = 1000;
			Karma = 1000;

			PackItem( new Bandage( Utility.RandomMinMax( 1, 15 ) ) );
		
		}

		public override bool AlwaysMurderer{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool CanRummageCorpses{ get{ return false; } }
		public override bool ClickTitle { get { return true; } }

		public override bool CanBandage{ get{ return true; } }
		public override TimeSpan BandageDelay{ get{ return TimeSpan.FromSeconds( Utility.RandomMinMax( 10, 13 ) ); } }

		public Fighter( Serial serial ) : base( serial )
		{
		}

		public override void InitBody()
		{
			if ( this.Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "female" );
				
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "male" );
			}
		}
		public override void InitOutfit()
		{
			WipeLayers();
			Item hair = new Item( Utility.RandomList( this.Female ? 0x203B : 0x2048, 0x203C, 0x203D ) );
			hair.Hue = Utility.RandomHairHue();
			hair.Layer = Layer.Hair;
			hair.Movable = false;
			AddItem( hair );
			
			AddItem( new ChainChest(), Utility.RandomBool() ? LootType.Newbied : LootType.Regular );
			AddItem( new ChainLegs(), Utility.RandomBool() ? LootType.Newbied : LootType.Regular );
			AddItem( new ChainCoif(), Utility.RandomBool() ? LootType.Newbied : LootType.Regular );
			AddItem( new Boots( Utility.RandomNeutralHue() ) );
			AddItem( new BodySash( Utility.RandomSpecialRedHue() ), LootType.Newbied ); // never drop, you need the IOB version ));

			if (Utility.RandomBool())
				AddItem( new MetalShield(), Utility.RandomBool() ? LootType.Newbied : LootType.Regular );

			switch ( Utility.Random( 7 ))
			{
				case 0: AddItem( new Longsword() ); break;
				case 1: AddItem( new Cutlass() ); break;
				case 2: AddItem( new Broadsword() ); break;
				case 3: AddItem( new Kryss() ); break;
				case 4: AddItem( new HammerPick() ); break;
				case 5: AddItem( new WarAxe() ); break;
				case 6: AddItem( new WarFork() ); break;
			}
	
		}

		public override void GenerateLoot()
		{
			PackGold( 100, 150 );

			// Froste: 12% random IOB drop
			if (0.12 > Utility.RandomDouble())
			{
				Item iob = Loot.RandomIOB();
				PackItem(iob);
			}

			// if we are in our own stronghold, add 1/3 more gold+
			if (IOBRegions.GetIOBStronghold(this) == IOBAlignment)
			{
				// 30% boost to gold
				PackGold( base.GetGold()/3 );
				
				// chance at powder of translocation
				if (CoreAI.PowderOfTranslocationAvail > Utility.RandomDouble())			
					PackItem(new PowderOfTranslocation(Utility.RandomMinMax( 1, 5 )));
			}
		}

		public override void Serialize( GenericWriter writer )
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
}
