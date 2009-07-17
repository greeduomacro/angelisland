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
 /* ChangeLog
  *	4/27/05 smerX
  *		Low level debuffs always interrupt targets' spells, circa OSI15
  */


using System;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;

namespace Server.Spells.First
{
	public class FeeblemindSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Feeblemind", "Rel Wis",
				SpellCircle.First,
				212,
				9031,
				Reagent.Ginseng,
				Reagent.Nightshade
			);

		public FeeblemindSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckHSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );
				
				if ( m.Spell != null )
					m.Spell.OnCasterHurt();

				SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m );

				if ( Caster is PlayerMobile )
				{					
					if ( SpellHelper.AddStatCurse( Caster, m, StatType.Int ) )
					{
						m.FixedParticles( 0x3779, 10, 15, 5009, EffectLayer.Waist );
						m.PlaySound( 0x1E6 );
					}
					else
					{
						Caster.LocalOverheadMessage( MessageType.Regular, 0x3B2, 502632 ); // The spell fizzles.
						Caster.FixedEffect( 0x3735, 6, 30 );
						Caster.PlaySound( 0x5C );
					}
				}

				m.Paralyzed = false;
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private FeeblemindSpell m_Owner;

			public InternalTarget( FeeblemindSpell owner ) : base( 12, false, TargetFlags.Harmful )
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
