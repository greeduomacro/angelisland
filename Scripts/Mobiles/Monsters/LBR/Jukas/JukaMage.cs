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

/* Scripts/Mobiles/Monsters/LBR/Jukas/JukaMage.cs
 * ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *  8/16/06, Rhiannon
 *		Changed speed settings to match SpeedInfo table.
 *	7/26/05, erlein
 *		Automated removal of AoS resistance related function calls. 6 lines removed.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Misc;
using Server.Spells;

namespace Server.Mobiles
{
	[CorpseName( "a juka corpse" )] // Why is this 'juka' and warriors 'jukan' ? :-(
	public class JukaMage : BaseCreature
	{
		[Constructable]
		public JukaMage() : base( AIType.AI_Mage, FightMode.All | FightMode.Closest, 10, 1, 0.25, 0.5 )
		{
			Name = "a juka mage";
			Body = 765;

			SetStr( 201, 300 );
			SetDex( 71, 90 );
			SetInt( 451, 500 );

			SetHits( 121, 180 );

			SetDamage( 4, 10 );



			SetSkill( SkillName.Anatomy, 80.1, 90.0 );
			SetSkill( SkillName.EvalInt, 80.2, 100.0 );
			SetSkill( SkillName.Magery, 99.1, 100.0 );
			SetSkill( SkillName.Meditation, 80.2, 100.0 );
			SetSkill( SkillName.MagicResist, 140.1, 150.0 );
			SetSkill( SkillName.Tactics, 80.1, 90.0 );
			SetSkill( SkillName.Wrestling, 80.1, 90.0 );

			Fame = 15000;
			Karma = -15000;

			VirtualArmor = 16;

			m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 2, 5 ) );
		}

		public override int GetIdleSound()
		{
			return 0x1CD;
		}

		public override int GetAngerSound()
		{
			return 0x1CD;
		}

		public override int GetHurtSound()
		{
			return 0x1D0;
		}

		public override int GetDeathSound()
		{
			return 0x28D;
		}

		public override bool AlwaysMurderer{ get{ return true; } }
		public override bool CanRummageCorpses{ get{ return true; } }
		public override int Meat{ get{ return 1; } }

		private DateTime m_NextAbilityTime;

		public override void OnThink()
		{
			if ( DateTime.Now >= m_NextAbilityTime )
			{
				JukaLord toBuff = null;

				IPooledEnumerable eable = this.GetMobilesInRange( 8 );
				foreach ( Mobile m in eable)
				{
					if ( m is JukaLord && IsFriend( m ) && m.Combatant != null && CanBeBeneficial( m ) && m.CanBeginAction( typeof( JukaMage ) ) && InLOS( m ) )
					{
						toBuff = (JukaLord)m;
						break;
					}
				}
				eable.Free();

				if ( toBuff != null )
				{
					if ( CanBeBeneficial( toBuff ) && toBuff.BeginAction( typeof( JukaMage ) ) )
					{
						m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 30, 60 ) );

						toBuff.Say( true, "Give me the power to destroy my enemies!" );
						this.Say( true, "Fight well my lord!" );

						DoBeneficial( toBuff );

						object[] state = new object[]{ toBuff, toBuff.HitsMaxSeed, toBuff.RawStr, toBuff.RawDex };

						SpellHelper.Turn( this, toBuff );

						int toScale = toBuff.HitsMaxSeed;

						if ( toScale > 0 )
						{
							toBuff.HitsMaxSeed += AOS.Scale( toScale, 75 );
							toBuff.Hits += AOS.Scale( toScale, 75 );
						}

						toScale = toBuff.RawStr;

						if ( toScale > 0 )
							toBuff.RawStr += AOS.Scale( toScale, 50 );

						toScale = toBuff.RawDex;

						if ( toScale > 0 )
						{
							toBuff.RawDex += AOS.Scale( toScale, 50 );
							toBuff.Stam += AOS.Scale( toScale, 50 );
						}

						toBuff.Hits = toBuff.Hits;
						toBuff.Stam = toBuff.Stam;

						toBuff.FixedParticles( 0x375A, 10, 15, 5017, EffectLayer.Waist );
						toBuff.PlaySound( 0x1EE );

						Timer.DelayCall( TimeSpan.FromSeconds( 20.0 ), new TimerStateCallback( Unbuff ), state );
					}
				}
				else
				{
					m_NextAbilityTime = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 2, 5 ) );
				}
			}

			base.OnThink();
		}

		private void Unbuff( object state )
		{
			object[] states = (object[])state;

			JukaLord toDebuff = (JukaLord)states[0];

			toDebuff.EndAction( typeof( JukaMage ) );

			if ( toDebuff.Deleted )
				return;

			toDebuff.HitsMaxSeed = (int)states[1];
			toDebuff.RawStr = (int)states[2];
			toDebuff.RawDex = (int)states[3];

			toDebuff.Hits = toDebuff.Hits;
			toDebuff.Stam = toDebuff.Stam;
		}

		public JukaMage( Serial serial ) : base( serial )
		{
		}

		public override void GenerateLoot()
		{
			Container bag = new Bag();

			int count = Utility.RandomMinMax( 10, 20 );

			for ( int i = 0; i < count; ++i )
			{
				Item item = Loot.RandomReagent();

				if ( item == null )
					continue;

				if ( !bag.TryDropItem( this, item, false ) )
					item.Delete();
			}

			PackItem( bag );

			PackItem( new ArcaneGem() );

			PackGold( 125, 175 );
			PackGem();
			PackScroll( 3, 6 );
			PackScroll( 3, 6 );
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
