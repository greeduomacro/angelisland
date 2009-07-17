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

/* Scripts/Mobiles/Monsters/Humanoid/Melee/BrigandLeader.cs
 * ChangeLog
 *  7/02/06, Kit
 *		InitBody/InitOutfit Additions
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	4/13/05, Kit
 *		Switch to new region specific loot model
 *	2/4/05, Adam
 *		Hookup PowderOfTranslocation drop rates to the CoreManagementConsole
 *	1/25/05, Adam
 *		Add PowderOfTranslocation as region specific loot 
 *		Brigands are the only ones that carry this loot.
 *	1/2/05, Adam
 *		Cleanup name management, make use of Titles
 *			Show title when clicked = false
 *	12/30/04, Adam
 *		Created by Adam.
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
	public class BrigandLeader : BaseCreature
	{
		[Constructable]
		public BrigandLeader() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			SpeechHue = Utility.RandomDyedHue();
			Title = "the brigand leader";
			Hue = Utility.RandomSkinHue();
			IOBAlignment = IOBAlignment.Brigand;
			ControlSlots = 5;

			SetStr( 386, 400 );
			SetDex( 70, 90 );
			SetInt( 161, 175 );

			SetDamage( 20, 30 );

			SetSkill( SkillName.Anatomy, 125.0 );
			SetSkill( SkillName.Fencing, 46.0, 77.5 );
			SetSkill( SkillName.Macing, 35.0, 57.5 );
			SetSkill( SkillName.Poisoning, 60.0, 82.5 );
			SetSkill( SkillName.MagicResist, 83.5, 92.5 );
			SetSkill( SkillName.Swords, 125.0 );
			SetSkill( SkillName.Tactics, 125.0 );
			SetSkill( SkillName.Lumberjacking, 125.0 );

			InitBody();
			InitOutfit();

			Fame = 5000;
			Karma = -5000;

			VirtualArmor = 40;

			PackItem( new Bandage( Utility.RandomMinMax( 1, 15 ) ) );

		}
            
		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool ClickTitle { get { return true; } }

		public override bool CanBandage{ get{ return true; } }
		public override TimeSpan BandageDelay{ get{ return TimeSpan.FromSeconds( Utility.RandomMinMax( 13, 15 ) ); } }

		public BrigandLeader( Serial serial ) : base( serial )
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
			Item hair = new Item( Utility.RandomList( 0x203B, 0x2049, 0x2048, 0x204A ) );
			hair.Hue = Utility.RandomNondyedHue();
			hair.Layer = Layer.Hair;
			hair.Movable = false;
			AddItem( hair );

			AddItem( new Boots( Utility.RandomNeutralHue() ) );
			AddItem( new FancyShirt());
			AddItem(new Bandana());
			AddItem( new ExecutionersAxe() );

			if(Female)
				AddItem( new Skirt( Utility.RandomNeutralHue() ) );
			else
				AddItem( new ShortPants( Utility.RandomNeutralHue() ) );

		}

		public override void GenerateLoot()
		{
			PackGold( 300, 450 );

			// Category 2 MID
			PackMagicItem( 1, 1, 0.05 );

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
				
				// chance at powder of translocation
				if (CoreAI.PowderOfTranslocationAvail > Utility.RandomDouble())			
					PackItem(new PowderOfTranslocation(Utility.RandomMinMax( 5, 15 )));
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
