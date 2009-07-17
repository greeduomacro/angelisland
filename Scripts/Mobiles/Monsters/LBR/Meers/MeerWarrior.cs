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

/* Scripts/Mobiles/Monsters/LBR/Meers/MeerWarrior.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Misc;
using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
	[CorpseName( "a meer corpse" )]
	public class MeerWarrior : BaseCreature
	{
		[Constructable]
		public MeerWarrior() : base( AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.25, 0.5 )
		{
			Name = "a meer warrior";
			Body = 771;
			BardImmune = true;

			SetStr( 86, 100 );
			SetDex( 186, 200 );
			SetInt( 86, 100 );

			SetHits( 52, 60 );

			SetDamage( 12, 19 );



			SetSkill( SkillName.MagicResist, 91.0, 100.0 );
			SetSkill( SkillName.Tactics, 91.0, 100.0 );
			SetSkill( SkillName.Wrestling, 91.0, 100.0 );

			VirtualArmor = 22;

			Fame = 2000;
			Karma = 5000;
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool InitialInnocent{ get{ return true; } }

		public override void OnDamage( int amount, Mobile from, bool willKill )
		{
			if ( from != null && !willKill && amount > 3 && from != null && !InRange( from, 7 ) )
			{
				this.MovingEffect( from, 0xF51, 10, 0, false, false );
				SpellHelper.Damage( TimeSpan.FromSeconds( 1.0 ), from, this, Utility.RandomMinMax( 30, 40 ) - (Core.AOS ? 0 : 10), 100, 0, 0, 0, 0 );
			}

			base.OnDamage( amount, from, willKill );
		}

		public override int GetHurtSound()
		{
			return 0x156;
		}

		public override int GetDeathSound()
		{
			return 0x15C;
		}

		public MeerWarrior( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGold( 25, 50 );
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
