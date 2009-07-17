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

/* Spells/Fourth/ArchCure.cs
 * CHANGELOG:
 *  11/07/05, Kit
 *		Restored former cure rates.
 *	10/16/05, Pix
 *		Change to chance to cure.
 *  6/4/04, Pixie
 *		Changed to Greater Cure type spell (no more area effect)
 *		with greater chance to cure than Cure.
 *		Added debugging for cure chance for people > playerlevel
 *	5/25/04, Pixie
 *		Changed formula for success curing poison
 *	5/22/04, Pixie
 *		Made it so chance to cure poison was based on the caster's magery vs the level of poison
 */


using System;
using System.Collections;
using Server.Network;
using Server.Items;
using Server.Targeting;

namespace Server.Spells.Fourth
{
	public class ArchCureSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Greater Cure", "Vas An Nox",
				SpellCircle.Fourth,
				215,
				9061,
				Reagent.Garlic,
				Reagent.Ginseng,
				Reagent.MandrakeRoot
			);

		public ArchCureSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
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
			else if ( CheckBSequence( m ) )
			{
				SpellHelper.Turn( Caster, m );
				//chance to cure poison is ((caster's magery/poison level) - 20%)

				double chance = 100;
				try //I threw this try-catch block in here because Poison is whacky... there'll be a tiny 
				{   //race condition if multiple people are casting cure on the same target... 
					if( m.Poison != null )
					{
						//desired is: LP: 50%, DP: 90% GP-: 100%
						double multiplier = 0.5 + 0.4 * (4 - m.Poison.Level);
						chance = Caster.Skills[SkillName.Magery].Value * multiplier;
					}

					if( Caster.AccessLevel > AccessLevel.Player )
					{
						Caster.SendMessage("Chance to cure is " + chance + "%");
					}
				}
				catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

				/*
				//new cure rates
				int chance = 100;
				Poison p = m.Poison;
				try
				{
					if( p != null )
					{
						chance = 10000 
							+ (int)(Caster.Skills[SkillName.Magery].Value * 75) 
							- ((p.Level + 1) * 1750);
						chance /= 100;
						if( p.Level > 3 ) //lethal poison further penalty
						{
							chance -= 35; //@ GM magery, chance will be 52%
						}
					}

					if( Caster.AccessLevel > AccessLevel.Player )
					{
						Caster.SendMessage("Chance to cure is " + chance + "%");
					}
				}
				catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
				*/

				if( Utility.Random( 0, 100 ) <= chance )
				{
					if ( m.CurePoison( Caster ) )
					{
						if ( Caster != m )
							Caster.SendLocalizedMessage( 1010058 ); // You have cured the target of all poisons!

						m.SendLocalizedMessage( 1010059 ); // You have been cured of all poisons.
					}
				}
				else
				{
					Caster.SendLocalizedMessage( 1010060 ); // You have failed to cure your target!
				}

				m.FixedParticles( 0x373A, 10, 15, 5012, EffectLayer.Waist );
				m.PlaySound( 0x1E0 );
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private ArchCureSpell m_Owner;

			public InternalTarget( ArchCureSpell owner ) : base( 12, false, TargetFlags.Beneficial )
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