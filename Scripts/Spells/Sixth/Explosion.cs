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
 /*	/Scripts/Spells/Sixth/Explosion.cs
  *  ChangeLog:
  * 6/03/06, Kit
  *		Added damage type define
  * 5/15/06, Kit
  *		Added Min/Max damage defines
  * 3/28/06, weaver
  *		Moved CanBeHarmful() call / check so that flagging occurs on targetting
  *		rather than when the spell timer ticks over.
  * 6/5/04, Pix
  *		Merged in 1.0RC0 code.
  * 4/26/04 smerx
  * 	Slightly lowered base damage
  */
		

using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Sixth
{
	public class ExplosionSpell : Spell
	{
		public override int MinDamage{get{return 18;}}
		public override int MaxDamage{get{return 19;}}
		public override SpellDamageType DamageType
		{
			get
			{
				return SpellDamageType.Fire;
			}
		}

		private static SpellInfo m_Info = new SpellInfo(
				"Explosion", "Vas Ort Flam",
				SpellCircle.Sixth,
				230,
				9041,
				Reagent.Bloodmoss,
				Reagent.MandrakeRoot
			);

		public ExplosionSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public override bool DelayedDamage{ get{ return false; } }

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( Caster.CanBeHarmful( m ) && CheckSequence() )
			{
				Mobile attacker = Caster, defender = m;

				SpellHelper.Turn( Caster, m );
				SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m );

				// wea: moved harmful check so that flagging occurs on target...
                // timer is now only created if harmful action is possible

				if ( attacker.HarmfulCheck( defender ) )
				{
					InternalTimer t = new InternalTimer( this, attacker, defender, m, MinDamage, MaxDamage );
					t.Start();
				}
			}

			FinishSequence();
		}

		private class InternalTimer : Timer
		{
			private Spell m_Spell;
			private Mobile m_Target;
			private Mobile m_Attacker, m_Defender;
			private int Min, Max;

			public InternalTimer( Spell spell, Mobile attacker, Mobile defender, Mobile target , int MinDamage, int MaxDamage) : base( TimeSpan.FromSeconds( Core.AOS ? 3.0 : 2.5 ) )
			{
				m_Spell = spell;
				m_Attacker = attacker;
				m_Defender = defender;
				m_Target = target;
				Min = MinDamage;
				Max = MaxDamage;

				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				double damage;

				if ( Core.AOS )
				{
					damage = m_Spell.GetNewAosDamage( 38, 1, 5 );
				}
				else
				{
					damage = Utility.Random( Min, Max );

					if ( m_Spell.CheckResisted( m_Target ) )
					{
						damage *= 0.75;

						m_Target.SendLocalizedMessage( 501783 ); // You feel yourself resisting magical energy.
					}

					damage *= m_Spell.GetDamageScalar( m_Target );
				}

				m_Target.FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.Head );
				m_Target.PlaySound( 0x307 );

				SpellHelper.Damage( m_Spell, m_Target, damage, 0, 100, 0, 0, 0 );
			}
		}

		private class InternalTarget : Target
		{
			private ExplosionSpell m_Owner;

			public InternalTarget( ExplosionSpell owner ) : base( 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}