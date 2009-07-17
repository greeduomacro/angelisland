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
 *	7/26/05, Adam
 *		Massive AOS cleanout
 *	11/10/04, Pix
 *		Added call to DoHarmful so that discordance is an aggressive action.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Server.SkillHandlers
{
	public class Discordance
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Discordance].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.RevealingAction();

			BaseInstrument.PickInstrument( m, new InstrumentPickedCallback( OnPickedInstrument ) );

			return TimeSpan.FromSeconds( 1.0 ); // Cannot use another skill for 1 second
		}

		public static void OnPickedInstrument( Mobile from, BaseInstrument instrument )
		{
			from.RevealingAction();
			from.SendLocalizedMessage( 1049541 ); // Choose the target for your song of discordance.
			from.Target = new DiscordanceTarget( from, instrument );
		}

		private class DiscordanceInfo
		{
			public Mobile m_From;
			public Mobile m_Creature;
			public DateTime m_EndTime;
			public Timer m_Timer;
			public double m_Scalar;
			public ArrayList m_Mods;

			public DiscordanceInfo( Mobile from, Mobile creature, TimeSpan duration, double scalar, ArrayList mods )
			{
				m_From = from;
				m_Creature = creature;
				m_EndTime = DateTime.Now + duration;
				m_Scalar = scalar;
				m_Mods = mods;

				Apply();
			}

			public void Apply()
			{
				for ( int i = 0; i < m_Mods.Count; ++i )
				{
					object mod = m_Mods[i];

					/*if ( mod is ResistanceMod )
						m_Creature.AddResistanceMod( (ResistanceMod) mod );
					else*/ if ( mod is StatMod )
						m_Creature.AddStatMod( (StatMod) mod );
					else if ( mod is SkillMod )
						m_Creature.AddSkillMod( (SkillMod) mod );
				}
			}

			public void Clear()
			{
				for ( int i = 0; i < m_Mods.Count; ++i )
				{
					object mod = m_Mods[i];

					/*if ( mod is ResistanceMod )
						m_Creature.RemoveResistanceMod( (ResistanceMod) mod );
					else*/ if ( mod is StatMod )
						m_Creature.RemoveStatMod( ((StatMod) mod).Name );
					else if ( mod is SkillMod )
						m_Creature.RemoveSkillMod( (SkillMod) mod );
				}
			}
		}

		private static Hashtable m_Table = new Hashtable();

		public static bool GetScalar( Mobile targ, ref double scalar )
		{
			DiscordanceInfo info = m_Table[targ] as DiscordanceInfo;

			if ( info == null )
				return false;

			scalar = info.m_Scalar;
			return true;
		}

		private static void ProcessDiscordance( object state )
		{
			DiscordanceInfo info = (DiscordanceInfo)state;
			Mobile from = info.m_From;
			Mobile targ = info.m_Creature;

			if ( DateTime.Now >= info.m_EndTime || targ.Deleted || from.Map != targ.Map || targ.GetDistanceToSqrt( from ) > 16 )
			{
				if ( info.m_Timer != null )
					info.m_Timer.Stop();

				info.Clear();
				m_Table.Remove( targ );
			}
			else
			{
				targ.FixedEffect( 0x376A, 1, 32 );
			}
		}

		public class DiscordanceTarget : Target
		{
			private BaseInstrument m_Instrument;

			public DiscordanceTarget( Mobile from, BaseInstrument inst ) : base( BaseInstrument.GetBardRange( from, SkillName.Discordance ), false, TargetFlags.Harmful )
			{
				m_Instrument = inst;
			}

			protected override void OnTarget( Mobile from, object target )
			{
				from.RevealingAction();

				if ( target is Mobile )
				{
					Mobile targ = (Mobile)target;

					//Pixie - 11/10/04 - added this so discordance is an aggressive action.
					from.DoHarmful( targ );

					if ( targ is BaseCreature && ((BaseCreature)targ).BardImmune )
					{
						from.SendLocalizedMessage( 1049535 ); // A song of discord would have no effect on that.
					}
					else if ( !targ.Player )
					{
						TimeSpan len = TimeSpan.FromSeconds( from.Skills[SkillName.Discordance].Value * 2 );
						double diff = m_Instrument.GetDifficultyFor( targ ) - 10.0;
						double music = from.Skills[SkillName.Musicianship].Value;

						if ( music > 100.0 )
							diff -= (music - 100.0) * 0.5;

						if ( !BaseInstrument.CheckMusicianship( from ) )
						{
							from.SendLocalizedMessage( 500612 ); // You play poorly, and there is no effect.
							m_Instrument.PlayInstrumentBadly( from );
							m_Instrument.ConsumeUse( from );
						}
						else if ( from.CheckTargetSkill( SkillName.Discordance, target, diff-25.0, diff+25.0 ) )
						{
							if ( !m_Table.Contains( targ ) )
							{
								from.SendLocalizedMessage( 1049539 ); // You play the song surpressing your targets strength
								m_Instrument.PlayInstrumentWell( from );
								m_Instrument.ConsumeUse( from );

								ArrayList mods = new ArrayList();
								double scalar;

								if ( Core.AOS )
								{
									double discord = from.Skills[SkillName.Discordance].Value;
									int effect;

									if ( discord > 100.0 )
										effect = -20 + (int)((discord - 100.0) / -2.5);
									else
										effect = (int)(discord / -5.0);

									scalar = effect * 0.01;

									mods.Add( new ResistanceMod( ResistanceType.Physical, effect ) );
									mods.Add( new ResistanceMod( ResistanceType.Fire, effect ) );
									mods.Add( new ResistanceMod( ResistanceType.Cold, effect ) );
									mods.Add( new ResistanceMod( ResistanceType.Poison, effect ) );
									mods.Add( new ResistanceMod( ResistanceType.Energy, effect ) );

									for ( int i = 0; i < targ.Skills.Length; ++i )
									{
										if ( targ.Skills[i].Value > 0 )
											mods.Add( new DefaultSkillMod( (SkillName)i, true, targ.Skills[i].Value * scalar ) );
									}
								}
								else
								{
									scalar = ( from.Skills[SkillName.Discordance].Value / -5.0 ) / 100.0;

									mods.Add( new StatMod( StatType.Str, "DiscordanceStr", (int)(targ.RawStr * scalar), TimeSpan.Zero ) );
									mods.Add( new StatMod( StatType.Int, "DiscordanceInt", (int)(targ.RawInt * scalar), TimeSpan.Zero ) );
									mods.Add( new StatMod( StatType.Dex, "DiscordanceDex", (int)(targ.RawDex * scalar), TimeSpan.Zero ) );

									for ( int i = 0; i < targ.Skills.Length; ++i )
									{
										if ( targ.Skills[i].Value > 0 )
											mods.Add( new DefaultSkillMod( (SkillName)i, true, targ.Skills[i].Value * scalar ) );
									}
								}

								DiscordanceInfo info = new DiscordanceInfo( from, targ, len, scalar, mods );
								info.m_Timer = Timer.DelayCall( TimeSpan.Zero, TimeSpan.FromSeconds( 1.25 ), new TimerStateCallback( ProcessDiscordance ), info );

								m_Table[targ] = info;
							}
							else
							{
								from.SendLocalizedMessage( 1049537 );// Your target is already in discord.
							}
						}
						else
						{
							from.SendLocalizedMessage( 1049540 );// You fail to disrupt your target
							m_Instrument.PlayInstrumentBadly( from );
							m_Instrument.ConsumeUse( from );
						}
					}
					else
					{
						m_Instrument.PlayInstrumentBadly( from );
					}
				}
			}
		}
	}
}