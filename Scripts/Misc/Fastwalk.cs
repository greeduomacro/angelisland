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
 
 /* Scripts/Misc/Fastwalk.cs
  * CHANGELOG
  *
  *  02/05/07 Taran Kain
  *		Added a bit of flexibility and logging capabilities.
  */

using System;
using System.Collections.Generic;
using Server;
using Server.Scripts.Commands;

namespace Server.Misc
{
	// This fastwalk detection is no longer required
	// As of B36 PlayerMobile implements movement packet throttling which more reliably controls movement speeds
	public class Fastwalk
	{
        public static bool ProtectionEnabled = false;
        public static int WarningThreshold = 4;
        public static TimeSpan WarningCooldown = TimeSpan.FromSeconds(0.4);

        private static Dictionary<Mobile, List<DateTime>> m_Blocks = new Dictionary<Mobile, List<DateTime>>();

		public static void Initialize()
		{
			EventSink.FastWalk += new FastWalkEventHandler( OnFastWalk );
		}

		public static void OnFastWalk( FastWalkEventArgs e )
		{
            if (!m_Blocks.ContainsKey(e.NetState.Mobile))
            {
                m_Blocks.Add(e.NetState.Mobile, new List<DateTime>());
            }
            m_Blocks[e.NetState.Mobile].Add(DateTime.Now);

            if (ProtectionEnabled)
            {
                e.Blocked = true;//disallow this fastwalk
                //Console.WriteLine("Client: {0}: Fast movement detected (name={1})", e.NetState, e.NetState.Mobile.Name);
            }

            try
            {
                List<DateTime> blocks = m_Blocks[e.NetState.Mobile];
                if (e.FastWalkCount > WarningThreshold &&
                    blocks.Count >= 2 && // sanity check, shouldn't be possible to reach this point w/o Count >= 2
                    (blocks[blocks.Count - 1] - blocks[blocks.Count - 2]) > WarningCooldown)
                {
                    Console.WriteLine("FW Warning");
                }
            }
            catch (Exception ex) // we can only exception if Mobile.FwdMaxSteps < 2 - make sure SecurityManagementConsole doesn't set it too low
            {
                LogHelper.LogException(ex);
                Console.WriteLine(ex);
            }
		}
	}
}
