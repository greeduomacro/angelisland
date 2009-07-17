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

/* Scripts/Mobiles/Monsters/Reptile/Melee/Scorpion.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 *	9/30/04, Adam
 *		Scale HitPoison (poison strength) based on poisoning skill
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a scorpion corpse" )]
	public class Scorpion : BaseCreature
	{
		//private bool m_wasPoisoned = false;

		[Constructable]
		public Scorpion() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.25, 0.5 )
		{
			Name = "a scorpion";
			Body = 48;
			BaseSoundID = 397;

			SetStr( 73, 115 );
			SetDex( 76, 95 );
			SetInt( 16, 30 );

			SetHits( 50, 63 );
			SetMana( 0 );

			SetDamage( 5, 10 );



			SetSkill( SkillName.Poisoning, 80.1, 100.0 );
			SetSkill( SkillName.MagicResist, 30.1, 35.0 );
			SetSkill( SkillName.Tactics, 60.3, 75.0 );
			SetSkill( SkillName.Wrestling, 50.3, 65.0 );

			Fame = 2000;
			Karma = -2000;

			VirtualArmor = 28;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 47.1;
		}

		public override int Meat{ get{ return 1; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat; } }
		public override PackInstinct PackInstinct{ get{ return PackInstinct.Arachnid; } }
		public override Poison PoisonImmune{ get{ return Poison.Greater; } }
		
		public override Poison HitPoison{ 
			get { 
				if ( Skills[SkillName.Poisoning].Base == 100.0 )
					return (0.8 >= Utility.RandomDouble() ? Poison.Greater : Poison.Deadly); 
				if ( Skills[SkillName.Poisoning].Base > 90.0 )
					return (0.8 >= Utility.RandomDouble() ? Poison.Regular : Poison.Greater); 
				if ( Skills[SkillName.Poisoning].Base > 80.0 )
					return (0.8 >= Utility.RandomDouble() ? Poison.Lesser : Poison.Regular); 
				return ( Poison.Lesser ); 
			} 
		}

		public Scorpion( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGold( 25, 50 );
			PackItem( new LesserPoisonPotion() );
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
