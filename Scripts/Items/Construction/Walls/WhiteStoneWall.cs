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
 * NAME    : White Stone Wall           *
 * SCRIPT  : WhiteStoneWall.cs          *
 * VERSION : v1.00                      *
 * CREATOR : Mans Sjoberg (Allmight)    *
 * CREATED : 10-07.2002                 *
 * **************************************/

using System;

namespace Server.Items
{
	public enum WhiteStoneWallTypes
	{
		EastWall,
		SouthWall,
		SECorner,
		NWCornerPost,
		EastArrowLoop,
		SouthArrowLoop,
		EastWindow,
		SouthWindow,
		SouthWallMedium,
		EastWallMedium,
		SECornerMedium,
		NWCornerPostMedium,
		SouthWallShort,
		EastWallShort,
		SECornerShort,
		NWCornerPostShort,
		NECornerPostShort,
		SWCornerPostShort,
		SouthWallVShort,
		EastWallVShort,
		SECornerVShort,
		NWCornerPostVShort,
		SECornerArch,
		SouthArch,
		WestArch,
		EastArch,
		NorthArch,
		EastBattlement,
		SECornerBattlement,
		SouthBattlement,
		NECornerBattlement,
		SWCornerBattlement,
		Column,
		SouthWallVVShort,
		EastWallVVShort
	}

	public class WhiteStoneWall : BaseWall
	{
		[Constructable]
		public WhiteStoneWall( WhiteStoneWallTypes type) : base( 0x0057 + (int)type )
		{
		}

		public WhiteStoneWall( Serial serial ) : base( serial )
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
} 
