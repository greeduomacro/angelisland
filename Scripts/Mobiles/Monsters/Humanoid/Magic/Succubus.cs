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

/* Scripts/Mobiles/Monsters/Humanoid/Magic/Succubus.cs
 * ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *  03/31/07. plasma
 *      Added LOS check to life drain attack
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 7 lines removed.
 *	7/6/04, Adam
 *		1. implement Jade's new Category Based Drop requirements
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a succubus corpse" )]
	public class Succubus : BaseCreature
	{
		[Constructable]
		public Succubus () : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a succubus";
            Female = true;
			Body = 149;
			BaseSoundID = 0x4B0;

			SetStr( 488, 620 );
			SetDex( 121, 170 );
			SetInt( 498, 657 );

			SetHits( 312, 353 );

			SetDamage( 18, 28 );



			SetSkill( SkillName.EvalInt, 90.1, 100.0 );
			SetSkill( SkillName.Magery, 99.1, 100.0 );
			SetSkill( SkillName.Meditation, 90.1, 100.0 );
			SetSkill( SkillName.MagicResist, 100.5, 150.0 );
			SetSkill( SkillName.Tactics, 80.1, 90.0 );
			SetSkill( SkillName.Wrestling, 80.1, 90.0 );

			Fame = 24000;
			Karma = -24000;

			VirtualArmor = 80;
		}

		public override int Meat{ get{ return 1; } }
		public override int TreasureMapLevel{ get{ return 4; } }
		
		public override 	AuraType 	MyAura{ get{ return AuraType.Hate; } }
		public override 	int 		AuraRange{ get{ return 5; } }
		public override 	int 		AuraMin{ get{ return 5; } }
		public override 	int 		AuraMax{ get{ return 10; } }
		public override	TimeSpan	NextAuraDelay{ get{ return TimeSpan.FromSeconds( 4.0 ); } }

		public void DrainLife()
		{
			ArrayList list = new ArrayList();

			IPooledEnumerable eable = this.GetMobilesInRange( 2 );
			foreach ( Mobile m in eable)
			{
                // pla: Added LOS check!
				if ( m == this || !CanBeHarmful( m ) || !InLOS(m) )
					continue;

				if ( m is BaseCreature && (((BaseCreature)m).Controlled || ((BaseCreature)m).Summoned || ((BaseCreature)m).Team != this.Team) )
					list.Add( m );
				else if ( m.Player )
					list.Add( m );
			}
			eable.Free();

			foreach ( Mobile m in list )
			{
				DoHarmful( m );

				m.FixedParticles( 0x374A, 10, 15, 5013, 0x496, 0, EffectLayer.Waist );
				m.PlaySound( 0x231 );

				m.SendMessage( "You feel the life drain out of you!" );

				int toDrain = Utility.RandomMinMax( 10, 40 );

				Hits += toDrain;
				m.Damage( toDrain, this );
			}
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );

			if ( 0.1 >= Utility.RandomDouble() )
				DrainLife();
		}

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );

			if ( 0.1 >= Utility.RandomDouble() )
				DrainLife();
		}

		public Succubus( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			PackGold( 450, 700 );
			PackScroll( 1, 7 );
			PackScroll( 1, 7 );
			PackMagicEquipment( 1, 3, 0.30, 0.30 );
			PackMagicEquipment( 1, 3, 0.05, 0.30 );
			// Category 4 MID
			PackMagicItem( 2, 3, 0.10 );
			PackMagicItem( 2, 3, 0.05 );
			PackMagicItem( 2, 3, 0.02 );
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
