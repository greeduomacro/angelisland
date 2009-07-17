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

namespace Server.Engines.Craft
{
	public class CraftGroupCol : System.Collections.CollectionBase
	{
		public CraftGroupCol()
		{
		}

		public int Add( CraftGroup craftGroup )
		{
			return List.Add( craftGroup );
		}

		public void Remove( int index )
		{
			if ( index > Count - 1 || index < 0 )
			{
			}
			else
			{
				List.RemoveAt( index );
			}
		}

		public CraftGroup GetAt( int index )
		{
			return ( CraftGroup ) List[index];
		}

		public int SearchFor( int groupName )
		{
			for ( int i = 0; i < List.Count; i++ )
			{
				CraftGroup craftGroup = (CraftGroup)List[i];

				if ( craftGroup.NameNumber == groupName )
					return i;
			}

			return -1;
		}

		public int SearchFor( string groupName )
		{
			for ( int i = 0; i < List.Count; i++ )
			{
				CraftGroup craftGroup = (CraftGroup)List[i];

				if ( craftGroup.NameString == groupName )
					return i;
			}

			return -1;
		}
	}
} 
