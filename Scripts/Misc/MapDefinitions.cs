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

/* /Scripts/Misc/MapDefinitions.cs
 * ChangeLog
 *  12/23/04, Jade
 *      Changed Ilshenar to a Felucca ruleset.
 *  5/26/04, Old Salty
 * 		Changed Felucca season to 1 (Old Style UO landscape).  
 *	5/21/04, mith
 *		Changed season to 5 (Blooming/Spring). Added comment detailing which numbers = which seasons.
 */

using System;
using Server;

namespace Server.Misc
{
	public class MapDefinitions
	{
		public static void Configure()
		{
			/* Here we configure all maps. Some notes:
			 * 
			 * 1) The first 32 maps are reserved for core use.
			 * 2) Map 0x7F is reserved for core use.
			 * 3) Map 0xFF is reserved for core use.
			 * 4) Changing or removing any predefined maps may cause server instability.
			 */

			RegisterMap( 0, 0, 0, 6144, 4096, 1, "Felucca",  MapRules.FeluccaRules );
			RegisterMap( 1, 1, 0, 6144, 4096, 0, "Trammel",  MapRules.TrammelRules );
			RegisterMap( 2, 2, 2, 2304, 1600, 1, "Ilshenar", MapRules.FeluccaRules );
			RegisterMap( 3, 3, 3, 2560, 2048, 1, "Malas",    MapRules.TrammelRules );

			RegisterMap( 0x7F, 0x7F, 0x7F, Map.SectorSize, Map.SectorSize, 1, "Internal", MapRules.Internal );

			/* Example of registering a custom map:
			 * RegisterMap( 32, 0, 0, 6144, 4096, 3, "Iceland", MapRules.FeluccaRules );
			 * 
			 * Defined:
			 * RegisterMap( <index>, <mapID>, <fileIndex>, <width>, <height>, <season>, <name>, <rules> );
			 *  - <index> : An unreserved unique index for this map
			 *  - <mapID> : An identification number used in client communications. For any visible maps, this value must be from 0-3
			 *  - <fileIndex> : A file identification number. For any visible maps, this value must be 0, 2, or 3
			 *  - <width>, <height> : Size of the map (in tiles)
			 *  - <season> : 0 = spring, 1 = summer, 2 = autumn, 3 = winter, 4 = desolation, 5 = blooming (pre-UO:R Feluca)
			 *  - <name> : Reference name for the map, used in props gump, get/set commands, region loading, etc
			 *  - <rules> : Rules and restrictions associated with the map. See documentation for details
			*/
		}

		public static void RegisterMap( int mapIndex, int mapID, int fileIndex, int width, int height, int season, string name, MapRules rules )
		{
			Map newMap = new Map( mapID, mapIndex, fileIndex, width, height, season, name, rules );

			Map.Maps[mapIndex] = newMap;
			Map.AllMaps.Add( newMap );
		}
	}
}
