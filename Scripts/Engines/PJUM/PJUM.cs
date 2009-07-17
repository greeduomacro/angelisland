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

/* Scripts/Engines/PJUM/PJUM.cs
 * CHANGELOG:
 *	02/11/06, Adam
 *		Make common the formatting of sextant coords.
 *	01/13/06, Pix
 *		New file - result from separation of TCCS and PJUM systems
 */

using System;
using Server;
using Server.Engines;
using Server.Items;

namespace Server.PJUM
{
	/// <summary>
	/// Summary description for PJUM.
	/// </summary>
	public class PJUM
	{
		//public PJUM()
		//{
		//}

		public static void Initialize()
		{
			Timer.DelayCall( TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), new TimerCallback( PJUM_Work ) );
		}

		public static void PJUM_Work()
		{
			try
			{
				PJUM.MakeAllEntriesCriminal();
				PJUM.UpdateLocations();
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}

		public static bool HasBeenReported( Mobile m )
		{
			for(int i=0; i<TCCS.TheList.Count; i++)
			{
				if( ((ListEntry)TCCS.TheList[i]).Mobile == m )
				{	
					return true;
				}
			}
			return false;
		}

		public static void AddMacroer(string[] lines, Mobile m, DateTime dt)
		{
			ListEntry le = new ListEntry(lines, m, dt, ListEntryType.PJUM);
			TCCS.AddEntry( le );
			PJUM.MakeAllEntriesCriminal();
		}


		public static void UpdateLocations()
		{
			foreach( ListEntry le in TCCS.TheList )
			{
				if( le == null ) continue;
				if( le.Type != ListEntryType.PJUM ) continue;

				string location;
				int xLong = 0, yLat = 0, xMins = 0, yMins = 0;
				bool xEast = false, ySouth = false;
				Map map = le.Mobile.Map;
				bool valid = Sextant.Format( le.Mobile.Location, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth );

				if ( valid )
					location = Sextant.Format( xLong, yLat, xMins, yMins, xEast, ySouth );
				else
					location = "????";

				if( !valid )
					location = string.Format("{0} {1}", le.Mobile.X, le.Mobile.Y );

				if ( map != null )
				{
					Region reg = le.Mobile.Region;

					if ( reg != map.DefaultRegion )
					{
						location += (" in " + reg);
					}
				}

				le.Lines[1] = String.Format("{0} was last seen at {1}.", le.Mobile.Name, location);
			}

		}

		public static void MakeAllEntriesCriminal()
		{
			try //safety net
			{
				if( TCCS.TheList != null )
				{
					if( TCCS.TheList.Count > 0 )
					{
						for(int i=0; i<TCCS.TheList.Count; i++)
						{
							ListEntry entry = (ListEntry)TCCS.TheList[i];
							if( entry.Type == ListEntryType.PJUM && entry.Enabled )
							{
								if( entry.Mobile != null )
								{
									entry.Mobile.Criminal = true;
								}
							}
						}
					}
				}
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
		}

	}
}
