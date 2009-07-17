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

/* Scripts/Mobiles/Monsters/Reptile/Magic/WhiteWyrm.cs
 * ChangeLog
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 *	12/21/05, Adam
 *		10% at a pure White Wyrm
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *	7/2/04
 *		Change chance to drop a magic item to 30% 
 *		add a 5% chance for a bonus drop at next intensity level
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a white wyrm corpse" )]
	public class WhiteWyrm : BaseCreature
	{
		[Constructable]
		public WhiteWyrm () : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			// Adam: 10% at a pure White Wyrm
			if (0.12 > Utility.RandomDouble())
			{
				Body = 59;
				Hue = 1153;
			}
			else
				Body = 49;

			Name = "a white wyrm";
			BaseSoundID = 362;

			SetStr( 721, 760 );
			SetDex( 101, 130 );
			SetInt( 386, 425 );

			SetHits( 433, 456 );

			SetDamage( 17, 25 );



			SetSkill( SkillName.EvalInt, 99.1, 100.0 );
			SetSkill( SkillName.Magery, 99.1, 100.0 );
			SetSkill( SkillName.MagicResist, 99.1, 100.0 );
			SetSkill( SkillName.Tactics, 97.6, 100.0 );
			SetSkill( SkillName.Wrestling, 90.1, 100.0 );

			Fame = 18000;
			Karma = -18000;

			VirtualArmor = 64;

			Tamable = true;
			ControlSlots = 3;
			MinTameSkill = 96.3;
		}

		public override void GenerateLoot()
		{
			int gems = Utility.RandomMinMax( 1, 5 );

			for ( int i = 0; i < gems; ++i )
				PackGem();

			PackGold( 800, 900 );
			PackMagicEquipment( 1, 3, 0.50, 0.50 );
			PackMagicEquipment( 1, 3, 0.15, 0.15 );

			// Category 4 MID
			PackMagicItem( 2, 3, 0.10 );
			PackMagicItem( 2, 3, 0.05 );
			PackMagicItem( 2, 3, 0.02 );
		}

		public override int TreasureMapLevel{ get{ return 4; } }
		public override int Meat{ get{ return 20; } }
		public override int Hides{ get{ return 40; } }
		public override HideType HideType{ get{ return HideType.Barbed; } }
//		public override int Scales{ get{ return 9; } }
//		public override ScaleType ScaleType{ get{ return ScaleType.White; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat | FoodType.Gold; } }

		public WhiteWyrm( Serial serial ) : base( serial )
		{
		}

		public override bool OnBeforeDeath()
		{
//			if( !IsBonded )
//			{
//				int gems = Utility.RandomMinMax( 1, 5 );
//				for ( int i = 0; i < gems; ++i )
//					PackGem();
//
//				PackGold( 800, 900 );
//				PackMagicEquipment( 1, 3, 0.50, 0.50 );
//				PackMagicEquipment( 1, 3, 0.15, 0.15 );
//
//				// Category 4 MID
//				PackMagicItem( 2, 3, 0.10 );
//				PackMagicItem( 2, 3, 0.05 );
//				PackMagicItem( 2, 3, 0.02 );
//			}

			return base.OnBeforeDeath();
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
