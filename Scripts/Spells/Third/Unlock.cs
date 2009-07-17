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

/* Change Log
 *
 * 1/11/05, Darva
 *		Made magic unlock work on all magic locked chests.
 */
using System;
using Server.Targeting;
using Server.Network;
using Server.Items;

namespace Server.Spells.Third
{
	public class UnlockSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Unlock Spell", "Ex Por",
				SpellCircle.Third,
				215,
				9001,
				Reagent.Bloodmoss,
				Reagent.SulfurousAsh
			);

		public UnlockSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( LockableContainer targ )
		{
			if ( Multis.BaseHouse.CheckSecured( targ ) )
			{
				// You cannot cast this on a secure item.
				Caster.SendLocalizedMessage( 503098 );
			}
			else if ( CheckSequence() )
			{
				SpellHelper.Turn( Caster, targ );

				Point3D loc = targ.GetWorldLocation();

				Effects.SendLocationParticles(
					EffectItem.Create( loc, targ.Map, EffectItem.DefaultDuration ),
					0x376A, 9, 32, 5024 );

				Effects.PlaySound( loc, targ.Map, 0x1FF );

				if ( targ.Locked && targ.LockLevel != 0 )
				{
					double magery = Caster.Skills[SkillName.Magery].Value;
					int level = (int)(magery * 0.8) - 4;

					if ( targ.LockLevel == -255 )
					{
						targ.Locked = false;
						FinishSequence();
						return;
					}
					if ( level >= targ.RequiredSkill && !(targ is TreasureMapChest && ((TreasureMapChest)targ).Level > 2) )
					{
						targ.Locked = false;

						

						if ( targ.LockLevel == 0 )
							targ.LockLevel = -1;
					}
					else
					{
						// My spell does not seem to have an effect on that lock.
						Caster.LocalOverheadMessage( MessageType.Regular, 0x3B2, 503099 );
					}
				}
				else
				{
					// That did not need to be unlocked.
					Caster.LocalOverheadMessage( MessageType.Regular, 0x3B2, 503101 );
				}
			}

			FinishSequence();
		}

		private class InternalTarget : Target
		{
			private UnlockSpell m_Owner;

			public InternalTarget( UnlockSpell owner ) : base( 12, false, TargetFlags.None )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is LockableContainer )
					m_Owner.Target( (LockableContainer)o );
				else
					from.SendLocalizedMessage( 501666 ); // You can't unlock that!

				// TODO: Really we can cast on anything, even mobiles, but it will effect and say 'That did not need to be unlocked'
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
} 
