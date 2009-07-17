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

/* Scripts/Mobiles/Monsters/Misc/Melee/Centaur.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *  07/02/06, Kit
 *		InitBody/InitOutfit additions, changed range fight to 6
 *  08/29/05 TK
 *		Changed AIType to Archer
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *	6/11/04, mith
 *		Moved the equippable items out of OnBeforeDeath()
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a centaur corpse" )]
	public class Centaur : BaseCreature
	{
		[Constructable]
		public Centaur() : base( AIType.AI_Archer, FightMode.Aggressor, 10, 6, 0.25, 0.5 )
		{
		
			BaseSoundID = 679;

			SetStr( 202, 300 );
			SetDex( 104, 260 );
			SetInt( 91, 100 );

			SetHits( 130, 172 );

			SetDamage( 13, 24 );



			SetSkill( SkillName.Anatomy, 95.1, 115.0 );
			SetSkill( SkillName.Archery, 95.1, 100.0 );
			SetSkill( SkillName.MagicResist, 50.3, 80.0 );
			SetSkill( SkillName.Tactics, 90.1, 100.0 );
			SetSkill( SkillName.Wrestling, 95.1, 100.0 );

			Fame = 6500;
			Karma = 0;

			InitBody();
			InitOutfit();

			VirtualArmor = 50;

			
			PackItem( new Arrow( Utility.RandomMinMax( 80, 90 ) ) ); // OSI it is different: in a sub backpack, this is probably just a limitation of their engine
		}

		public override int Meat{ get{ return 1; } }
		public override int Hides{ get{ return 8; } }
		public override HideType HideType{ get{ return HideType.Spined; } }

		public Centaur( Serial serial ) : base( serial )
		{
		}

		public override void InitBody()
		{
			Name = NameList.RandomName( "centaur" );
			Body = 101;
			
		}
		public override void InitOutfit()
		{
			WipeLayers();
			AddItem( new Bow() );
			
		}
		public override void GenerateLoot()
		{
			PackGold( 180, 250 );
			PackGem();
			PackMagicEquipment( 1, 2, 0.15, 0.15 );
			// Category 2 MID
			PackMagicItem( 1, 1, 0.05 );
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

			if ( BaseSoundID == 678 )
				BaseSoundID = 679;
		}
	}
}
