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
 * Scripts/Skills/Tracking.cs
 *	ChangeLog
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loops updated.
 *	9/19/04, Adam
 *		Return tracking to it's original distance.
 *	9/11/04, Pix
 *		Doubled tracking range.
 *	7/21/04, Pixie
 *		Changed tracking to be tracking vs stealth.
 *	7/11/04, Pixie
 *		Fixed my poor arithmetic ...
 *	7/7/04, Pixie
 *		Changed to use the Value of Tracking/Hiding/Stealth instead of the Base
 *	7/4/04, Pixie
 *		Changed the chance to track vs hiding/stealth/visible
 *	4/7/04, changes by mith
 *		TrackWhoGump.DisplayTo() code modified to include target's stealth.
 *	3/16/04, changes by mith 
 *		TrackWhoGump.DisplayTo() code to compare tracking vs. target's hiding.
 *	last modified 3/16/04 by mith.
*/

using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.SkillHandlers
{
	public class Tracking
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Tracking].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.SendLocalizedMessage( 1011350 ); // What do you wish to track?

			m.CloseGump( typeof( TrackWhatGump ) );
			m.CloseGump( typeof( TrackWhoGump ) );
			m.SendGump( new TrackWhatGump( m ) );

			return TimeSpan.FromSeconds( 10.0 ); // 10 second delay before beign able to re-use a skill
		}
	}

	public class TrackWhatGump : Gump
	{
		private Mobile m_From;
		private bool m_Success;

		public TrackWhatGump( Mobile from ) : base( 20, 30 )
		{
			m_From = from;
			m_Success = from.CheckSkill( SkillName.Tracking, 0, 21.1 );

			AddPage( 0 );

			AddBackground( 0, 0, 440, 135, 5054 );

			AddBackground( 10, 10, 420, 75, 2620 );
			AddBackground( 10, 85, 420, 25, 3000 );

			AddItem( 20, 20, 9682 );
			AddButton( 20, 110, 4005, 4007, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 20, 90, 100, 20, 1018087, false, false ); // Animals

			AddItem( 120, 20, 9607 );
			AddButton( 120, 110, 4005, 4007, 2, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 120, 90, 100, 20, 1018088, false, false ); // Monsters

			AddItem( 220, 20, 8454 );
			AddButton( 220, 110, 4005, 4007, 3, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 220, 90, 100, 20, 1018089, false, false ); // Human NPCs

			AddItem( 320, 20, 8455 );
			AddButton( 320, 110, 4005, 4007, 4, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 320, 90, 100, 20, 1018090, false, false ); // Players
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			if ( info.ButtonID >= 1 && info.ButtonID <= 4 )
				TrackWhoGump.DisplayTo( m_Success, m_From, info.ButtonID - 1 );
		}
	}

	public delegate bool TrackTypeDelegate( Mobile m );

	public class TrackWhoGump : Gump
	{
		private Mobile m_From;
		private int m_Range;

		private static TrackTypeDelegate[] m_Delegates = new TrackTypeDelegate[]
			{
				new TrackTypeDelegate( IsAnimal ),
				new TrackTypeDelegate( IsMonster ),
				new TrackTypeDelegate( IsHumanNPC ),
				new TrackTypeDelegate( IsPlayer )
			};

		private class InternalSorter : IComparer
		{
			private Mobile m_From;

			public InternalSorter( Mobile from )
			{
				m_From = from;
			}

			public int Compare( object x, object y )
			{
				if ( x == null && y == null )
					return 0;
				else if ( x == null )
					return -1;
				else if ( y == null )
					return 1;

				Mobile a = x as Mobile;
				Mobile b = y as Mobile;

				if ( a == null || b == null )
					throw new ArgumentException();

				return m_From.GetDistanceToSqrt( a ).CompareTo( m_From.GetDistanceToSqrt( b ) );
			}
		}

		public static void DisplayTo( bool success, Mobile from, int type )
		{
			if ( !success )
			{
				from.SendLocalizedMessage( 1018092 ); // You see no evidence of those in the area.
				return;
			}

			Map map = from.Map;

			if ( map == null )
				return;

			TrackTypeDelegate check = m_Delegates[type];
			
			from.CheckSkill( SkillName.Tracking, 0, 21.1 );

			int range = 10 + (int)(from.Skills[SkillName.Tracking].Value / 10);
			
			// Adam: 9/19/04 - Return tracking to it's original distance.
			//Pixie: 9/11/04 - increase tracking range (double it)
			// range *= 2;

			ArrayList list = new ArrayList();

			IPooledEnumerable eable = from.GetMobilesInRange( range );
			foreach ( Mobile m in eable)
			{
				if ( m != from && (!m.Hidden || m.AccessLevel == AccessLevel.Player || from.AccessLevel > m.AccessLevel) && check( m ) )
				{
					// At this point, we have a list of all mobiles in range, whether hidden or not.
					// Now we check to see if the current mobile, m, is a player.
					if ( !m.Player )
						list.Add( m );
							else
					{
						bool TRACKINGSKILLDEBUG = (from.AccessLevel > AccessLevel.Player);

						double Tracking = from.Skills[SkillName.Tracking].Value;
						double Forensics = from.Skills[SkillName.Forensics].Value;
						double Hiding = m.Skills[SkillName.Hiding].Value;
						double Stealth = m.Skills[SkillName.Stealth].Value;

						double trackersSkill = Tracking;
						double targetSkill = Stealth;

						double chance = 0;
						if( targetSkill == 0 ) // if target's stealth is 0, start with a base of 100%
						{
							chance = 1;
						}
						else // chance is tracking/stealth*2 (giving 50% at equal levels)
						{
							chance = trackersSkill / ( targetSkill * 2 );
						}

						//Special case - if tracking is < 20 and hiding >= 80,
						// make sure there's difficulty in tracking
						if( from.Skills[SkillName.Tracking].Base < 20.0 
							&& from.Skills[SkillName.Hiding].Base >= 80.0 )
						{
							if( TRACKINGSKILLDEBUG )
							{
								from.SendMessage( string.Format("Changing chance to track {0} from {1} because tracker is below 20.0 base tracking", m.Name, chance) );
							}

							chance = 0;
						}

						//if tracker can see the other, it's much
						//easier to track them
						if( from.CanSee(m) )
						{
							if( TRACKINGSKILLDEBUG )
							{
								from.SendMessage( string.Format("Changing chance to track {0} from {1} because he can be seen",	m.Name, chance) );
							}

							double newchance = Tracking/25.0; //25 tracking == 100% success when visible
							if( newchance > chance ) // make sure we're not killing their chances
							{
								chance = newchance;
							}
						}

						// add bonus for fonensics (10% at GM forensics)
						chance += Forensics/1000;

						// make sure there's always a small chance to
						// succeed and a small chance to fail
						if ( chance <= 0 )
						{
							chance = 0.001; // minimum .1% (1/1000) chance
						}
						if( chance >= 1.0 )
						{
							chance = 0.999; // maximum 99.9% chance
						}

						if( TRACKINGSKILLDEBUG )
						{
							from.SendMessage( 
								string.Format("Your chance to track {0} is {1}.  T:{2:0.00} F:{3:0.00} S:{4:0.00} H{5:0.00}",
								m.Name, chance, Tracking, Forensics, Stealth, Hiding) 
								);
						}

						// Check Skill takes two arguments, the skill to check, and the chance to succeed.
						bool succeeded = from.CheckSkill( SkillName.Tracking, chance );

						// If our skill check succeeds, add the mobile to the list.
						if ( succeeded )
						{
							//Can't track > Player level
							if( m.AccessLevel <= AccessLevel.Player )
							{
								list.Add( m );
							}
						}
					}
				}
			}
			eable.Free();

			if ( list.Count > 0 )
			{
				list.Sort( new InternalSorter( from ) );

				from.SendGump( new TrackWhoGump( from, list, range ) );
				from.SendLocalizedMessage( 1018093 ); // Select the one you would like to track.
			}
			else
			{
				if ( type == 0 )
					from.SendLocalizedMessage( 502991 ); // You see no evidence of animals in the area.
				else if ( type == 1 )
					from.SendLocalizedMessage( 502993 ); // You see no evidence of creatures in the area.
				else
					from.SendLocalizedMessage( 502995 ); // You see no evidence of people in the area.
			}
		}

		private static bool IsAnimal( Mobile m )
		{
			return ( !m.Player && m.Body.IsAnimal );
		}

		private static bool IsMonster( Mobile m )
		{
			return ( !m.Player && m.Body.IsMonster );
		}

		private static bool IsHumanNPC( Mobile m )
		{
			return ( !m.Player && m.Body.IsHuman );
		}

		private static bool IsPlayer( Mobile m )
		{
			return m.Player;
		}

		private ArrayList m_List;

		private TrackWhoGump( Mobile from, ArrayList list, int range ) : base( 20, 30 )
		{
			m_From = from;
			m_List = list;
			m_Range = range;

			AddPage( 0 );

			AddBackground( 0, 0, 440, 155, 5054 );

			AddBackground( 10, 10, 420, 75, 2620 );
			AddBackground( 10, 85, 420, 45, 3000 );

			if ( list.Count > 4 )
			{
				AddBackground( 0, 155, 440, 155, 5054 );

				AddBackground( 10, 165, 420, 75, 2620 );
				AddBackground( 10, 240, 420, 45, 3000 );

				if ( list.Count > 8 )
				{
					AddBackground( 0, 310, 440, 155, 5054 );

					AddBackground( 10, 320, 420, 75, 2620 );
					AddBackground( 10, 395, 420, 45, 3000 );
				}
			}

			for ( int i = 0; i < list.Count && i < 12; ++i )
			{
				Mobile m = (Mobile)list[i];

				AddItem( 20 + ((i % 4) * 100), 20 + ((i / 4) * 155), ShrinkTable.Lookup( m ) );
				AddButton( 20 + ((i % 4) * 100), 130 + ((i / 4) * 155), 4005, 4007, i + 1, GumpButtonType.Reply, 0 );

				if ( m.Name != null )
					AddHtml( 20 + ((i % 4) * 100), 90 + ((i / 4) * 155), 90, 40, m.Name, false, false );
			}
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			int index = info.ButtonID - 1;

			if ( index >= 0 && index < m_List.Count && index < 12 )
			{
				Mobile m = (Mobile)m_List[index];

				m_From.QuestArrow = new TrackArrow( m_From, m, m_Range * 2 );
			}
		}
	}

	public class TrackArrow : QuestArrow
	{
		private Mobile m_From;
		private Timer m_Timer;

		public TrackArrow( Mobile from, Mobile target, int range ) : base( from )
		{
			m_From = from;
			m_Timer = new TrackTimer( from, target, range, this );
			m_Timer.Start();
		}

		public override void OnClick( bool rightClick )
		{
			if ( rightClick )
			{
				m_From = null;

				Stop();
			}
		}

		public override void OnStop()
		{
			m_Timer.Stop();

			if ( m_From != null )
				m_From.SendLocalizedMessage( 503177 ); // You have lost your quarry.
		}
	}

	public class TrackTimer : Timer
	{
		private Mobile m_From, m_Target;
		private int m_Range;
		private int m_LastX, m_LastY;
		private QuestArrow m_Arrow;

		public TrackTimer( Mobile from, Mobile target, int range, QuestArrow arrow ) : base( TimeSpan.FromSeconds( 0.25 ), TimeSpan.FromSeconds( 2.5 ) )
		{
			m_From = from;
			m_Target = target;
			m_Range = range;

			m_Arrow = arrow;
		}

		protected override void OnTick()
		{
			if ( !m_Arrow.Running )
			{
				Stop();
				return;
			}
			else if ( m_From.NetState == null || m_From.Deleted || m_Target.Deleted || m_From.Map != m_Target.Map || !m_From.InRange( m_Target, m_Range ) )
			{
				m_From.Send( new CancelArrow() );
				m_From.SendLocalizedMessage( 503177 ); // You have lost your quarry.

				Stop();
				return;
			}

			if ( m_LastX != m_Target.X || m_LastY != m_Target.Y )
			{
				m_LastX = m_Target.X;
				m_LastY = m_Target.Y;

				m_Arrow.Update( m_LastX, m_LastY );
			}
		}
	}
}
