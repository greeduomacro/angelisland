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
	public class LargeBedEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed{ get{ return new LargeBedEastDeed(); } }

		[Constructable]
		public LargeBedEastAddon()
		{
			AddComponent( new AddonComponent( 0xA7D ), 0, 0, 0 );
			AddComponent( new AddonComponent( 0xA7C ), 0, 1, 0 );
			AddComponent( new AddonComponent( 0xA79 ), 1, 0, 0 );
			AddComponent( new AddonComponent( 0xA78 ), 1, 1, 0 );
		}

		public LargeBedEastAddon( Serial serial ) : base( serial )
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

	public class LargeBedEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon{ get{ return new LargeBedEastAddon(); } }
		public override int LabelNumber{ get{ return 1044324; } } // large bed (east)

		[Constructable]
		public LargeBedEastDeed()
		{
		}

		public LargeBedEastDeed( Serial serial ) : base( serial )
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