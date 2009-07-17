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

/* Scripts/Misc/ServerList.cs
 * Changelog:
 *  11/17,06, Adam
 *      Add: #pragma warning disable 429
 *      The Unreachable code complaints in this file are acceptable
 *      C:\Program Files\RunUO\Scripts\Misc\ServerList.cs(65,38): warning CS0429: Unreachable expression code detected
 *      C:\Program Files\RunUO\Scripts\Misc\ServerList.cs(65,67): warning CS0429: Unreachable expression code detected
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 *  4/5/04 code changes by Pixie
 *		Changed servername to "Angel Island"
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Server;
using Server.Network;

namespace Server.Misc
{
    public class ServerList
    {
        /* 
         * The default setting for Address, a value of 'null', will use your local IP address. If all of your local IP addresses
         * are private network addresses and AutoDetect is 'true' then RunUO will attempt to discover your public IP address
         * for you automatically.
         *
         * If you do not plan on allowing clients outside of your LAN to connect, you can set AutoDetect to 'false' and leave
         * Address set to 'null'.
         * 
         * If your public IP address cannot be determined, you must change the value of Address to your public IP address
         * manually to allow clients outside of your LAN to connect to your server. Address can be either an IP address or
         * a hostname that will be resolved when RunUO starts.
         * 
         * If you want players outside your LAN to be able to connect to your server and you are behind a router, you must also
         * forward TCP port 2593 to your private IP address. The procedure for doing this varies by manufacturer but generally
         * involves configuration of the router through your web browser.
         *
         * ServerList will direct connecting clients depending on both the address they are connecting from and the address and
         * port they are connecting to. If it is determined that both ends of a connection are private IP addresses, ServerList
         * will direct the client to the local private IP address. If a client is connecting to a local public IP address, they
         * will be directed to whichever address and port they initially connected to. This allows multihomed servers to function
         * properly and fully supports listening on multiple ports. If a client with a public IP address is connecting to a
         * locally private address, the server will direct the client to either the AutoDetected IP address or the manually entered
         * IP address or hostname, whichever is applicable. Loopback clients will be directed to loopback.
         * 
         * If you would like to listen on additional ports (i.e. 22, 23, 80, for clients behind highly restrictive egress
         * firewalls) or specific IP adddresses you can do so by modifying the file SocketOptions.cs found in this directory.
         */

        public static readonly string Address = null;
        public static readonly string ServerName = "Angel Island";

        public static readonly bool AutoDetect = false;

        public static void Initialize()
        {
            if (Address == null)
            {
                if (AutoDetect)
                {
                    AutoDetection();
                }
            }
            else
            {
                Resolve(Address, out m_PublicAddress);
            }

            EventSink.ServerList += new ServerListEventHandler(EventSink_ServerList);
        }

        private static IPAddress m_PublicAddress;

        private static void EventSink_ServerList(ServerListEventArgs e)
        {
            try
            {
                NetState ns = e.State;
                Socket s = ns.Socket;

                IPEndPoint ipep = (IPEndPoint)s.LocalEndPoint;

                IPAddress localAddress = ipep.Address;
                int localPort = ipep.Port;

                if (IsPrivateNetwork(localAddress))
                {
                    ipep = (IPEndPoint)s.RemoteEndPoint;
                    if (!IsPrivateNetwork(ipep.Address) && m_PublicAddress != null)
                    {
                        localAddress = m_PublicAddress;
                    }
                }

                e.AddServer(ServerName, new IPEndPoint(localAddress, localPort));
            }
            catch
            {
                e.Rejected = true;
            }
        }

        private static void AutoDetection()
        {
            if (!HasPublicIPAddress())
            {
                Console.Write("ServerList: Auto-detecting public IP address...");
                m_PublicAddress = FindPublicAddress();

                if (m_PublicAddress != null)
                {
                    Console.WriteLine("done ({0})", m_PublicAddress.ToString());
                }
                else
                {
                    Console.WriteLine("failed");
                }
            }
        }

        private static void Resolve(string addr, out IPAddress outValue)
        {
            if (IPAddress.TryParse(addr, out outValue))
                return;

            try
            {
                IPHostEntry iphe = Dns.GetHostEntry(addr);

                if (iphe.AddressList.Length > 0)
                    outValue = iphe.AddressList[iphe.AddressList.Length - 1];
            }
            catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
        }

        private static bool HasPublicIPAddress()
        {
            IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress[] ips = iphe.AddressList;

            for (int i = 0; i < ips.Length; ++i)
                if (!IsPrivateNetwork(ips[i]))
                    return true;

            return false;
        }

        private static bool IsPrivateNetwork(IPAddress ip)
        {
            // 10.0.0.0/8
            // 172.16.0.0/12
            // 192.168.0.0/16

            if (Utility.IPMatch("192.168.*", ip))
                return true;
            else if (Utility.IPMatch("10.*", ip))
                return true;
            else if (Utility.IPMatch("172.16-31.*", ip))
                return true;
            else
                return false;
        }

        private static IPAddress FindPublicAddress()
        {
            try
            {
                WebRequest req = HttpWebRequest.Create("http://www.runuo.com/ip.php");
                req.Timeout = 15000;

                WebResponse res = req.GetResponse();

                Stream s = res.GetResponseStream();

                StreamReader sr = new StreamReader(s);

                IPAddress ip = IPAddress.Parse(sr.ReadLine());

                sr.Close();
                s.Close();
                res.Close();

                return ip;
            }
            catch
            {
                return null;
            }
        }
    }
}


//#pragma warning disable 429

//using System;
//using System.Net;
//using System.Net.Sockets;
//using Server;
//using Server.Network;

//namespace Server.Misc
//{
//    public class ServerList
//    {
//        /* Address:
//         * 
//         * The default setting, a value of 'null', will attempt to detect your IP address automatically:
//         * private const string Address = null;
//         * 
//         * This detection, however, does not work for servers behind routers. If you're running behind a router, put in your IP:
//         * private const string Address = "12.34.56.78";
//         * 
//         * If you need to resolve a DNS host name, you can do that too:
//         * private const string Address = "shard.host.com";
//         */

//        public const string Address = "obtuse.org";
//        //public const string Address = null;

//        public const string ServerName = "Pix's AI Test";

//        public static void Initialize()
//        {
//            //Listener.Port = 2593;

//            EventSink.ServerList += new ServerListEventHandler( EventSink_ServerList );
//        }

//        public static void EventSink_ServerList( ServerListEventArgs e )
//        {
//            try
//            {
//                IPAddress ipAddr;

//                string localAddress = null;
//                IPAddress localAddr;

//                IPAddress hostAddress;

//                Resolve(localAddress != null && !IsLocalMachine(e.State) ? localAddress : Dns.GetHostName(), out localAddr);
//                Resolve("192.168.11.154", out hostAddress);

//                if (Resolve(Address != null && !IsLocalMachine(e.State) ? Address : Dns.GetHostName(), out ipAddr))
//                {
//                    e.AddServer(ServerName, new IPEndPoint(ipAddr, Listener.Port));

//                    e.AddServer("HOST", new IPEndPoint(hostAddress, Listener.Port));

//                    e.AddServer("Local Server", new IPEndPoint(localAddr, Listener.Port));
//                }
//                else
//                {
//                    e.Rejected = true;
//                }
//            }
//            catch
//            {
//                e.Rejected = true;
//            }
//        }

//        public static bool Resolve( string addr, out IPAddress outValue )
//        {
//            try
//            {
//                outValue = IPAddress.Parse( addr );
//                return true;
//            }
//            catch
//            {
//                try
//                {
//                    IPHostEntry iphe = Dns.Resolve( addr );

//                    if ( iphe.AddressList.Length > 0 )
//                    {
//                        outValue = iphe.AddressList[iphe.AddressList.Length - 1];
//                        return true;
//                    }
//                }
//                catch
//                {
//                }
//            }

//            outValue = IPAddress.None;
//            return false;
//        }

//        private static bool IsLocalMachine( NetState state )
//        {
//            Socket sock = state.Socket;

//            IPAddress theirAddress = ((IPEndPoint)sock.RemoteEndPoint).Address;

//            if ( IPAddress.IsLoopback( theirAddress ) )
//                return true;

//            bool contains = false;
//            IPHostEntry iphe = Dns.Resolve( Dns.GetHostName() );

//            for ( int i = 0; !contains && i < iphe.AddressList.Length; ++i )
//                contains = theirAddress.Equals( iphe.AddressList[i] );

//            return contains;
//        }
//    }
//}
