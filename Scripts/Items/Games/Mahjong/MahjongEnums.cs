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

namespace Server.Engines.Mahjong
{
	public enum MahjongPieceDirection
	{
		Up,
		Left,
		Down,
		Right
	}

	public enum MahjongWind
	{
		North,
		East,
		South,
		West
	}

	public enum MahjongTileType
	{
		Dagger1 = 1,
		Dagger2,
		Dagger3,
		Dagger4,
		Dagger5,
		Dagger6,
		Dagger7,
		Dagger8,
		Dagger9,
		Gem1,
		Gem2,
		Gem3,
		Gem4,
		Gem5,
		Gem6,
		Gem7,
		Gem8,
		Gem9,
		Number1,
		Number2,
		Number3,
		Number4,
		Number5,
		Number6,
		Number7,
		Number8,
		Number9,
		North,
		East,
		South,
		West,
		Green,
		Red,
		White
	}
} 
