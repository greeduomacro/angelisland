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

namespace Server.PathAlgorithms
{
	public abstract class PathAlgorithm
	{
		public abstract bool CheckCondition( Mobile m, Map map, Point3D start, Point3D goal );
		public abstract Direction[] Find( Mobile m, Map map, Point3D start, Point3D goal );

		private static Direction[] m_CalcDirections = new Direction[9]
			{
				Direction.Up,
				Direction.North,
				Direction.Right,
				Direction.West,
				Direction.North,
				Direction.East,
				Direction.Left,
				Direction.South,
				Direction.Down
			};

		public Direction GetDirection( int xSource, int ySource, int xDest, int yDest )
		{
			int x = xDest + 1 - xSource;
			int y = yDest + 1 - ySource;
			int v = (y * 3) + x;

			if ( v < 0 || v >= 9 )
				return Direction.North;

			return m_CalcDirections[v];
		}
	}
}�