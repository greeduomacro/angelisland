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

/* Scripts/Commands/Host.cs
 * 	CHANGELOG:
 *  12/19/06, Adam
 *      Improve output to show if this is the PROD or TC server.
 * 	2/23/06, Adam
 *	    Initial Version
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
	public class Host
	{
		public static void Initialize()
		{
			Server.Commands.Register( "Host", AccessLevel.GameMaster, new CommandEventHandler( Host_OnCommand ) );
		}

		[Usage( "Host" )]
		[Description( "Display host information." )]
		public static void Host_OnCommand( CommandEventArgs e )
		{
			try
			{
				string host = Dns.GetHostName();
				IPHostEntry iphe = Dns.Resolve( host );
				IPAddress[] ips = iphe.AddressList;

                e.Mobile.SendMessage("You are on the \"{0}\" Server.",
                    Utility.IsHostPROD(host) ? "PROD" : Utility.IsHostTC(host) ? "Test Center" : host );

				for ( int i = 0; i < ips.Length; ++i )
					e.Mobile.SendMessage( "IP: {0}", ips[i]);
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}
	}
}