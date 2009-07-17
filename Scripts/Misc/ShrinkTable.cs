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
using System.IO;

namespace Server
{
	public class ShrinkTable
	{
		private const int DefaultItemID = 0x1870; // Yellow virtue stone

		private static int[] m_Table;

		public static int Lookup( Mobile m )
		{
			return Lookup( m.Body.BodyID, DefaultItemID );
		}

		public static int Lookup( int body )
		{
			return Lookup( body, DefaultItemID );
		}

		public static int Lookup( Mobile m, int defaultValue )
		{
			return Lookup( m.Body.BodyID, defaultValue );
		}

		public static int Lookup( int body, int defaultValue )
		{
			if ( m_Table == null )
				Load();

			int val = 0;

			if ( body >= 0 && body < m_Table.Length )
				val = m_Table[body];

			if ( val == 0 )
				val = defaultValue;

			return val;
		}

		private static void Load()
		{
			string path = Path.Combine( Core.BaseDirectory, "Data/shrink.cfg" );

			if ( !File.Exists( path ) )
			{
				m_Table = new int[0];
				return;
			}

			m_Table = new int[1000];

			using ( StreamReader ip = new StreamReader( path ) )
			{
				string line;

				while ( (line = ip.ReadLine()) != null )
				{
					line = line.Trim();

					if ( line.Length == 0 || line.StartsWith( "#" ) )
						continue;

					try
					{
						string[] split = line.Split( '\t' );

						if ( split.Length >= 2 )
						{
							int body = Utility.ToInt32( split[0] );
							int item = Utility.ToInt32( split[1] );

							if ( body >= 0 && body < m_Table.Length )
								m_Table[body] = item;
						}
					}
					catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
				}
			}
		}
	}
}