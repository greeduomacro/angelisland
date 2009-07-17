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
using Server;

namespace Server.Engines.Mahjong
{
	public class MahjongTileTypeGenerator
	{
		private ArrayList m_LeftTileTypes;

		public ArrayList LeftTileTypes { get { return m_LeftTileTypes; } }

		public MahjongTileTypeGenerator( int count )
		{
			m_LeftTileTypes = new ArrayList( 34 * count );

			for ( int i = 1; i <= 34; i++ )
			{
				for ( int j = 0; j < count; j++ )
				{
					m_LeftTileTypes.Add( (MahjongTileType)i );
				}
			}
		}

		public MahjongTileType Next()
		{
			int random = Utility.Random( m_LeftTileTypes.Count );
			MahjongTileType next = (MahjongTileType)m_LeftTileTypes[random];
			m_LeftTileTypes.RemoveAt( random );

			return next;
		}
	}
} 
