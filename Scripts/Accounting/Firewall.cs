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

/* Accounting/Firewall.cs
 * CHANGELOG:
 *	2/15/05 - Pix
 *		Initial version for 1.0.0
 */

using System;
using System.IO;
using System.Collections;
using System.Net;

namespace Server
{
	public class Firewall
	{
		private static ArrayList m_Blocked;

		static Firewall()
		{
			m_Blocked = new ArrayList();

			string path = "firewall.cfg";

			if ( File.Exists( path ) )
			{
				using ( StreamReader ip = new StreamReader( path ) )
				{
					string line;

					while ( (line = ip.ReadLine()) != null )
					{
						line = line.Trim();

						if ( line.Length == 0 )
							continue;

						object toAdd;

						try{ toAdd = IPAddress.Parse( line ); }
						catch{ toAdd = line; }

						m_Blocked.Add( toAdd.ToString() );
					}
				}
			}
		}

		public static ArrayList List
		{
			get
			{
				return m_Blocked;
			}
		}

		public static void RemoveAt( int index )
		{
			m_Blocked.RemoveAt( index );
			Save();
		}

		public static void Remove( string pattern )
		{
			m_Blocked.Remove( pattern );
			Save();
		}

		public static void Remove( IPAddress ip )
		{
			m_Blocked.Remove( ip );
			Save();
		}

		public static void Add( object obj )
		{
			if ( !(obj is IPAddress) && !(obj is String) )
				return;

			if ( !m_Blocked.Contains( obj ) )
				m_Blocked.Add( obj );

			Save();
		}

		public static void Add( string pattern )
		{
			if ( !m_Blocked.Contains( pattern ) )
				m_Blocked.Add( pattern );

			Save();
		}

		public static void Add( IPAddress ip )
		{
			if ( !m_Blocked.Contains( ip ) )
				m_Blocked.Add( ip );

			Save();
		}

		public static void Save()
		{
			string path = "firewall.cfg";

			using ( StreamWriter op = new StreamWriter( path ) )
			{
				for ( int i = 0; i < m_Blocked.Count; ++i )
					op.WriteLine( m_Blocked[i] );
			}
		}

		public static bool IsBlocked( IPAddress ip )
		{
			bool contains = false;

			for ( int i = 0; !contains && i < m_Blocked.Count; ++i )
			{
				if ( m_Blocked[i] is IPAddress )
					contains = ip.Equals( m_Blocked[i] );
				else if ( m_Blocked[i] is String )
					contains = Utility.IPMatch( (string)m_Blocked[i], ip );
			}

			return contains;
		}
	}
}