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

/* Scripts/Mobiles/Monsters/Misc/Melee/PlagueBeast.cs
 * ChangeLog
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 *  9/26/04, Jade
 *      Decreased gold drop from (600, 1000) to (350, 500)
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a plague beast corpse" )]
	public class PlagueBeast : BaseCreature
	{
		[Constructable]
		public PlagueBeast() : base( AIType.AI_Melee, FightMode.All | FightMode.Closest, 10, 1, 0.3, 0.6 )
		{
			Name = "a plague beast";
			Body = 775;

			SetStr( 302, 500 );
			SetDex( 80 );
			SetInt( 16, 20 );

			SetHits( 318, 404 );

			SetDamage( 20, 24 );



			SetSkill( SkillName.MagicResist, 35.0 );
			SetSkill( SkillName.Tactics, 100.0 );
			SetSkill( SkillName.Wrestling, 100.0 );

			Fame = 13000;
			Karma = -13000;

			VirtualArmor = 30;
		}

		// TODO: Poison attack

		public override void OnDamagedBySpell( Mobile caster )
		{
			if ( caster != this && 0.25 > Utility.RandomDouble() )
			{
				BaseCreature spawn = new PlagueSpawn( this );

				spawn.Team = this.Team;
				spawn.MoveToWorld( this.Location, this.Map );
				spawn.Combatant = caster;

				Say( 1053034 ); // * The plague beast creates another beast from its flesh! *
			}

			base.OnDamagedBySpell( caster );
		}

		public override bool AutoDispel{ get{ return true; } }

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			if ( attacker != this && 0.25 > Utility.RandomDouble() )
			{
				BaseCreature spawn = new PlagueSpawn( this );

				spawn.Team = this.Team;
				spawn.MoveToWorld( this.Location, this.Map );
				spawn.Combatant = attacker;

				Say( 1053034 ); // * The plague beast creates another beast from its flesh! *
			}

			base.OnGotMeleeAttack( attacker );
		}

		public PlagueBeast( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGold( 350, 500 );
			PackGem();

			// TODO: jewelry, dungeon chest, healthy gland

			// Category 3 MID
			PackMagicItem( 1, 2, 0.10 );
			PackMagicItem( 1, 2, 0.05 );
		}

		public override int GetIdleSound()
		{
			return 0x1BF;
		}

		public override int GetAttackSound()
		{
			return 0x1C0;
		}

		public override int GetHurtSound()
		{
			return 0x1C1;
		}

		public override int GetDeathSound()
		{
			return 0x1C2;
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
