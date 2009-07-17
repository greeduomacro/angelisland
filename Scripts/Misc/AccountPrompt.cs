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
using Server.Accounting;

namespace Server.Misc
{
	public class AccountPrompt
	{
		// This script prompts the console for a username and password when 0 accounts have been loaded
		public static void Initialize()
		{
			if ( Accounts.Table.Count == 0 && !Core.Service )
			{
				Console.WriteLine( "This server has no accounts." );
				Console.WriteLine( "Do you want to create an administrator account now? (y/n)" );

				if ( Console.ReadLine().StartsWith( "y" ) )
				{
					Console.Write( "Username: " );
					string username = Console.ReadLine();

					Console.Write( "Password: " );
					string password = Console.ReadLine();

					Account a = Accounts.AddAccount( username, password );

					a.AccessLevel = AccessLevel.Administrator;

					Console.WriteLine( "Account created, continuing" );
				}
				else
				{
					Console.WriteLine( "Account not created." );
				}
			}
		}
	}
} 
