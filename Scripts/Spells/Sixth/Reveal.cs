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
  * 5/30/05, Kit
		Made internal target class public so AI could access it
  */
using System;
using System.Collections;
using Server.Misc;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Sixth
{
	public class RevealSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Reveal", "Wis Quas",
				SpellCircle.Sixth,
				206,
				9002,
				Reagent.Bloodmoss,
				Reagent.SulfurousAsh
			);

		public RevealSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( IPoint3D p )
		{
			if ( !Caster.CanSee( p ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckSequence() )
			{
				SpellHelper.Turn( Caster, p );

				SpellHelper.GetSurfaceTop( ref p );

				ArrayList targets = new ArrayList();

				Map map = Caster.Map;

				if ( map != null )
				{
					IPooledEnumerable eable = map.GetMobilesInRange( new Point3D( p ), 1 + (int)(Caster.Skills[SkillName.Magery].Value / 20.0) );

					foreach ( Mobile m in eable )
					{
						if ( m.Hidden && (m.AccessLevel == AccessLevel.Player || Caster.AccessLevel > m.AccessLevel) && Caster.CanBeHarmful( m, false, true ) )
							targets.Add( m );
					}

					eable.Free();
				}

				for ( int i = 0; i < targets.Count; ++i )
				{
					Mobile m = (Mobile)targets[i];

					m.RevealingAction();

					m.FixedParticles( 0x375A, 9, 20, 5049, EffectLayer.Head );
					m.PlaySound( 0x1FD );
				}
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private RevealSpell m_Owner;

			public InternalTarget( RevealSpell owner ) : base( 12, true, TargetFlags.None )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				IPoint3D p = o as IPoint3D;

				if ( p != null )
					m_Owner.Target( p );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}