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

using System;
using Server;
using Server.Network;
using Server.Accounting;

namespace Server.Misc
{
	public class Profile
	{
		public static void Initialize()
		{
			EventSink.ProfileRequest += new ProfileRequestEventHandler( EventSink_ProfileRequest );
			EventSink.ChangeProfileRequest += new ChangeProfileRequestEventHandler( EventSink_ChangeProfileRequest );
		}

		public static void EventSink_ChangeProfileRequest( ChangeProfileRequestEventArgs e )
		{
			Mobile from = e.Beholder;

			if ( from.ProfileLocked )
				from.SendMessage( "Your profile is locked. You may not change it." );
			else
				from.Profile = e.Text;
		}

		public static void EventSink_ProfileRequest( ProfileRequestEventArgs e )
		{
			Mobile beholder = e.Beholder;
			Mobile beheld = e.Beheld;

			if ( !beheld.Player )
				return;

			if ( beholder.Map != beheld.Map || !beholder.InRange( beheld, 12 ) || !beholder.CanSee( beheld ) )
				return;

			string header = Titles.ComputeTitle( beholder, beheld );

			string footer = "";

			if ( beheld.ProfileLocked )
			{
				if ( beholder == beheld )
					footer = "Your profile has been locked.";
				else if ( beholder.AccessLevel >= AccessLevel.Counselor )
					footer = "This profile has been locked.";
			}

			if ( footer == "" && beholder == beheld )
				footer = GetAccountDuration( beheld );

			string body = beheld.Profile;

			if ( body == null || body.Length <= 0 )
				body = "";

			beholder.Send( new DisplayProfile( beholder != beheld || !beheld.ProfileLocked, beheld, header, body, footer ) );
		}

		private static string GetAccountDuration( Mobile m )
		{
			Account a = m.Account as Account;

			if ( a == null )
				return "";

			TimeSpan ts = DateTime.Now - a.Created;

			string v;

			if ( Format( ts.TotalDays, "This account is {0} day{1} old.", out v ) )
				return v;

			if ( Format( ts.TotalHours, "This account is {0} hour{1} old.", out v ) )
				return v;

			if ( Format( ts.TotalMinutes, "This account is {0} minute{1} old.", out v ) )
				return v;

			if ( Format( ts.TotalSeconds, "This account is {0} second{1} old.", out v ) )
				return v;

			return "";
		}

		public static bool Format( double value, string format, out string op )
		{
			if ( value >= 1.0 )
			{
				op = String.Format( format, (int)value, (int)value != 1 ? "s" : "" );
				return true;
			}

			op = null;
			return false;
		}
	}
} 
