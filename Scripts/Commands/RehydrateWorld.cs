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

/* Scripts/Commands/RehydrateWorld.cs
 * 	CHANGELOG:
 * 	2/23/06, Adam
 *	Initial Version
 */

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Scripts.Commands;

namespace Server.Scripts.Commands
{
	public class RehydrateWorld
	{
		public static void Initialize()
		{
			Server.Commands.Register( "RehydrateWorld", AccessLevel.Administrator, new CommandEventHandler( RehydrateWorld_OnCommand ) );
		}

		[Usage( "RehydrateWorld" )]
		[Description( "Rehydrates the entire world." )]
		public static void RehydrateWorld_OnCommand( CommandEventArgs e )
		{
			// make it known			
			Server.World.Broadcast( 0x35, true, "The world is rehydrating, please wait." );
			Console.WriteLine( "World: rehydrating..." );
			DateTime startTime = DateTime.Now;

			LogHelper Logger = new LogHelper("RehydrateWorld.log", e.Mobile, true);

			// Extract property & value from command parameters
			ArrayList containers = new ArrayList();

			// Loop items and check vs. types
			foreach ( Item item in World.Items.Values )
			{
				if (item is Container)
				{
					if ((item as Container).CanFreezeDry == true)
						containers.Add(item);
				}
			}

			Logger.Log(LogType.Text,  
				string.Format("{0} containers scheduled for Rehydration...", 
					containers.Count) );	

			int count=0;
			for ( int ix=0; ix < containers.Count; ix++ )
			{
				Container cont = containers[ix] as Container;

				if( cont != null )
				{
					// Rehydrate it if necessary
					if( cont.CanFreezeDry && cont.IsFreezeDried == true)
						cont.Rehydrate();
					count++;
				}
			}

			Logger.Log(LogType.Text,  
				string.Format("{0} containers actually Rehydrated",	count) );	

			Logger.Finish();

			e.Mobile.SendMessage( "{0} containers actually Rehydrated",	count );

			DateTime endTime = DateTime.Now;
			Console.WriteLine( "done in {0:F1} seconds.", (endTime - startTime).TotalSeconds );
			Server.World.Broadcast( 0x35, true, "World rehydration complete. The entire process took {0:F1} seconds.", (endTime - startTime).TotalSeconds );
		}
	}
}