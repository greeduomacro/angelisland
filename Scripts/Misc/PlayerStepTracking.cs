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

/* Scripts/Misc/PlayerStepTracking.cs
 * ChangeLog
 *  6/11/04, Pix
 *		Initial version
 */

using System;
using System.Collections;
using Server;
using Server.Accounting;
using Server.Mobiles;
using Server.Multis;

namespace Server.Misc
{
	public class PlayerStepTracking
	{
		public static void Initialize()
		{
			EventSink.Movement += new MovementEventHandler( EventSink_Movement );
		}


		public static void EventSink_Movement( MovementEventArgs e )
		{
			Mobile from = e.Mobile;

			if ( !from.Player )
				return;

			if ( from is PlayerMobile )
			{
				Account acct = from.Account as Account;
				
				if( acct.m_STIntervalStart + TimeSpan.FromMinutes(20.0) > DateTime.Now )
				{//within 20 minutes from last step - count step
					acct.m_STSteps++;
				}
				else
				{
					//ok, we're outside of a 20-minute period,
					//so see if they've moved enough within the last 10 
					//minutes... if so, increment time
					if( acct.m_STSteps > 50 )
					{
						//Add an house to the house's refresh time
						BaseHouse house = null;
						for( int i=0; i<5; i++ )
						{
							Mobile m = acct[i];
							if( m != null )
							{
								ArrayList list = BaseHouse.GetHouses(m);
								if( list.Count > 0 )
								{
									house = (BaseHouse)list[0];
									break;
								}
							}
						}
						if( house != null )
						{
							house.RefreshHouseOneDay();
						}
					}
					acct.m_STIntervalStart = DateTime.Now;
					acct.m_STSteps = 1;
				}
			}
		}

	}

}