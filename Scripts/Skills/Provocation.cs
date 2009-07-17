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

/*
 * Scripts/Skills/Provocation.cs
 * ChangeLog
 *  2/1/08, Pix
 *      Protected RTT test from ever being called, even on TC, but left the code.
 *	8/30/07, Adam
 *		Revert change below and move the logic to m_Instrument.GetDifficultyFor(creature)
 *	8/28/07, Adam
 *		Fail provo on Paragon creatures (without a specific message)
 *	8/26/07, Pix.
 *		Added RTT to provocation.
 *	09/02/06, weaver
 *		Fixed IOB alignment check to not prevent provocation onto players if the player and creature
 *		are not aligned with an IOB!
 *	8/22/06, Pix
 *		Removed IOBEquipped check.
 *	01/04/05 - Pix
 *		Now you can't provoke a mob of the same IOBAlignment onto a player who is wearing that
 *		IOB.
 *	11/10/04 - Pix
 *		Made provoking two creatures an aggressive action (uncommented the DoHarmful calls)
 *		so that IOBs wouldn't allow risk-free farming.
 *  7/10/04, Old Salty
 *  	Added appropriate 10 second skill delay if the provocation target is a player character.
 *  6/12/04, Old Salty
 * 		You can no longer provoke orcs onto players wearing orcish kin masks
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 * 4/7/04 changes by Pixie
 *  Added ability to target onto a player.
 */



using System;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Misc; 

namespace Server.SkillHandlers
{
	public class Provocation
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Provocation].Callback = new SkillUseCallback( OnUse );
		}

        private static bool bUseRTT = false;

		public static TimeSpan OnUse( Mobile m )
		{
            PlayerMobile pm = m as PlayerMobile;

            if (bUseRTT && TestCenter.Enabled && pm != null)
			{
				pm.RTT("Please verify that you're at your computer.  Use of this skill will be disabled if too many failures occur.", false, 2, "Provocation");
			}

            if (bUseRTT && TestCenter.Enabled && pm != null && pm.RTTFailures >= 3)
			{
				pm.SendMessage("You cannot use this skill because you have failed the AFK check too many times in a row.");
				pm.SendMessage("After some time has elapsed, you will be tested again when you use the skill.");
				return TimeSpan.FromSeconds(5.0);
            }
            else
            {
                m.RevealingAction();

                BaseInstrument.PickInstrument(m, new InstrumentPickedCallback(OnPickedInstrument));

                return TimeSpan.FromSeconds(1.0); // Cannot use another skill for 1 second
            }
		}

		public static void OnPickedInstrument( Mobile from, BaseInstrument instrument )
		{
			from.RevealingAction();
			from.SendLocalizedMessage( 501587 ); // Whom do you wish to incite?
			from.Target = new InternalFirstTarget( from, instrument );
		}

		private class InternalFirstTarget : Target
		{
			private BaseInstrument m_Instrument;

			public InternalFirstTarget( Mobile from, BaseInstrument instrument ) : base( BaseInstrument.GetBardRange( from, SkillName.Provocation ), false, TargetFlags.None )
			{
				m_Instrument = instrument;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				from.RevealingAction();

				if ( targeted is BaseCreature && from.CanBeHarmful( (Mobile)targeted, true ) )
				{
					BaseCreature creature = (BaseCreature)targeted;

					if ( creature.Controlled )
					{
						from.SendLocalizedMessage( 501590 ); // They are too loyal to their master to be provoked.
					}
					else
					{
						from.RevealingAction();
						m_Instrument.PlayInstrumentWell( from );
						from.SendLocalizedMessage( 1008085 ); // You play your music and your target becomes angered.  Whom do you wish them to attack?
						from.Target = new InternalSecondTarget( from, m_Instrument, creature );
					}
				}
			}
		}

		private class InternalSecondTarget : Target
		{
			private BaseCreature m_Creature;
			private BaseInstrument m_Instrument;

			public InternalSecondTarget( Mobile from, BaseInstrument instrument, BaseCreature creature ) : base( BaseInstrument.GetBardRange( from, SkillName.Provocation ), false, TargetFlags.None )
			{
				m_Instrument = instrument;
				m_Creature = creature;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				from.RevealingAction();

				if ( targeted is BaseCreature )
				{
					BaseCreature creature = (BaseCreature)targeted;

					if ( m_Creature.Unprovokable || creature.Unprovokable )
					{
						from.SendLocalizedMessage( 1049446 ); // You have no chance of provoking those creatures.
					}
					else if ( m_Creature.Map != creature.Map || !m_Creature.InRange( creature, BaseInstrument.GetBardRange( from, SkillName.Provocation ) ) )
					{
						from.SendLocalizedMessage( 1049450 ); // The creatures you are trying to provoke are too far away from each other for your music to have an effect.
						from.Target = new InternalFirstTarget( from, m_Instrument );
					}
					else if ( m_Creature != creature )
					{
						from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 10.0 );

						double diff = ((m_Instrument.GetDifficultyFor( m_Creature ) + m_Instrument.GetDifficultyFor( creature )) * 0.5) - 5.0;
						double music = from.Skills[SkillName.Musicianship].Value;

						if ( music > 100.0 )
							diff -= (music - 100.0) * 0.5;

						if ( from.CanBeHarmful( m_Creature, true ) && from.CanBeHarmful( creature, true ) )
						{
							if ( !BaseInstrument.CheckMusicianship( from ) )
							{
								from.SendLocalizedMessage( 500612 ); // You play poorly, and there is no effect.
								m_Instrument.PlayInstrumentBadly( from );
								m_Instrument.ConsumeUse( from );
							}
							else
							{
								//Pix: 11/10/2004 - the following two lines (DoHarmfuls) were commented out
								//in the RunUO1.0RC0 release... I don't know why.
								// They need to be here, however, because provocation *is* an aggressive action
								// AND because NOT having them here let's people use IOBs and provoke without risk.
								from.DoHarmful( m_Creature );
								from.DoHarmful( creature );

								if ( !from.CheckTargetSkill( SkillName.Provocation, creature, diff-25.0, diff+25.0 ) )
								{
									from.SendLocalizedMessage( 501599 ); // Your music fails to incite enough anger.
									m_Instrument.PlayInstrumentBadly( from );
									m_Instrument.ConsumeUse( from );
								}
								else
								{
									from.SendLocalizedMessage(501602); // Your music succeeds, as you start a fight.
									m_Instrument.PlayInstrumentWell(from);
									m_Instrument.ConsumeUse(from);
									m_Creature.Provoke(from, creature, true);
								}
							}
						}
					}
					else
					{
						from.SendLocalizedMessage( 501593 ); // You can't tell someone to attack themselves!
					}
				}
				//SMD 4/7/2004: allow for targetting a player
				else if( targeted is PlayerMobile )
				{
					PlayerMobile player = (PlayerMobile)targeted;

					if ( m_Creature.Unprovokable )
					{
						from.SendLocalizedMessage( 1049446 ); // You have no chance of provoking those creatures.
					}
					else if ( m_Creature.Map != player.Map || 
						!m_Creature.InRange( player, BaseInstrument.GetBardRange( from, SkillName.Provocation ) ) )
					{
						from.SendLocalizedMessage( 1049450 ); // The creatures you are trying to provoke are too far away from each other for your music to have an effect.
						from.Target = new InternalFirstTarget( from, m_Instrument );
					}
					// wea: added check to ensure creature & player IOB alignment are not IOBAlignment.None
					else if( m_Creature.IOBAlignment == player.IOBAlignment && player.IOBAlignment != IOBAlignment.None ) //&& player.IOBEquipped )
					{
						from.SendMessage("That creature refuses to attack its ally.");
						m_Instrument.ConsumeUse(from);
					}
					else //valid pair
					{
						from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 10.0 );      //Appropriate skill delay added by Old Salty
						
						double diff = ((m_Instrument.GetDifficultyFor( m_Creature ) + m_Instrument.GetDifficultyFor( player )) * 0.5) - 5.0;
						double music = from.Skills[SkillName.Musicianship].Value;

						if ( music > 100.0 )
							diff -= (music - 100.0) * 0.5;

						if ( from.CanBeHarmful( m_Creature, true ) && from.CanBeHarmful( player, true ) )
						{
							if ( !BaseInstrument.CheckMusicianship( from ) )
							{
								from.SendLocalizedMessage( 500612 ); // You play poorly, and there is no effect.
								m_Instrument.PlayInstrumentBadly( from );
								m_Instrument.ConsumeUse( from );
							}
							else
							{
								from.DoHarmful( m_Creature );
								from.DoHarmful( player );

								if ( !from.CheckTargetSkill( SkillName.Provocation, player, diff-25.0, diff+25.0 ) )
								{
									from.SendLocalizedMessage( 501599 ); // Your music fails to incite enough anger.
									m_Instrument.PlayInstrumentBadly( from );
									m_Instrument.ConsumeUse( from );
								}
								else
								{
									from.SendLocalizedMessage( 501602 ); // Your music succeeds, as you start a fight.
									m_Instrument.PlayInstrumentWell( from );
									m_Instrument.ConsumeUse( from );
									m_Creature.Provoke( from, player, true );
								}
							}
						}
					}
				}//end of else (target is player)
			}//end of OnTarget()
		}
	}
}
