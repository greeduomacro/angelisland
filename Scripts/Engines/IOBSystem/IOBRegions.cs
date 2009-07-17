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

/* /Scripts/Engines/IOBSystem/IOBRegions.cs
 * CHANGELOG:
 *	5/27/05, Adam
 *		enhance debug message to show e.StackTrace
 *	5/25/05, Kit
 *		Added additional debug output for function IsInIOBREgion that is throwing exceptions
 *	5/04/05, Kit
 *		Added additional null checks and try catchs to drdt code
 *	5/03/05, Kit
 *		Revamped checks for DRDT regions to use new FindDRDTRegion() to allow iob functionality to continue
 *		when entering into a houseing region.
 * 	5/02/05, Kit
 *		Added generic check for DRDT iob regions to first return one vs a hardcoded iobzone
 *	4/13/05, Adam
 *		Add a family of Stronghold functions to complement the region 
 *		functions. (I.e., strongholds don't include Ocllo, but regions do.)
 *	3/31/05, Pix
 *		Change IOBStronghold to IOBRegion
 *	3/31/05: Pix
 *		Initial Version
 */

using System;
using Server;
using Server.Regions;
using Server.Items;
using System.Collections;
using Server.Scripts.Commands;

namespace Server.Engines.IOBSystem
{
	public class IOBRegions
	{
		public static bool IsInIOBRegion(Mobile m)
		{
			try
			{
				RegionControl regstone = null;
				//check if there is a DRDT region and also if were in a house and if so use the DRDT rules
				CustomRegion inHouse = null;
				inHouse = CustomRegion.FindDRDTRegion(m);
				if(inHouse !=null ) 
					regstone = inHouse.GetRegionControler(); 
				
				if( (regstone != null && regstone.IOBAlign != IOBAlignment.None) || (regstone != null && regstone.IOBZone) ) 
					return true;
			}
			catch(NullReferenceException e)
			{
				LogHelper.LogException(e);
				Console.WriteLine("{0}", e); 
			}
			catch
			{
				Console.WriteLine("Caught Exception in IOBRegions::IsInIOBRegion()"); 
			}

			return IsInIOBRegion( m.Location.X, m.Location.Y );
		}

		public static bool IsInIOBRegion(Point3D p)
		{
			return IsInIOBRegion( p.X, p.Y );
		}

		public static bool IsInIOBRegion(int x, int y)
		{
			if (GetIOBStronghold( x, y ) != IOBAlignment.None)
				return true;

			//ocllo : all of ocllo island
			if (x >= 3304 && y >= 2340 && x < 3845 && y < 2977)
				return true;

			return ( false );
		}

		public static IOBAlignment GetIOBStronghold(Mobile m)
		{
			RegionControl regstone = null;
			//check if there is a DRDT region and also if were in a house and if so use the DRDT rules
			CustomRegion inHouse = null;
			inHouse = CustomRegion.FindDRDTRegion(m);

			if(inHouse !=null ) 
				regstone = inHouse.GetRegionControler(); 

			if(regstone != null && regstone.IOBAlign != IOBAlignment.None)
				return regstone.IOBAlign;

			return GetIOBStronghold(m.Location.X, m.Location.Y );
		}

		public static IOBAlignment GetIOBStronghold(Point3D p)
		{
			return GetIOBStronghold( p.X, p.Y );
		}

		public static IOBAlignment GetIOBStronghold(int x, int y)
		{

			IOBAlignment alignment = IOBAlignment.None;

			//Orc : yew fort + ~3 screens
			if (x >= 557 && y >= 1424 && x < 736 && y < 1553)
				alignment = IOBAlignment.Orcish;

			//Savage : all 4 buildings + 1 screen
			if (x >= 918 && y >= 664 && x < 1041 && y < 849)
				alignment = IOBAlignment.Savage;
			
			//Pirate : all of buc's island
			if (x >= 2557 && y >= 1933 && x < 2883 && y < 2362)
				alignment = IOBAlignment.Pirate;

			//Brigand : all of serp's island
			if (x >= 2761 && y >= 3329 && x < 3081 && y < 3632)
				alignment = IOBAlignment.Brigand;

			//Council : all of Wind dungeon
			if (x >= 5120 && y >= 7 && x < 5368 && y < 254)
				alignment = IOBAlignment.Council;
			
			//undead : all of deceit
			if (x >= 5122 && y >= 518 && x < 5370 && y < 770)
				alignment = IOBAlignment.Undead;

			return ( alignment );
		}
	}
}
