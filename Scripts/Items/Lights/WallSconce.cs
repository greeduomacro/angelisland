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

namespace Server.Items
{
	[Flipable]
	public class WallSconce : BaseLight
	{
		public override int LitItemID
		{
			get
			{
				if ( ItemID == 0x9FB )
					return 0x9FD;
				else
					return 0xA02;
			}
		}
		
		public override int UnlitItemID
		{
			get
			{
				if ( ItemID == 0x9FD )
					return 0x9FB;
				else
					return 0xA00;
			}
		}
		
		[Constructable]
		public WallSconce() : base( 0x9FB )
		{
			Movable = false;
			Duration = TimeSpan.Zero; // Never burnt out
			Burning = false;
			Light = LightType.WestBig;
			Weight = 3.0;
		}

		public WallSconce( Serial serial ) : base( serial )
		{
		}

		public void Flip()
		{
			if ( Light == LightType.WestBig )
				Light = LightType.NorthBig;
			else if ( Light == LightType.NorthBig )
				Light = LightType.WestBig;

			switch ( ItemID )
			{
				case 0x9FB: ItemID = 0xA00; break;
				case 0x9FD: ItemID = 0xA02; break;

				case 0xA00: ItemID = 0x9FB; break;
				case 0xA02: ItemID = 0x9FD; break;
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}�