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

/* Scripts\Commands\FreezeDry.cs
 * Changelog
 *  6/7/07, Adam
 *      Add FDStats command to dump the eligible vs FD'ed containers
 *	12/14/05 Taran Kain
 *		Initial version.
 */

using System;
using System.Collections;
using Server;
using Server.Targeting;
using Server.Items;

namespace Server.Scripts.Commands
{
	/// <summary>
	/// Summary description for FreezeDry.
	/// </summary>
	public class FreezeDry
	{
		public static void Initialize()
		{
            Server.Commands.Register("FDStats", AccessLevel.Administrator, new CommandEventHandler(On_FDStats));
			Server.Commands.Register("StartFDTimers", AccessLevel.Administrator, new CommandEventHandler(On_StartFDTimers));
			Server.Commands.Register("RehydrateAll", AccessLevel.Administrator, new CommandEventHandler(On_RehydrateAll));
		}

        public static void On_FDStats(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Display FreezeDry status for all eligible containers...");

            DateTime start = DateTime.Now;
            int containers = 0;
            int eligible = 0;
            int freezeDried = 0;
            int scheduled = 0;
            int orphans = 0;
            foreach (Item i in World.Items.Values)
            {
                if (i as Container != null)
                {
                    Container cx = i as Container;
                    containers++;
                    if (cx.CanFreezeDry == true)
                    {
                        eligible++;
                        if (cx.IsFreezeDried == true)
                            freezeDried++;
                    }
                    
                    if (cx.IsFreezeScheduled == true)
                        scheduled++;

                    if (cx.CanFreezeDry == true && cx.IsFreezeDried == false && cx.IsFreezeScheduled == false)
                        orphans++;
                }
            }

            e.Mobile.SendMessage("Out of {0} eligible containers, {1} are freeze dried, {2} scheduled, and {3} orphans.", eligible, freezeDried, scheduled, orphans);
            DateTime end = DateTime.Now;
            e.Mobile.SendMessage("Finished in {0}ms.", (end - start).TotalMilliseconds);
        }

		public static void On_StartFDTimers(CommandEventArgs e)
		{
			e.Mobile.SendMessage("Starting FreezeTimers for all eligible containers...");

			DateTime start = DateTime.Now;
			foreach (Item i in World.Items.Values)
				i.OnRehydrate();

			DateTime end = DateTime.Now;
			e.Mobile.SendMessage("Finished in {0}ms.", (end - start).TotalMilliseconds);
		}

		public static void On_RehydrateAll(CommandEventArgs e)
		{
			e.Mobile.SendMessage("Rehydrating all FreezeDried containers...");

			int count = 0;

			DateTime start = DateTime.Now;
			ArrayList al = new ArrayList(World.Items.Values);
			foreach (Item i in al)
			{
				if (i.IsFreezeDried)
				{
					i.Rehydrate();
					count++;
				}
			}

			DateTime end = DateTime.Now;
			e.Mobile.SendMessage("{0} containers rehydrated in {1} seconds.", count, (end - start).TotalSeconds);
			e.Mobile.SendMessage("Rehydrate() averaged {0}ms per call.", (end - start).TotalMilliseconds / count);
		}
	}
}
