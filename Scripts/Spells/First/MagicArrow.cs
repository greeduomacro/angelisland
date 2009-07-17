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

/* Scripts/Spells/Base/Spell.cs
	ChangeLog:
	6/03/06, Kit
		Added damage type define
	5/15/06, Kit
		Added Min/Max defines
	7/4/04, Pix
		Added caster to SpellHelper.Damage call so corpse wouldn't stay blue to this caster
	6/5/04, Pix
		Merged in 1.0RC0 code.
	4/01/04 changes by merxypoo:
		Changed DamageDelay to 0.65
	3/25/04 changes by smerX:
		Changed DamageDelay to 0.35
	3/18/04 code changes by smerX:
		Added 0.3 second DamageDelay
*/
using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.First
{
	public class MagicArrowSpell : Spell
	{
		public override int MinDamage{get{return 4;}}
		public override int MaxDamage{get{return 4;}}
		public override SpellDamageType DamageType
		{
			get
			{
				return SpellDamageType.Energy;
			}
		}

		private static SpellInfo m_Info = new SpellInfo(
				"Magic Arrow", "In Por Ylem",
				SpellCircle.First,
				212,
				9041,
				Reagent.SulfurousAsh
			);

		public MagicArrowSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
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
			else if ( CheckHSequence( m ) )
			{
				Mobile source = Caster;

				SpellHelper.Turn( source, m );

				SpellHelper.CheckReflect( (int)this.Circle, ref source, ref m );

				double damage;
				
				if ( Core.AOS )
				{
					damage = GetNewAosDamage( 10, 1, 4 );
				}
				else
				{
					damage = Utility.Random( MinDamage, MaxDamage );

					if ( CheckResisted( m ) )
					{
						damage *= 0.75;

						m.SendLocalizedMessage( 501783 ); // You feel yourself resisting magical energy.
					}

					damage *= GetDamageScalar( m );
				}

				source.MovingParticles( m, 0x36E4, 5, 0, false, true, 3006, 4006, 0 );
				source.PlaySound( 0x1E5 );

				SpellHelper.Damage( TimeSpan.FromSeconds( 0.65 ), m, Caster, damage, 0, 100, 0, 0, 0 );
				// TimeSpan.FromSeconds ( 0.3 ) on above line is the DamageDelay for this spell
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private MagicArrowSpell m_Owner;

			public InternalTarget( MagicArrowSpell owner ) : base( 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
				{
					m_Owner.Target( (Mobile)o );
				}
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}