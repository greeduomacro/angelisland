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
	public class ShrineOfWisdomAddon : BaseAddon
	{
		[Constructable]
		public ShrineOfWisdomAddon()
		{
			AddComponent( new ShrineOfWisdomComponent( 0x14C3 ), 0, 0, 0 );
			AddComponent( new ShrineOfWisdomComponent( 0x14C6 ), 1, 0, 0 );
			AddComponent( new ShrineOfWisdomComponent( 0x14D4 ), 0, 1, 0 );
			AddComponent( new ShrineOfWisdomComponent( 0x14D5 ), 1, 1, 0 );
			Hue = 0x47E;
		}

		public ShrineOfWisdomAddon( Serial serial ) : base( serial )
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

	[Server.Engines.Craft.Forge]
	public class ShrineOfWisdomComponent : AddonComponent
	{
		public override int LabelNumber{ get{ return 1062046; } } // Shrine of Wisdom

		[Constructable]
		public ShrineOfWisdomComponent( int itemID ) : base( itemID )
		{
		}

		public ShrineOfWisdomComponent( Serial serial ) : base( serial )
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