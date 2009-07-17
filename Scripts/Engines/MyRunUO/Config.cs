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

/* /Scripts/Engines/MyRunUO/Config.cs
 * Changelog:
 *  04/28/05 TK
 *		Added DisplaySQL option to help in debugging
 *		Set up configuration parameters (re-configure for production server!)
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Text;
using System.Threading;

namespace Server.Engines.MyRunUO
{
	public class Config
	{
		// Is MyRunUO enabled?
		public static bool Enabled = false;

		// Details required for database connection string
		public static string DatabaseDriver			= "{MySQL ODBC 3.51 Driver}";
		public static string DatabaseServer			= "localhost";
		public static string DatabaseName			= "MyRunUO";
		public static string DatabaseUserID			= "myrunuo";
		public static string DatabasePassword		= "myrunuo";

		// Should we display all SQL commands? Useful for debugging.
		public static bool DisplaySQL = false;

		// Should the database use transactions? This is recommended
		public static bool UseTransactions = true;

		// this is false because it requires superuser permission for myrunuo user
		// Use optimized table loading techniques? (LOAD DATA INFILE)
		public static bool LoadDataInFile = false;

		// This must be enabled if the database server is on a remote machine.
		public static bool DatabaseNonLocal = ( DatabaseServer != "localhost" );

		// Text encoding used
		public static Encoding EncodingIO = Encoding.ASCII;

		// Database communication is done in a seperate thread. This value is the 'priority' of that thread, or, how much CPU it will try to use
		public static ThreadPriority DatabaseThreadPriority = ThreadPriority.BelowNormal;

		// Any character with an AccessLevel equal to or higher than this will not be displayed
		public static AccessLevel HiddenAccessLevel	= AccessLevel.Counselor;

		// Export character database every 30 minutes
		public static TimeSpan CharacterUpdateInterval = TimeSpan.FromMinutes( 30.0 );

		// Export online list database every 5 minutes
		public static TimeSpan StatusUpdateInterval = TimeSpan.FromMinutes( 5.0 );

		public static string CompileConnectionString()
		{
			string connectionString = String.Format( "DRIVER={0};SERVER={1};DATABASE={2};UID={3};PASSWORD={4};",
				DatabaseDriver, DatabaseServer, DatabaseName, DatabaseUserID, DatabasePassword );

			return connectionString;
		}
	}
}