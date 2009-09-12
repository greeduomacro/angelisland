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
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Prompts;
using Server.Targeting;
using Server.Misc;
using Server.Multis;

namespace Server.Scripts.Commands
{

	public class ComLogger
	{
		public static void Initialize()
		{
			Server.Commands.Register("ComLogger", AccessLevel.Administrator, new CommandEventHandler(ComLogger_OnCommand));
		}

		[Usage("ComLogger")]
		[Description("Logs all commodity deeds in world info")]
		public static void ComLogger_OnCommand(CommandEventArgs e)
		{
			LogHelper Logger = new LogHelper("Commoditydeed.log", true);

			foreach (Item m in World.Items.Values)
			{

				if (m != null)
				{
					if (m is CommodityDeed && ((CommodityDeed)m).Commodity != null)
					{
						string output = string.Format("{0}\t{1,-25}\t{2,-25}",
							m.Serial + ",", ((CommodityDeed)m).Commodity + ",", ((CommodityDeed)m).Commodity.Amount);

						Logger.Log(LogType.Text, output);
					}
				}
			}
			Logger.Finish();
		}
	}
}
