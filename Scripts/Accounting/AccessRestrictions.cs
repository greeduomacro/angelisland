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

/* Scripts/Accounting/AccountHandler.cs
 * ChangeLog:
 *	2/27/06 - Pix.
 *		Added.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Server;
using Server.Misc;

namespace Server
{
	public class AccessRestrictions
	{
		public static void Initialize()
		{
			EventSink.SocketConnect += new SocketConnectEventHandler( EventSink_SocketConnect );
		}
			
		private static void EventSink_SocketConnect( SocketConnectEventArgs e )
		{
			try
			{
				IPAddress ip = ((IPEndPoint)e.Socket.RemoteEndPoint).Address;

				if ( Firewall.IsBlocked( ip ) )
				{
					Console.WriteLine( "Client: {0}: Firewall blocked connection attempt.", ip );
					e.AllowConnection = false;
					return;
				}
				else if ( IPLimiter.SocketBlock && !IPLimiter.Verify( ip ) )
				{
					Console.WriteLine( "Client: {0}: Past IP limit threshold", ip );

					using ( StreamWriter op = new StreamWriter( "ipLimits.log", true ) )
						op.WriteLine( "{0}\tPast IP limit threshold\t{1}", ip, DateTime.Now );
	
					e.AllowConnection = false;
					return;
				}
			}
			catch
			{
				e.AllowConnection = false;
			}
		}
	}
}