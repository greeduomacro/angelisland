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

/* Scripts/Commands/RotateLogs.cs
 * 	CHANGELOG:
 *	11/14/06, Adam
 *		Adjust rollover tag to be seconds since 1/1/2000
 * 	11/3/06, Adam
 *		Initial Version
 */

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Scripts.Commands;

namespace Server.Scripts.Commands
{
	public class RotateLogs
	{
		public static void Initialize()
		{
			Server.Commands.Register( "RotateLogs", AccessLevel.Administrator, new CommandEventHandler( RotateLogs_OnCommand ) );
		}

		[Usage( "RotateLogs" )]
		[Description( "Rotate player command logs." )]
		public static void RotateLogs_OnCommand( CommandEventArgs e )
		{
			try
			{
				RotateNow();
				e.Mobile.SendMessage("Log rotation complete.");
			}
			catch(Exception ex)
            {
                LogHelper.LogException(ex);
				Console.WriteLine(ex.ToString());
				e.Mobile.SendMessage("Log rotation failed.");
			}
		}

		public static void RotateNow()
		{
			try
			{
				// close the open logfile
				CommandLogging.Close();

				string root = Path.Combine( Core.BaseDirectory, "Logs" );

				if ( !Directory.Exists( root ) )
					Directory.CreateDirectory( root );

				string[] existing = Directory.GetDirectories( root );

				DirectoryInfo dir;

				// rename the commands directory with a date-time stamp
				dir = Match( existing, "Commands" );
				if ( dir != null )
				{
					TimeSpan tx = DateTime.Now - new DateTime(2000,1,1);
					string ToName = String.Format( "{0}, {1:X}", DateTime.Now.ToLongDateString(), (int)tx.TotalSeconds );
					try{ dir.MoveTo( FormatDirectory( root, ToName, "" ) ); }
					catch(Exception	ex)
					{ 
                        LogHelper.LogException(ex); 
						Console.WriteLine("Failed to move to {0}", FormatDirectory( root, ToName, "" )); 
						Console.WriteLine(ex.ToString());
						throw(ex);
					}
				}

				// reopen the logfile
				CommandLogging.Open();
			}
			catch(Exception ex)
			{
                LogHelper.LogException(ex); 
				throw(ex);
			}
		}

		private static string FormatDirectory( string root, string name, string timeStamp )
		{
			return Path.Combine( root, String.Format( "{0}", name) );
		}

		private static DirectoryInfo Match( string[] paths, string match )
		{
			for ( int i = 0; i < paths.Length; ++i )
			{
				DirectoryInfo info = new DirectoryInfo( paths[i] );

				if ( info.Name.StartsWith( match ) )
					return info;
			}

			return null;
		}
	}
}