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

/* Scripts/Misc/SocketOptions.cs
 * ChangeLog
 *	2/27/06, Pix
 *		Changes for IPLimiter.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Server;
using Server.Misc;
using Server.Network;

namespace Server
{
    public class SocketOptions
    {
        private const bool NagleEnabled = false; // Should the Nagle algorithm be enabled? This may reduce performance
        private const int CoalesceBufferSize = 512; // MSS that the core will use when buffering packets
        private const int PooledSockets = 32; // The number of sockets to initially pool. Ideal value is expected client count. 

        private static IPEndPoint[] m_ListenerEndPoints = new IPEndPoint[] {
			new IPEndPoint( IPAddress.Any, 2593 ), // Default: Listen on port 2593 on all IP addresses
			
			// Examples:
			// new IPEndPoint( IPAddress.Any, 80 ), // Listen on port 80 on all IP addresses
			// new IPEndPoint( IPAddress.Parse( "1.2.3.4" ), 2593 ), // Listen on port 2593 on IP address 1.2.3.4
		};

        public static void Initialize()
        {
            SendQueue.CoalesceBufferSize = CoalesceBufferSize;
            SocketPool.InitialCapacity = PooledSockets;

            EventSink.SocketConnect += new SocketConnectEventHandler(EventSink_SocketConnect);

            Listener.EndPoints = m_ListenerEndPoints;
        }

        private static void EventSink_SocketConnect(SocketConnectEventArgs e)
        {
            if (!e.AllowConnection)
                return;

            if (!NagleEnabled)
                e.Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1); // RunUO uses its own algorithm
        }
    }
}

//using System;
//using System.IO;
//using System.Net;
//using System.Net.Sockets;
//using Server;
//using Server.Misc;
//using Server.Network;

//namespace Server
//{
//    public class SocketOptions
//    {
//        private const int CoalesceBufferSize = 512; // MSS that the core will use when buffering packets, a value of 0 will turn this buffering off and Nagle on

//        private static int[] m_AdditionalPorts = new int[0];
//        //private static int[] m_AdditionalPorts = new int[]{ 2594 };

//        public static void Initialize()
//        {
//            NetState.CreatedCallback = new NetStateCreatedCallback( NetState_Created );
//            SendQueue.CoalesceBufferSize = CoalesceBufferSize;

//            if ( m_AdditionalPorts.Length > 0 )
//                EventSink.ServerStarted += new ServerStartedEventHandler( EventSink_ServerStarted );
//        }

//        public static void EventSink_ServerStarted()
//        {
//            //for (int i = 0; i < m_AdditionalPorts.Length; ++i)
//            //{
//            //    Core.MessagePump.AddListener(new Listener(m_AdditionalPorts[i]));
//            //}
//        }

//        public static void NetState_Created( NetState ns )
//        {
//            if ( IPLimiter.SocketBlock && !IPLimiter.Verify( ns.Address ) )
//            {
//                Console.WriteLine( "Login: {0}: Past IP limit threshold", ns );

//                using ( StreamWriter op = new StreamWriter( "ipLimits.log", true ) )
//                    op.WriteLine( "{0}\tPast IP limit threshold\t{1}", ns, DateTime.Now );

//                ns.Dispose();
//                return;
//            }

//            Socket s = ns.Socket;

//            s.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 15000 );
//            s.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 15000 );

//            if ( CoalesceBufferSize > 0 )
//                s.SetSocketOption( SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1 ); // RunUO uses its own algorithm
//        }
//    }
//}