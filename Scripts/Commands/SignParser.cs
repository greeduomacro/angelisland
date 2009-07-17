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
using System.Collections;
using System.IO;
using Server;
using Server.Items;

namespace Server.Scripts.Commands
{
	public class SignParser
	{
		private class SignEntry
		{
			public string m_Text;
			public Point3D m_Location;
			public int m_ItemID;
			public int m_Map;

			public SignEntry( string text, Point3D pt, int itemID, int mapLoc )
			{
				m_Text = text;
				m_Location = pt;
				m_ItemID = itemID;
				m_Map = mapLoc;
			}
		}

		public static void Initialize()
		{
			Server.Commands.Register( "SignGen", AccessLevel.Administrator, new CommandEventHandler( SignGen_OnCommand ) );
		}

		[Usage( "SignGen" )]
		[Description( "Generates world/shop signs on all facets." )]
		public static void SignGen_OnCommand( CommandEventArgs c )
		{
			Parse( c.Mobile );
		}

		public static void Parse( Mobile from )
		{
			string cfg = Path.Combine( Core.BaseDirectory, "Data/signs.cfg" );

			if ( File.Exists( cfg ) )
			{
				ArrayList list = new ArrayList();
				from.SendMessage( "Generating signs, please wait." );

				using ( StreamReader ip = new StreamReader( cfg ) )
				{
					string line;

					while ( (line = ip.ReadLine()) != null )
					{
						string[] split = line.Split( ' ' );

						SignEntry e = new SignEntry(
							line.Substring( split[0].Length + 1 + split[1].Length + 1 + split[2].Length + 1 + split[3].Length + 1 + split[4].Length + 1 ),
							new Point3D( Utility.ToInt32( split[2] ), Utility.ToInt32( split[3] ), Utility.ToInt32( split[4] ) ),
							Utility.ToInt32( split[1] ), Utility.ToInt32( split[0] ) );

						list.Add( e );
					}
				}

				Map[] brit = new Map[]{ Map.Felucca, Map.Trammel };
				Map[] fel = new Map[]{ Map.Felucca };
				Map[] tram = new Map[]{ Map.Trammel };
				Map[] ilsh = new Map[]{ Map.Ilshenar };
				Map[] malas = new Map[]{ Map.Malas };

				for ( int i = 0; i < list.Count; ++i )
				{
					SignEntry e = (SignEntry)list[i];
					Map[] maps = null;

					switch ( e.m_Map )
					{
						case 0: maps = brit; break; // Trammel and Felucca
						case 1: maps = fel; break;  // Felucca
						case 2: maps = tram; break; // Trammel
						case 3: maps = ilsh; break; // Ilshenar
						case 4: maps = malas; break; // Malas
					}

					for ( int j = 0; maps != null && j < maps.Length; ++j )
						Add_Static( e.m_ItemID, e.m_Location, maps[j], e.m_Text );
				}

				from.SendMessage( "Sign generating complete." );
			}
			else
			{
				from.SendMessage( "{0} not found!", cfg );
			}
		}

		private static Queue m_ToDelete = new Queue();

		public static void Add_Static( int itemID, Point3D location, Map map, string name )
		{
			IPooledEnumerable eable = map.GetItemsInRange( location, 0 );

			foreach ( Item item in eable )
			{
				if ( item is Sign && item.Z == location.Z && item.ItemID == itemID )
					m_ToDelete.Enqueue( item );
			}

			eable.Free();

			while ( m_ToDelete.Count > 0 )
				((Item)m_ToDelete.Dequeue()).Delete();

			Item sign;

			if ( name.StartsWith( "#" ) )
			{
				sign = new LocalizedSign( itemID, Utility.ToInt32( name.Substring( 1 ) ) );
			}
			else
			{
				sign = new Sign( itemID );
				sign.Name = name;
			}

			if ( map == Map.Malas )
			{
				if ( location.X >= 965 && location.Y >= 502 && location.X <= 1012 && location.Y <= 537 )
					sign.Hue = 0x47E;
				else if ( location.X >= 1960 && location.Y >= 1278 && location.X < 2106 && location.Y < 1413 )
					sign.Hue = 0x44E;
			}

			sign.MoveToWorld( location, map );
		}
	}
} 
