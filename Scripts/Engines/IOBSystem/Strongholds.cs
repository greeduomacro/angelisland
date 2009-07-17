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

/* /Scripts/Engines/IOBSystem/Strongholds.cs
 * CHANGELOG:
 *	5/020/05, Kit
 *		removed wind and occolo from stronghold list as they are to be added with DRDT
 *	3/31/05: Pix
 *		Initial Version
 */

using System;
using Server;

namespace Server.Engines.IOBSystem
{
	public class Strongholds
	{
		public static bool IsInIOBStronghold(Mobile m)
		{
			return IsInIOBStronghold( m.Location.X, m.Location.Y );
		}

		public static bool IsInIOBStronghold(Point3D p)
		{
			return IsInIOBStronghold( p.X, p.Y );
		}

		public static bool IsInIOBStronghold(int x, int y)
		{
			//Orc : yew fort + ~3 screens
			bool orc = (x >= 557 && y >= 1424 && x < 736 && y < 1553);

			//Savage : all 4 buildings + 1 screen
			bool savage = (x >= 918 && y >= 664 && x < 1041 && y < 849);
			
			//Pirate : all of buc's island
			bool pirate = (x >= 2557 && y >= 1933 && x < 2883 && y < 2362);

			//Brigand : all of serp's island
			bool brigand = (x >= 2761 && y >= 3329 && x < 3081 && y < 3632);

			//Council : all of Wind dungeon
			bool council = (x >= 5120 && y >= 7 && x < 5368 && y < 254);
			
			//undead : all of deceit
			bool undead = (x >= 5122 && y >= 518 && x < 5370 && y < 770);

			//ocllo : all of ocllo island
			bool ocllo = (x >= 3304 && y >= 2340 && x < 3845 && y < 2977);

			return ( orc || savage || pirate || brigand || council || undead || ocllo);
		}
	}
}
