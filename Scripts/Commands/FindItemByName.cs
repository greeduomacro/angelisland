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
/* Changelog :
 *	03/25/05, erlein
 *		Integrated with LogHelper class.		
 *	03/23/05, erlein
 *		Moved to /Scripts/Commands/FindItemByName.cs (for Find* command normalization).
 *		Changed namespace to Server.Scripts.Commands.
 */

using System;
using Server;

namespace Server.Scripts.Commands
{
	public class FindItemByName
	{
		public static void Initialize()
		{
			Server.Commands.Register("FindItemByName", AccessLevel.Administrator, new CommandEventHandler(FindItemByName_OnCommand));
		}

		[Usage("FindItemByName <name>")]
		[Description("Finds an item by name.")]
		public static void FindItemByName_OnCommand(CommandEventArgs e)
		{
			if (e.Length == 1)
			{
				LogHelper Logger = new LogHelper("FindItemByName.log", e.Mobile, false);

				string name = e.GetString(0).ToLower();

				foreach (Item item in World.Items.Values)
					if (item.Name != null && item.Name.ToLower().IndexOf(name) >= 0)
						Logger.Log(LogType.Item, item);

				Logger.Finish();


			}
			else
			{
				e.Mobile.SendMessage("Format: FindItemByName <name>");
			}
		}
	}
}