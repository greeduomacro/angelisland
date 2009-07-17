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
 /* Scripts/Items/Addons/LargeEastForgeAddon.cs
 * ChangeLog
 *  7/30/06, Kit
 *		Add Forge Bellows, Rise of the animated forges!
 */

using System;
using Server;
using Server.Scripts.Commands;

namespace Server.Items
{

    public class LargeForgeEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed{ get{ return new LargeForgeEastDeed(); } }

		[Constructable]
		public LargeForgeEastAddon()
		{
			AddComponent( new ForgeBellows(6534), 0, 0, 0 );
			AddComponent( new ForgeComponent( 0x198A ), 0, 1, 0 );
			AddComponent( new ForgeComponent( 0x1996 ), 0, 2, 0 );
			AddComponent( new ForgeBellows( 6546 ), 0, 3, 0 );
		}

		public LargeForgeEastAddon( Serial serial ) : base( serial )
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
			//reset all graphics back to base animation, incase we saved and crashed, or went down
			//before bellow timer could reset graphics.
			try
			{
				((AddonComponent)this.Components[0]).ItemID = 6534;
				((AddonComponent)this.Components[1]).ItemID = 0x198A;
				((AddonComponent)this.Components[2]).ItemID = 0x1996;
				((AddonComponent)this.Components[3]).ItemID = 6546;
			}
			catch(Exception exc)
			{
				LogHelper.LogException(exc);
				Console.WriteLine("Exception caught in Large East forge addon Deserialization");
				System.Console.WriteLine(exc.Message);
				System.Console.WriteLine(exc.StackTrace);
			}
		}

	}
	
	public class LargeForgeEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon{ get{ return new LargeForgeEastAddon(); } }
		public override int LabelNumber{ get{ return 1044331; } } // large forge (east)

		[Constructable]
		public LargeForgeEastDeed()
		{
		}

		public LargeForgeEastDeed( Serial serial ) : base( serial )
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