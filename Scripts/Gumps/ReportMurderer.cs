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

/* Gumps/ReportMurderer.cs
 * ChangeLog
 *  12/21/07, Pix
 *      Added no counting flag for guilds.
 *  10/6/07, Adam
 *      No murder counts during ServerWars
 *	2/20/07, Pix
 *		Fix a premature change that was made for townships
 *	3/18/07, Pix
 *		Added functionality to thwart 'scripting' of responses to the murder count gump, 
 *		so people can't auto-respond 'yes' while AFK.
 *  1/5/07, Rhiannon
 *      Added check so bounties cannot be placed on staff members.
 *	6/15/06, Pix
 *		Now pays attention to new AggressorInfo.InitialAggressionInNoCountZone property.
 *	4/6/06, Adam
 *		Remove if( CoreAI.TempInt == 1 ) test from Pix's fix below
 *	3/27/06, Pix
 *		Added code to fix multiple-counting problem (Conditionalized by CoreAI.TempInt == 1)
 * 01/10/05, Pix
 *		Replaced NextMurderCountTime with KillerTimes arraylist for controlling repeated counting.
 *	9/2/04, Pix
 *		Made it so inmates can't give counts.
 *	8/7/04, mith
 *		modified so that a player can only respond to one series of count gumps in a 2 minute span.
 *			this is a temporary fix until I can get notoriety flagging working properly.
 *	5/16/04, Pixie
 *		BountyGump now comes up after the reportmurderer gump.
 *  4/19/04, pixie
 *    Gump now closes after 10 minutes.
 *	4/18/04, pixie
 *    Gump doesn't report murder 10 minutes after being created.
 */

using System;
using System.Collections;
using Server;
using Server.Misc;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;

using Server.BountySystem;

namespace Server.Gumps
{
	public class ReportMurdererGump : Gump
	{
		private int m_iRandomNumberYesResponse = 1;

		private int m_Idx;
		private ArrayList m_Killers;
		private Mobile m_Victum;

		private DateTime m_MaxResponseTime;

		public static void Initialize()
		{
			EventSink.PlayerDeath += new PlayerDeathEventHandler( EventSink_PlayerDeath );
		}

		public class KillerTime
		{
			private Mobile m_Killer;
			private DateTime m_DateTime;

			public KillerTime(Mobile m, DateTime dt)
			{
				m_Killer = m;
				m_DateTime = dt;
			}

			public Mobile Killer{ get{ return m_Killer; } }
			public DateTime Time{ get{ return m_DateTime; } set{ m_DateTime = value; } }
		}
 
		public static void EventSink_PlayerDeath( PlayerDeathEventArgs e )
		{
			Mobile m = e.Mobile;

			//Check to make sure inmates don't give counts.
			if( m is PlayerMobile )
			{
				if( ((PlayerMobile)m).Inmate )
				{
					return;
				}
			}

            //Check to make sure we don't give counts during Server Wars.
            if (Server.Misc.AutoRestart.ServerWars == true)
                return;

			ArrayList killers = new ArrayList();
			ArrayList toGive = new ArrayList();

			//if ( DateTime.Now < ((PlayerMobile)m).NextMurderCountTime )
			//	return;

			bool bTimeRestricted = false; //false means they're out of time restriction

			foreach ( AggressorInfo ai in m.Aggressors )
			{
				bTimeRestricted = false;

				//Pix: 3/20/07 - fix a premature change that was made for townships
				//if ( ai.Attacker.Player && ai.CanReportMurder && !ai.Reported && !ai.InitialAggressionNotCountable )
				if (ai.Attacker.Player && ai.CanReportMurder && !ai.Reported && !ai.InitialAggressionInNoCountZone)
				{
					try //just for safety's sake
					{
						if( m is PlayerMobile )
						{
							PlayerMobile pm = (PlayerMobile)m;
							if( pm.KillerTimes == null )
							{
								pm.KillerTimes = new ArrayList();
							}

							bool bFound = false;
							KillerTime kt = null;
							foreach( KillerTime k in pm.KillerTimes )
							{
								if( k.Killer == ai.Attacker )
								{
									bFound = true;
									kt = k;
								}
							}

							if( bFound )
							{
								if( kt != null )
								{
									if( DateTime.Now - kt.Time < TimeSpan.FromMinutes(2.0) )
									{
										bTimeRestricted = true;
									}
									kt.Time = DateTime.Now;
								}
							}
							else
							{
								kt = new KillerTime( ai.Attacker, DateTime.Now );
								pm.KillerTimes.Add(kt);
							}
						}
					}
					catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

					if( !bTimeRestricted )
					{
						killers.Add( ai.Attacker );
					}
					ai.Reported = true;
				}

				if ( ai.Attacker.Player && (DateTime.Now - ai.LastCombatTime) < TimeSpan.FromSeconds( 30.0 ) && !toGive.Contains( ai.Attacker ) )
					toGive.Add( ai.Attacker );
			}

			foreach ( AggressorInfo ai in m.Aggressed )
			{
				if ( ai.Defender.Player && (DateTime.Now - ai.LastCombatTime) < TimeSpan.FromSeconds( 30.0 ) && !toGive.Contains( ai.Defender ) )
					toGive.Add( ai.Defender );
			}

			foreach ( Mobile g in toGive )
			{
				int n = Notoriety.Compute( g, m );

				int theirKarma = m.Karma, ourKarma = g.Karma;
				bool innocent = ( n == Notoriety.Innocent );
				bool criminal = ( n == Notoriety.Criminal || n == Notoriety.Murderer );

				int fameAward = m.Fame / 200;
				int karmaAward = 0;

				if ( innocent )
					karmaAward = ( ourKarma > -2500 ? -850 : -110 - (m.Karma / 100) );
				else if ( criminal )
					karmaAward = 50;

				Titles.AwardFame( g, fameAward, false );
				Titles.AwardKarma( g, karmaAward, true );
			}

			if ( m is PlayerMobile && ((PlayerMobile)m).NpcGuild == NpcGuild.ThievesGuild )
				return;

			if ( killers.Count > 0 )
				new GumpTimer( m, killers ).Start();

			//((PlayerMobile)m).NextMurderCountTime = DateTime.Now + TimeSpan.FromMinutes(2.0);
		}

		private class GumpTimer : Timer
		{
			private Mobile m_Victim;
			private ArrayList m_Killers;

			public GumpTimer( Mobile victim, ArrayList killers ) : base( TimeSpan.FromSeconds( 4.0 ) )
			{
				m_Victim = victim;
				m_Killers = killers;
			}

			protected override void OnTick()
			{
                try
                {
                    if (m_Victim.Guild != null)
                    {
                        Server.Guilds.Guild g = m_Victim.Guild as Server.Guilds.Guild;
                        if (g != null)
                        {
                            if (g.IsNoCountingGuild)
                            {
                                return;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Scripts.Commands.LogHelper.LogException(e);
                }

				m_Victim.SendGump( new ReportMurdererGump( m_Victim, m_Killers ) );
			}
		}

		private class MurderGumpTimeoutTimer : Timer
		{
			private Mobile m_Player;

			public MurderGumpTimeoutTimer( Mobile m ) : base( TimeSpan.FromMinutes( 10.0 ) )
			{
				m_Player = m;
			}

			protected override void OnTick()
			{
				m_Player.CloseGump( typeof(ReportMurdererGump) );
				Stop();
			}
		}

		public ReportMurdererGump( Mobile victum, ArrayList killers ) : this( victum, killers, 0 )
		{
		}

		private ReportMurdererGump( Mobile victum, ArrayList killers, int idx ) : base( 0, 0 )
		{
			m_Killers = killers;
			m_Victum = victum;
			m_Idx = idx;
			
			m_MaxResponseTime = DateTime.Now + TimeSpan.FromMinutes(10);

			BuildGump();

			new MurderGumpTimeoutTimer(m_Victum).Start();

		}

		private void BuildGump() 
		{
			AddBackground( 265, 205, 320, 290, 5054 );
			Closable = false;
			Resizable = false;
			
			AddPage( 0 );      			
			
			AddImageTiled( 225, 175, 50, 45, 0xCE );   //Top left corner
			AddImageTiled( 267, 175, 315, 44, 0xC9 );  //Top bar
			AddImageTiled( 582, 175, 43, 45, 0xCF );   //Top right corner
			AddImageTiled( 225, 219, 44, 270, 0xCA );  //Left side
			AddImageTiled( 582, 219, 44, 270, 0xCB );  //Right side
			AddImageTiled( 225, 489, 44, 43, 0xCC );   //Lower left corner
			AddImageTiled( 267, 489, 315, 43, 0xE9 );  //Lower Bar
			AddImageTiled( 582, 489, 43, 43, 0xCD );   //Lower right corner

			AddPage( 1 );
			
			AddHtml( 260, 234, 300, 140, ((Mobile)m_Killers[m_Idx]).Name, false, false ); // Player's Name
			AddHtmlLocalized( 260, 254, 300, 140, 1049066, false, false ); // Would you like to report...

			m_iRandomNumberYesResponse = Utility.Random(10, 1000);

			AddButton(260, 300, 0xFA5, 0xFA7, m_iRandomNumberYesResponse, GumpButtonType.Reply, 0);
			AddHtmlLocalized( 300, 300, 300, 50, 1046362, false, false ); // Yes
			      	
			AddButton( 360, 300, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 400, 300, 300, 50, 1046363, false, false ); // No      
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			//check if we're more than 10 minutes from the gump creation time
			//if we are, then do nothing.
			if( m_MaxResponseTime < DateTime.Now )
			{
				return;
			}

			int buttonID = info.ButtonID;

			if (m_iRandomNumberYesResponse == info.ButtonID)
			{
				buttonID = 1;
			}

			//switch ( info.ButtonID )
			switch( buttonID )
			{
				case 1: 
				{            
					Mobile killer = (Mobile)m_Killers[m_Idx];
					if ( killer != null && !killer.Deleted )
					{
						killer.Kills++;
						killer.ShortTermMurders++;

						if ( killer is PlayerMobile )
							((PlayerMobile)killer).ResetKillTime();

						from.RemoveAggressor(killer);

						killer.SendLocalizedMessage( 1049067 );//You have been reported for murder!

						if ( killer.Kills == 5 )
							killer.SendLocalizedMessage( 502134 );//You are now known as a murderer!
						else if ( SkillHandlers.Stealing.SuspendOnMurder && killer.Kills == 1 && killer is PlayerMobile && ((PlayerMobile)killer).NpcGuild == NpcGuild.ThievesGuild )
							killer.SendLocalizedMessage( 501562 ); // You have been suspended by the Thieves Guild.

                        if ( killer.AccessLevel == AccessLevel.Player ) // Can't put bounties on staff.
						    from.SendGump( new BountyGump(from, killer) );
					}
					break; 
				}
				case 2: 
				{
					break; 
				}
				default:
				{
					//got an unknown response - just quit.
					return;
				}
			}

			m_Idx++;
			if ( m_Idx < m_Killers.Count )
				from.SendGump( new ReportMurdererGump( from, m_Killers, m_Idx ) );
		}
	}
}

