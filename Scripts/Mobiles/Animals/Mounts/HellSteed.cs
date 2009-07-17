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

/* Scripts/Mobiles/Animals/Mounts/HellSteed.cs
 * ChangeLog
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 *	12/12/04, Adam
 *		Remove IOBAlignment - tamables should not have alignment
 *	11/19/04
 *		1. AIType.AI_Melee
 *		2. Night mare loot and resources
 *		3. Add firebreath
 *		a. Base on Drake with bump in damage (SetDamage)
 *	11/19/04, Adam
 *		Make tamable: 98 skill, 2 control slots
 *	11/17/04, Adam
 *		Set the IOBAlignment to Undead
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 */

using System;
using Server.Mobiles;
using Server.Misc;
using Server.Network;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a hellsteed corpse" )]
	public class HellSteed : BaseMount
	{
		[Constructable] 
		public HellSteed() : this( "a hellsteed" )
		{
		}

		[Constructable]
		public HellSteed(string name) : base( name, 793, 0x3EBB, AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			// Adam: Remove IOBAlignment - tamables should not have alignment
			IOBAlignment = IOBAlignment.None;

			SetStr(401, 430);
			SetDex(133, 152);
			SetInt(101, 140);

			SetHits(241, 258);

			SetDamage(16, 20);



			SetSkill(SkillName.MagicResist, 65.1, 80.0);
			SetSkill(SkillName.Tactics, 65.1, 90.0);
			SetSkill(SkillName.Wrestling, 65.1, 80.0);

			Fame = 5500;
			Karma = -5500;

			VirtualArmor = 46;

			Tamable = true;
			ControlSlots = 2;
			MinTameSkill = 98.9;
		}

		public override bool HasBreath { get { return true; } } // fire breath enabled
		public override int Meat { get { return 5; } }
		public override int Hides { get { return 10; } }
		public override HideType HideType { get { return HideType.Barbed; } }
		public override FoodType FavoriteFood { get { return FoodType.Meat; } }
		public override Poison PoisonImmune { get { return Poison.Lethal; } }

		public HellSteed( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGem();
			PackGold(250, 350);
			PackItem(new SulfurousAsh(Utility.RandomMinMax(3, 5)));
			PackScroll(1, 5);
			PackPotion();
			// Category 3 MID
			PackMagicItem(1, 2, 0.10);
			PackMagicItem(1, 2, 0.05);
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
