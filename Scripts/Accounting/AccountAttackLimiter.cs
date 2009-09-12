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
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Server;
using Server.Network;

namespace Server.Accounting
{
	public class AccountAttackLimiter
	{
		public static bool Enabled = true;

		public static void Initialize()
		{
			if (!Enabled)
				return;

			RegisterThrottler(0x80);
			RegisterThrottler(0x91);
			RegisterThrottler(0xCF);
		}

		public static void RegisterThrottler(int packetID)
		{
			PacketHandler ph = PacketHandlers.GetHandler(packetID);

			if (ph == null)
				return;

			ph.ThrottleCallback = new ThrottlePacketCallback(Throttle_Callback);
		}

		public static bool Throttle_Callback(NetState ns)
		{
			InvalidAccountAccessLog accessLog = FindAccessLog(ns);

			if (accessLog == null)
				return true;

			return (DateTime.Now >= (accessLog.LastAccessTime + ComputeThrottle(accessLog.Counts)));
		}

		private static ArrayList m_List = new ArrayList();

		public static InvalidAccountAccessLog FindAccessLog(NetState ns)
		{
			if (ns == null)
				return null;

			IPAddress ipAddress = ns.Address;

			for (int i = 0; i < m_List.Count; ++i)
			{
				InvalidAccountAccessLog accessLog = (InvalidAccountAccessLog)m_List[i];

				if (accessLog.HasExpired)
					m_List.RemoveAt(i--);
				else if (accessLog.Address.Equals(ipAddress))
					return accessLog;
			}

			return null;
		}

		public static void RegisterInvalidAccess(NetState ns)
		{
			if (ns == null || !Enabled)
				return;

			InvalidAccountAccessLog accessLog = FindAccessLog(ns);

			if (accessLog == null)
				m_List.Add(accessLog = new InvalidAccountAccessLog(ns.Address));

			accessLog.Counts += 1;
			accessLog.RefreshAccessTime();
		}

		public static TimeSpan ComputeThrottle(int counts)
		{
			if (counts >= 15)
				return TimeSpan.FromMinutes(5.0);

			if (counts >= 10)
				return TimeSpan.FromMinutes(1.0);

			if (counts >= 5)
				return TimeSpan.FromSeconds(20.0);

			if (counts >= 3)
				return TimeSpan.FromSeconds(10.0);

			if (counts >= 1)
				return TimeSpan.FromSeconds(2.0);

			return TimeSpan.Zero;
		}
	}

	public class InvalidAccountAccessLog
	{
		private IPAddress m_Address;
		private DateTime m_LastAccessTime;
		private int m_Counts;

		public IPAddress Address
		{
			get { return m_Address; }
			set { m_Address = value; }
		}

		public DateTime LastAccessTime
		{
			get { return m_LastAccessTime; }
			set { m_LastAccessTime = value; }
		}

		public bool HasExpired
		{
			get { return (DateTime.Now >= (m_LastAccessTime + TimeSpan.FromHours(1.0))); }
		}

		public int Counts
		{
			get { return m_Counts; }
			set { m_Counts = value; }
		}

		public void RefreshAccessTime()
		{
			m_LastAccessTime = DateTime.Now;
		}

		public InvalidAccountAccessLog(IPAddress address)
		{
			m_Address = address;
			RefreshAccessTime();
		}
	}
}