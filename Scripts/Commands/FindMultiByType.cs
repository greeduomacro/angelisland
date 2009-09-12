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

/* Scripts/Commands/FindMultiByType.cs
 * Changelog : 
 *	3/9/07, Adam
 *		first time checkin
 */

using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Multis;

namespace Server.Scripts.Commands
{
	public class FindMultiByType
	{
		public static void Initialize()
		{
			Server.Commands.Register("FindMultiByType", AccessLevel.Administrator, new CommandEventHandler(FindMultiByType_OnCommand));
		}

		[Usage("FindMultiByType <type>")]
		[Description("Finds a multi by type.")]
		public static void FindMultiByType_OnCommand(CommandEventArgs e)
		{
			try
			{
				if (e.Length == 1)
				{
					LogHelper Logger = new LogHelper("FindMultiByType.log", e.Mobile, false);

					string name = e.GetString(0);

					foreach (ArrayList list in Server.Multis.BaseHouse.Multis.Values)
					{
						for (int i = 0; i < list.Count; i++)
						{
							BaseHouse house = list[i] as BaseHouse;
							// like Server.Multis.Tower
							if (house.GetType().ToString().ToLower().IndexOf(name.ToLower()) >= 0)
							{
								Logger.Log(house);
							}
						}
					}
					Logger.Finish();
				}
				else
				{
					e.Mobile.SendMessage("Format: FindMultiByType <type>");
				}
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}
	}
}