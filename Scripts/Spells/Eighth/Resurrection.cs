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

/* Scripts/Spells/Eighth/Resurrection.cs
 * ChangeLog
 *	11/27/07, Adam
 *		Replace the simple 18 unit z order check with the new Items.Bandage.ProximityCheck(). 
 *		This function (amoung other things) checks that the PATH to the patient is not blocked.
 *		This test prevents various in-house resurect-in-a-locked/blocked-room exploits.
 *	3/17/05, Adam
 *		Add a 18 unit Z order check for healing.
 *		This fixes the "resurrect on patio" exploit.
 */

using System;
using Server.Targeting;
using Server.Network;
using Server.Gumps;

namespace Server.Spells.Eighth
{
	public class ResurrectionSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Resurrection", "An Corp",
				SpellCircle.Eighth,
				245,
				9062,
				Reagent.Bloodmoss,
				Reagent.Garlic,
				Reagent.Ginseng
			);

		public ResurrectionSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
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
			else if ( m == Caster )
			{
				Caster.SendLocalizedMessage( 501039 ); // Thou can not resurrect thyself.
			}
			else if ( !Caster.Alive )
			{
				Caster.SendLocalizedMessage( 501040 ); // The resurrecter must be alive.
			}
			else if ( m.Alive )
			{
				Caster.SendLocalizedMessage( 501041 ); // Target is not dead.
			}
			else if ( !Caster.InRange( m, 1 ) )
			{
				Caster.SendLocalizedMessage( 501043 ); // Target is not close enough.
			}
			else if ( !m.Player )
			{
				Caster.SendLocalizedMessage( 501043 ); // Target is not a being.
			}
            else if (m.Map == null || !m.Map.CanFit(m.Location, 16, CanFitFlags.requireSurface))
			{
				Caster.SendLocalizedMessage( 501042 ); // Target can not be resurrected at that location.
				m.SendLocalizedMessage( 502391 ); // Thou can not be resurrected there!
			}
			//else if ( Math.Abs(m.Z - Caster.Z) > 18 )
			else if (Items.Bandage.ProximityCheck(Caster, m, 1) == false)
			{
				Caster.SendLocalizedMessage( 501043 ); // Target is not close enough.
			}
			else if ( CheckBSequence( m, true ) )
			{
				SpellHelper.Turn( Caster, m );

				m.PlaySound( 0x214 );
				m.FixedEffect( 0x376A, 10, 16 );

				m.SendGump( new ResurrectGump( m, Caster ) );
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private ResurrectionSpell m_Owner;

			public InternalTarget( ResurrectionSpell owner ) : base( 1, false, TargetFlags.Beneficial )
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