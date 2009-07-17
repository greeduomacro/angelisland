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

/* Scripts/Mobiles/Monsters/Misc/Melee/EvilCentaur.cs
 * ChangeLog
 *  07/02/06, Kit
 *		InitBody/InitOutfit additions, changed rangefight to 6
 *  08/29/05 TK
 *		Changed AIType to Archer
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *	3/5/05, Adam
 *		1. First time checkin - based on centaur.cs
 *		2. Add healing
 *		3. Make evil (red)
 *		4. set FightMode to "Weakest". This is anti-bard code :)
 *		5. Add neg karma for kill
 *		6. reduce arrows from 80-90 to 20-30
 */

using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a centaur corpse" )]
	public class EvilCentaur : BaseCreature
	{
		[Constructable]
		public EvilCentaur() : base( AIType.AI_Archer, FightMode.All | FightMode.Weakest, 10, 6, 0.2, 0.4 )
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
			Karma = -6500;

			InitBody();
			InitOutfit();

			VirtualArmor = 50;

			
			PackItem( new Arrow( Utility.RandomMinMax( 20, 30 ) ) );
			PackItem( new Bandage( Utility.RandomMinMax( 1, 15 ) ) );
		}

		public override int Meat{ get{ return 1; } }
		public override int Hides{ get{ return 8; } }
		public override HideType HideType{ get{ return HideType.Spined; } }
		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool CanBandage{ get{ return true; } }
		public override TimeSpan BandageDelay{ get{ return TimeSpan.FromSeconds( Utility.RandomMinMax( 10, 13 ) ); } }

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
		public EvilCentaur( Serial serial ) : base( serial )
		{
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
