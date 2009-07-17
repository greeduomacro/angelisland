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

/* Scripts/Regions/Felucca/Town.cs
 * ChangeLog
 *	04/24/09, plasma
 *		Commented out all regions, replaced with DRDT
 *	2/2/06, Adam
 *		Remove Yew from the guarded regions
 *	9/16/05, Adam
 *		Remove Vesper from the guarded regions
 *	4/11/05, Adam
 *		Remove Ocllo from the guarded regions
 *	9/17/04, Adam
 *		Remove Serpent's Hold from the guarded regions
 *	3/26/04 changes by mith
 *		Initialize():  Removed AngelIsland initialization and placed new Initialize event in AngelIsland.cs.
 *	3/15/04 changes by mith	
 *		Initialize(): Added new GuardedRegion, AngelIsland, based on definition in Regions.xml.
 */

using System;
using System.Collections;
using Server;
using Server.Mobiles;
using Server.Spells;

namespace Server.Regions
{
	public class FeluccaTown : GuardedRegion
	{
		public static new void Initialize()
		{
			//replaced with dynamic region(s)
			/*
			Region.AddRegion( new FeluccaTown( "Cove" ) );
			Region.AddRegion( new FeluccaTown( "Britain" ) );
			Region.AddRegion( new FeluccaTown( "Minoc" ) );
			Region.AddRegion( new FeluccaTown( "Trinsic" ) );
			Region.AddRegion( new FeluccaTown( "Skara Brae" ) );
			Region.AddRegion( new FeluccaTown( "Nujel'm" ) );
			Region.AddRegion( new FeluccaTown( "Moonglow" ) );
			Region.AddRegion( new FeluccaTown( "Magincia" ) );
			Region.AddRegion( new FeluccaTown( "Delucia" ) );
			Region.AddRegion( new FeluccaTown( "Papua" ) );
			*/
			
			// Region.AddRegion(new FeluccaTown("Jhelom"));
			//Region.AddRegion( GuardedRegion.Disable( new FeluccaTown( "Wind" ) ) );
			//Region.AddRegion( GuardedRegion.Disable( new FeluccaTown( "Serpent's Hold" ) ) );
			//Region.AddRegion( GuardedRegion.Disable( new FeluccaTown( "Buccaneer's Den" ) ) );
			//Region.AddRegion( GuardedRegion.Disable( new FeluccaTown( "Ocllo" ) ) );
			//Region.AddRegion( GuardedRegion.Disable( new FeluccaTown( "Vesper" ) ) );
			//Region.AddRegion( GuardedRegion.Disable( new FeluccaTown( "Yew" ) ) );

			//Region.AddRegion( new GuardedRegion( "", "Moongates", Map.Felucca, typeof( WarriorGuard ) ) );
		}

		public FeluccaTown( string name ) : this( name, typeof( WarriorGuard ) )
		{
		}

		public FeluccaTown( string name, Type guardType ) : base( "the town of", name, Map.Felucca, guardType )
		{
		}
	}
}