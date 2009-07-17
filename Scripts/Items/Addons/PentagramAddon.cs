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

/* Scripts/Items/Addons/PentagramAddon.cs
 * ChangeLog
 *	06/06/06, Adam
 *		Add AddComponent override to allow the components being added to be invisible.
 */

using System;
using Server;

namespace Server.Items
{
	public class PentagramAddon : BaseAddon
	{
		public override BaseAddonDeed Deed{ get{ return new PentagramDeed(); } }
		private bool m_quiet;

		[Constructable]
		public PentagramAddon()
		{
			AddComponent( new AddonComponent( 0xFE7 ), -1, -1, 0 );
			AddComponent( new AddonComponent( 0xFE8 ),  0, -1, 0 );
			AddComponent( new AddonComponent( 0xFEB ),  1, -1, 0 );
			AddComponent( new AddonComponent( 0xFE6 ), -1,  0, 0 );
			AddComponent( new AddonComponent( 0xFEA ),  0,  0, 0 );
			AddComponent( new AddonComponent( 0xFEE ),  1,  0, 0 );
			AddComponent( new AddonComponent( 0xFE9 ), -1,  1, 0 );
			AddComponent( new AddonComponent( 0xFEC ),  0,  1, 0 );
			AddComponent( new AddonComponent( 0xFED ),  1,  1, 0 );
		}

		[Constructable]
		public PentagramAddon(bool bQuiet)
		{
			m_quiet = bQuiet;

			AddComponent( new AddonComponent( 0xFE7 ), -1, -1, 0 );
			AddComponent( new AddonComponent( 0xFE8 ),  0, -1, 0 );
			AddComponent( new AddonComponent( 0xFEB ),  1, -1, 0 );
			AddComponent( new AddonComponent( 0xFE6 ), -1,  0, 0 );
			AddComponent( new AddonComponent( 0xFEA ),  0,  0, 0 );
			AddComponent( new AddonComponent( 0xFEE ),  1,  0, 0 );
			AddComponent( new AddonComponent( 0xFE9 ), -1,  1, 0 );
			AddComponent( new AddonComponent( 0xFEC ),  0,  1, 0 );
			AddComponent( new AddonComponent( 0xFED ),  1,  1, 0 );
		}

		public PentagramAddon( Serial serial ) : base( serial )
		{
		}

		public override void AddComponent( AddonComponent c, int x, int y, int z )
		{
			if ( Deleted )
				return;

			base.AddComponent( c, x, y, z );

			c.Visible = (m_quiet == true) ? false : true;
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

	public class PentagramDeed : BaseAddonDeed
	{
		public override BaseAddon Addon{ get{ return new PentagramAddon(); } }
		public override int LabelNumber{ get{ return 1044328; } } // pentagram

		[Constructable]
		public PentagramDeed()
		{
		}

		public PentagramDeed( Serial serial ) : base( serial )
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