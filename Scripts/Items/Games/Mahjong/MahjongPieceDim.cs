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
using Server;

namespace Server.Engines.Mahjong
{
	public struct MahjongPieceDim
	{
		private Point2D m_Position;
		private int m_Width;
		private int m_Height;

		public Point2D Position { get { return m_Position; } }
		public int Width { get { return m_Width; } }
		public int Height { get { return m_Height; } }

		public MahjongPieceDim( Point2D position, int width, int height )
		{
			m_Position = position;
			m_Width = width;
			m_Height = height;
		}

		public bool IsValid()
		{
			return m_Position.X >= 0 && m_Position.Y >= 0 && m_Position.X + m_Width <= 670 && m_Position.Y + m_Height <= 670;
		}

		public bool IsOverlapping( MahjongPieceDim dim )
		{
			return m_Position.X < dim.m_Position.X + dim.m_Width && m_Position.Y < dim.m_Position.Y + dim.m_Height && m_Position.X + m_Width > dim.m_Position.X && m_Position.Y + m_Height > dim.m_Position.Y;
		}

		public int GetHandArea()
		{
			if ( m_Position.X + m_Width > 150 && m_Position.X < 520 && m_Position.Y < 35 )
				return 0;

			if ( m_Position.X + m_Width > 635 && m_Position.Y + m_Height > 150 && m_Position.Y < 520 )
				return 1;

			if ( m_Position.X + m_Width > 150 && m_Position.X < 520 && m_Position.Y + m_Height > 635 )
				return 2;

			if ( m_Position.X < 35 && m_Position.Y + m_Height > 150 && m_Position.Y < 520 )
				return 3;

			return -1;
		}
	}
}�