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
	public class BloodPentagram : BaseAddon
	{
		[Constructable]
		public BloodPentagram ()
		{
			AddComponent( new AddonComponent( 0x1CF9 ), 0, 1, 0 );
			AddComponent( new AddonComponent( 0x1CF8 ), 0, 2, 0 );
			AddComponent( new AddonComponent( 0x1CF7 ), 0, 3, 0 );
			AddComponent( new AddonComponent( 0x1CF6 ), 0, 4, 0 );
			AddComponent( new AddonComponent( 0x1CF5 ), 0, 5, 0 );

			AddComponent( new AddonComponent( 0x1CFB ), 1, 0, 0 );
			AddComponent( new AddonComponent( 0x1CFA ), 1, 1, 0 );
			AddComponent( new AddonComponent( 0x1D09 ), 1, 2, 0 );
			AddComponent( new AddonComponent( 0x1D08 ), 1, 3, 0 );
			AddComponent( new AddonComponent( 0x1D07 ), 1, 4, 0 );
			AddComponent( new AddonComponent( 0x1CF4 ), 1, 5, 0 );

			AddComponent( new AddonComponent( 0x1CFC ), 2, 0, 0 );
			AddComponent( new AddonComponent( 0x1D0A ), 2, 1, 0 );
			AddComponent( new AddonComponent( 0x1D11 ), 2, 2, 0 );
			AddComponent( new AddonComponent( 0x1D10 ), 2, 3, 0 );
			AddComponent( new AddonComponent( 0x1D06 ), 2, 4, 0 );
			AddComponent( new AddonComponent( 0x1CF3 ), 2, 5, 0 );

			AddComponent( new AddonComponent( 0x1CFD ), 3, 0, 0 );
			AddComponent( new AddonComponent( 0x1D0B ), 3, 1, 0 );
			AddComponent( new AddonComponent( 0x1D12 ), 3, 2, 0 );
			AddComponent( new AddonComponent( 0x1D0F ), 3, 3, 0 );
			AddComponent( new AddonComponent( 0x1D05 ), 3, 4, 0 );
			AddComponent( new AddonComponent( 0x1CF2 ), 3, 5, 0 );

			AddComponent( new AddonComponent( 0x1CFE ), 4, 0, 0 );
			AddComponent( new AddonComponent( 0x1D0C ), 4, 1, 0 );
			AddComponent( new AddonComponent( 0x1D0D ), 4, 2, 0 );
			AddComponent( new AddonComponent( 0x1D0E ), 4, 3, 0 );
			AddComponent( new AddonComponent( 0x1D04 ), 4, 4, 0 );
			AddComponent( new AddonComponent( 0x1CF1 ), 4, 5, 0 );

			AddComponent( new AddonComponent( 0x1CFF ), 5, 0, 0 );
			AddComponent( new AddonComponent( 0x1D00 ), 5, 1, 0 );
			AddComponent( new AddonComponent( 0x1D01 ), 5, 2, 0 );
			AddComponent( new AddonComponent( 0x1D02 ), 5, 3, 0 );
			AddComponent( new AddonComponent( 0x1D03 ), 5, 4, 0 );
		}

		public BloodPentagram( Serial serial ) : base( serial )
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