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

/****************************************
 * NAME    : Thick Gray Stone Wall      *
 * SCRIPT  : ThickGrayStoneWall.cs      *
 * VERSION : v1.00                      *
 * CREATOR : Mans Sjoberg (Allmight)    *
 * CREATED : 10-07.2002                 *
 * **************************************/

using System;

namespace Server.Items
{
	public enum ThickGrayStoneWallTypes
	{
		WestArch,
		NorthArch,
		SouthArchTop,
		EastArchTop,
		EastArch,
		SouthArch,
		Wall1,
		Wall2,
		Wall3,
		SouthWindow,
		Wall4,
		EastWindow,
		WestArch2,
		NorthArch2,
		SouthArchTop2,
		EastArchTop2,
		EastArch2,
		SouthArch2,
		SWArchEdge2,
		SouthWindow2,
		NEArchEdge2,
		EastWindow2
	}

	public class ThickGrayStoneWall : BaseWall
	{
		[Constructable]
		public ThickGrayStoneWall( ThickGrayStoneWallTypes type) : base( 0x007A + (int)type )
		{
		}

		public ThickGrayStoneWall( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}�