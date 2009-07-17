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

/* Scripts/Regions/GuardedRegion.cs
 * ChangeLog
 *	07/23/08, weaver
 *		Added Free() before return in IPooledEnumerable loop.
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 3 loops updated.
 *	9/1/07, Adam
 *		change [SetGuarded from AccessLevel Administrator to Seer
 *  03/31/06 Taran Kain
 *		Changed GuardTimer, CallGuards to only display "Guards cannot be called on you" if mobile is not red
 *  04/23/05, erlein
 *    Changed ToggleGuarded command to Seer level access.
 *	04/19/05, Kit
 *		Added check to IsGuardCandidate( ) to not make hidden players canadites.
 *	10/28/04, Pix
 *		In CheckGuardCandidate() ruled out the case where a player can recall to a guardzone before
 *		explosion hits and the person that casts explosion gets guardwhacked.
 *	8/12/04, mith
 *		IsGuardCandidate(): Modified to that player vendors will not call guards.
 *  6/21/04, Old Salty
 *  	Added a little code to CallGuards to close the bankbox of a criminal when the guards come
 *  6/20/04, Old Salty
 * 		Fixed IsGuardCandidate so that guards react properly 
 * 
 *	6/10/04, mith
 *		Modified to work with the new non-insta-kill guards.
 */

using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Spells;

namespace Server.Regions
{
	public class GuardedRegion : Region
	{
		private static object[] m_GuardParams = new object[1];
		private Type m_GuardType;
		private bool m_GuardsDisabled;

		public virtual bool IsGuarded{ get{ return !m_GuardsDisabled; } set{ m_GuardsDisabled = !value; } }

		public static void Initialize()
		{
			Commands.Register( "CheckGuarded", AccessLevel.GameMaster, new CommandEventHandler( CheckGuarded_OnCommand ) );
			Commands.Register( "SetGuarded", AccessLevel.Seer, new CommandEventHandler( SetGuarded_OnCommand ) );
			Commands.Register( "ToggleGuarded", AccessLevel.Seer, new CommandEventHandler( ToggleGuarded_OnCommand ) );
		}

		[Usage( "CheckGuarded" )]
		[Description( "Returns a value indicating if the current region is guarded or not." )]
		private static void CheckGuarded_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			GuardedRegion reg = from.Region as GuardedRegion;

			if ( reg == null )
				from.SendMessage( "You are not in a guardable region." );
			else if ( reg.IsGuarded == false )
				from.SendMessage( "The guards in this region have been disabled." );
			else
				from.SendMessage( "This region is actively guarded." );
		}

		[Usage( "SetGuarded <true|false>" )]
		[Description( "Enables or disables guards for the current region." )]
		private static void SetGuarded_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( e.Length == 1 )
			{
				GuardedRegion reg = from.Region as GuardedRegion;

				if ( reg == null )
				{
					from.SendMessage( "You are not in a guardable region." );
				}
				else
				{
					reg.IsGuarded = e.GetBoolean( 0 );

					if ( reg.IsGuarded == false )
						from.SendMessage( "The guards in this region have been disabled." );
					else
						from.SendMessage( "The guards in this region have been enabled." );
				}
			}
			else
			{
				from.SendMessage( "Format: SetGuarded <true|false>" );
			}
		}

		[Usage( "ToggleGuarded" )]
		[Description( "Toggles the state of guards for the current region." )]
		private static void ToggleGuarded_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			GuardedRegion reg = from.Region as GuardedRegion;

			if ( reg == null )
			{
				from.SendMessage( "You are not in a guardable region." );
			}
			else
			{
				reg.IsGuarded = !reg.IsGuarded;

				if ( reg.IsGuarded == false )
					from.SendMessage( "The guards in this region have been disabled." );
				else
					from.SendMessage( "The guards in this region have been enabled." );
			}
		}

		public GuardedRegion( string prefix, string name, Map map, Type guardType ) : base( prefix, name, map )
		{
			m_GuardType = guardType;
		}

		public override bool OnBeginSpellCast( Mobile m, ISpell s )
		{
			if ( IsGuarded && !s.OnCastInTown( this ) )
			{
				m.SendLocalizedMessage( 500946 ); // You cannot cast this in town!
				return false;
			}

			return base.OnBeginSpellCast( m, s );
		}

		public override bool AllowHousing( Mobile from, Point3D p )
		{
			return false;
		}

		public override void MakeGuard( Mobile focus )
		{
			BaseGuard useGuard = null;

			IPooledEnumerable eable = focus.GetMobilesInRange( 8 );
			foreach ( Mobile m in eable)
			{
				if ( m is BaseGuard )
				{
					BaseGuard g = (BaseGuard)m;

					if ( g.Focus == null ) // idling
					{
						useGuard = g;
						break;
					}
				}
			}
			eable.Free();

			if ( useGuard != null )
			{
				useGuard.Focus = focus;
			}
			else
			{
				m_GuardParams[0] = focus;

				Activator.CreateInstance( m_GuardType, m_GuardParams );
			}
		}

		public override void OnEnter( Mobile m )
		{
			if ( IsGuarded == false )
				return;

			//m.SendLocalizedMessage( 500112 ); // You are now under the protection of the town guards.

			if ( m.Kills >= 5 )
				CheckGuardCandidate( m );
		}

		public override void OnExit( Mobile m )
		{
			if ( IsGuarded == false )
				return;

			//m.SendLocalizedMessage( 500113 ); // You have left the protection of the town guards.
		}

		public override void OnSpeech( SpeechEventArgs args )
		{
			if ( IsGuarded == false )
				return;

			if ( args.Mobile.Alive && args.HasKeyword( 0x0007 ) ) // *guards*
				CallGuards( args.Mobile.Location );
		}

		public override void OnAggressed( Mobile aggressor, Mobile aggressed, bool criminal )
		{
			base.OnAggressed( aggressor, aggressed, criminal );

			if ( IsGuarded && aggressor != aggressed && criminal )
				CheckGuardCandidate( aggressor );
		}

		public override void OnGotBenificialAction( Mobile helper, Mobile helped )
		{
			base.OnGotBenificialAction( helper, helped );

			if ( IsGuarded == false )
				return;

			int noto = Notoriety.Compute( helper, helped );

			if ( helper != helped && (noto == Notoriety.Criminal || noto == Notoriety.Murderer) )
				CheckGuardCandidate( helper );
		}

		public override void OnCriminalAction( Mobile m, bool message )
		{
			base.OnCriminalAction( m, message );

			if ( IsGuarded )
				CheckGuardCandidate( m );
		}

		private Hashtable m_GuardCandidates = new Hashtable();

		public void CheckGuardCandidate( Mobile m )
		{
			if ( IsGuarded == false )
				return;

			if ( IsGuardCandidate( m ) )
			{
				GuardTimer timer = (GuardTimer)m_GuardCandidates[m];

				if ( timer == null )
				{
					timer = new GuardTimer( m, m_GuardCandidates );
					timer.Start();

					m_GuardCandidates[m] = timer;
					m.SendLocalizedMessage( 502275 ); // Guards can now be called on you!

					Map map = m.Map;

					if ( map != null )
					{
						Mobile fakeCall = null;
						double prio = 0.0;

						IPooledEnumerable eable = m.GetMobilesInRange( 8 );
						foreach ( Mobile v in eable)
						{
							if ( !v.Player &&
								  v.Body.IsHuman && 
								  v != m && 
								 !IsGuardCandidate( v ) )
							{
								//Pixie 10/28/04: checking whether v is in the region fixes the problem
								// where player1 recalls to a guardzone before player2's explosion hits ...
								if( this.Contains( v.Location ) )
								{
									double dist = m.GetDistanceToSqrt( v );

									if ( fakeCall == null || dist < prio )
									{
										fakeCall = v;
										prio = dist;
									}
								}
								else
								{
									//System.Console.WriteLine( "Mobile ({0}) isn't in this region, so skip him!", v.Name );
								}
							}
						}
						eable.Free();

						if ( fakeCall != null )
						{
							fakeCall.Say( Utility.RandomList( 1007037, 501603, 1013037, 1013038, 1013039, 1013041, 1013042, 1013043, 1013052 ) );
							MakeGuard( m );
							m_GuardCandidates.Remove( m );
							m.SendLocalizedMessage( 502276 ); // Guards can no longer be called on you.
						}
					}
				}
				else
				{
					timer.Stop();
					timer.Start();
				}
			}
		}

		public void CallGuards( Point3D p )
		{
			if ( IsGuarded == false )
				return;

			IPooledEnumerable eable = Map.GetMobilesInRange( p, 14 );

			foreach ( Mobile m in eable )
			{
				if ( IsGuardCandidate( m ) && ((m.Kills >= 5 && Mobiles.ContainsKey( m.Serial )) || m_GuardCandidates.Contains( m )) )
				{
					if ( m.BankBox != null ) // Old Salty - Added to close the bankbox of a criminal on GuardCall
						m.BankBox.Close();
					MakeGuard( m );
					m_GuardCandidates.Remove( m );
					if (m.Kills < 5)
						m.SendLocalizedMessage( 502276 ); // Guards can no longer be called on you.
					break;
				}
			}

			eable.Free();
		}

		public bool IsGuardCandidate( Mobile m )
		{
			if ( m is BaseGuard || m is PlayerVendor || !m.Alive || m.AccessLevel > AccessLevel.Player || m.Blessed || !IsGuarded || m.Hidden)
				return false;

			IPooledEnumerable eable = m.GetMobilesInRange( 10 );
			foreach ( Mobile check in eable)
			{
				BaseGuard guard = check as BaseGuard;
				if (guard != null && guard.Focus == m)
				{
					eable.Free();
					return false;
				}
			}
			eable.Free();
					

			return m.Kills >= 5 || m.Criminal;
		}

		private class GuardTimer : Timer
		{
			private Mobile m_Mobile;
			private Hashtable m_Table;

			public GuardTimer( Mobile m, Hashtable table ) : base( TimeSpan.FromSeconds( 15.0 ) )
			{
				Priority = TimerPriority.TwoFiftyMS;

				m_Mobile = m;
				m_Table = table;
			}

			protected override void OnTick()
			{
				if ( m_Table.Contains( m_Mobile ) )
				{
					m_Table.Remove( m_Mobile );
					if (m_Mobile.Kills < 5)
						m_Mobile.SendLocalizedMessage( 502276 ); // Guards can no longer be called on you.
				}
			}
		}
	}
}
