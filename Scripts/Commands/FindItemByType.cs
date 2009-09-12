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

/* Scripts/Commands/FindItemByType.cs
 * Changelog : 
 *	3/9/07, Adam
 *      Convert to a "find item by type" command
 *	9/7/06, Adam
 *		Remove the hack and make into: Find(multi)ByType 
 *	06/28/06, Adam
 *		Find Mobile by Type (currently hacked to find PlayerBarkeeper only)
 */

using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Multis;

namespace Server.Scripts.Commands
{
	public class FindItemByType
	{
		public static void Initialize()
		{
			Server.Commands.Register("FindItemByType", AccessLevel.Administrator, new CommandEventHandler(FindItemByType_OnCommand));
		}

		[Usage("FindItemByType <type>")]
		[Description("Finds an item by type.")]
		public static void FindItemByType_OnCommand(CommandEventArgs e)
		{
			try
			{
				if (e.Length == 1)
				{
					LogHelper Logger = new LogHelper("FindItemByType.log", e.Mobile, false);

					string name = e.GetString(0);

					foreach (Item item in World.Items.Values)
					{
						if (item != null && item.GetType().ToString().ToLower().IndexOf(name.ToLower()) >= 0)
						{
							Logger.Log(LogType.Item, item);
						}
					}
					Logger.Finish();
				}
				else
				{
					e.Mobile.SendMessage("Format: FindItemByType <type>");
				}
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}
	}
}