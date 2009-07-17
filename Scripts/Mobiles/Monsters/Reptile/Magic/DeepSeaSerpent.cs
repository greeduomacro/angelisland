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

/* Scripts/Mobiles/Monsters/Reptile/Magic/DeepSeaSerpent.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *  11/10/04, Froste
 *      Removed PirateHat as loot, now restricted to "brethren only" drop
 *  9/26/04, Jade
 *      Increased gold drop from (25,50) to (150,200).
 *  7/21/04, Adam
 *		CS0654: (line 101, column 18) Method 'Server.Utility.RandomBool()' referenced without parentheses
 *		Fixed a little mith'take ;p
 *	7/21/04, mith
 *		Added PirateHat as loot, 5% drop.
 *	6/29/04, Pix
 *		Fixed MIB loot to spawn for the current facet.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a deep sea serpents corpse" )]
	public class DeepSeaSerpent : BaseCreature
	{
		[Constructable]
		public DeepSeaSerpent() : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.25, 0.5 )
		{
			Name = "a deep sea serpent";
			Body = 150;
			BaseSoundID = 447;

			SetStr( 251, 425 );
			SetDex( 87, 135 );
			SetInt( 87, 155 );

			SetHits( 151, 255 );

			SetDamage( 6, 14 );



			SetSkill( SkillName.MagicResist, 60.1, 75.0 );
			SetSkill( SkillName.Tactics, 60.1, 70.0 );
			SetSkill( SkillName.Wrestling, 60.1, 70.0 );

			Fame = 6000;
			Karma = -6000;

			VirtualArmor = 60;
			CanSwim = true;
			CantWalk = true;
		}

		public override int TreasureMapLevel{ get{ return 1; } }
		public override bool HasBreath{ get{ return true; } }
		public override int Hides{ get{ return 10; } }
		public override HideType HideType{ get{ return HideType.Horned; } }
		public override int Meat{ get{ return 10; } }
		// public override int Scales{ get{ return 8; } }
		// public override ScaleType ScaleType{ get{ return ScaleType.Blue; } }

		public DeepSeaSerpent( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGold( 150, 200 );
			PackItem( new SulfurousAsh( 4 ) );
			PackItem( new BlackPearl( 4 ) );

			double chance = Utility.RandomDouble();
			// 30% chance to get a MIB or Special Net
			if ( chance < 0.30 )
				// If that succeeds, 50/50 chance to get one or the other
				if ( Utility.RandomBool() )
					PackItem( new SpecialFishingNet() );
				else 
					PackItem( new MessageInABottle(this.Map) );
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
