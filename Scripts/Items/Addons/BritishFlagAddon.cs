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

/*	/Scripts/Items/Addons/BritishFlagAddon.cs
 *	12/16/07, plasma
 *		- Added deeds for these addons
 *		- Commented the OnDoubleClick behaviour
 *	9/23/04 Created by smerX
 *
 */
 
using System;
using Server;
using System.Collections;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{

	public class SerpentBannerSouthAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new BritishFlagSouthAddon();
			}
		}

		[Constructable]
		public SerpentBannerSouthAddonDeed()
		{
			Name = "a serpent banner (south)";
		}

		public SerpentBannerSouthAddonDeed(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}


	public class SerpentBannerEastAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new BritishFlagEastAddon();
			}
		}

		[Constructable]
		public SerpentBannerEastAddonDeed()
		{
			Name = "a serpent banner (east)";
		}

		public SerpentBannerEastAddonDeed(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}

	public class BritishFlagSouthAddon : BaseAddon
	{
		[Constructable]
		public BritishFlagSouthAddon()
		{
			AddComponent( new LBFlagSouthTop(), 0, 0, 0 );
			AddComponent( new LBFlagSouthBottom(), 1, 0, 0 );
		}

		public BritishFlagSouthAddon( Serial serial ) : base( serial )
		{
		}

		public override BaseAddonDeed Deed
		{
			get
			{
				return new SerpentBannerSouthAddonDeed();
			}
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
	
	public class BritishFlagEastAddon : BaseAddon
	{
		[Constructable]
		public BritishFlagEastAddon()
		{
			AddComponent( new LBFlagEastTop(), 0, 1, 0 );
			AddComponent( new LBFlagEastBottom(), 0, 0, 0 );
		}

		public BritishFlagEastAddon( Serial serial ) : base( serial )
		{
		}

		public override BaseAddonDeed Deed
		{
			get
			{
				return new SerpentBannerEastAddonDeed();
			}
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
	
	public class LBFlagEastTop : AddonComponent
	{
		[Constructable]
		public LBFlagEastTop() : this( 0x1613 )
		{
		}

		public LBFlagEastTop( int itemID ) : base( itemID )
		{
		}

		public LBFlagEastTop( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{/*
			if ( from.InRange( this, 2 ) )
			{
				from.Criminal = true;
				from.Say( "*Spits on British's flag*" );
			}
			else
			{
				from.SendMessage( "You can't spit that far. " );
			}*/
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

	public class LBFlagEastBottom : AddonComponent
	{
		[Constructable]
		public LBFlagEastBottom() : this( 0x1614 )
		{
		}

		public LBFlagEastBottom( int itemID ) : base( itemID )
		{
		}

		public LBFlagEastBottom( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{/*
			if ( from.InRange( this, 2 ) )
			{
				from.Criminal = true;
				from.Say( "*Spits on British's flag*" );
			}
			else
			{
				from.SendMessage( "You can't spit that far. " );
			}*/
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

	public class LBFlagSouthTop : AddonComponent
	{
		[Constructable]
		public LBFlagSouthTop() : this( 0x15A4 )
		{
		}

		public LBFlagSouthTop( int itemID ) : base( itemID )
		{
		}

		public LBFlagSouthTop( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{/*
			if ( from.InRange( this, 2 ) )
			{
				from.Criminal = true;
				from.Say( "*Spits on British's flag*" );
			}
			else
			{
				from.SendMessage( "You can't spit that far. " );
			}*/
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
		
	public class LBFlagSouthBottom : AddonComponent
	{
		[Constructable]
		public LBFlagSouthBottom() : this( 0x15A5 )
		{
		}

		public LBFlagSouthBottom( int itemID ) : base( itemID )
		{
		}

		public LBFlagSouthBottom( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{/*
			if ( from.InRange( this, 2 ) )
			{
				from.Criminal = true;
				from.Say( "*Spits on British's flag*" );
			}
			else
			{
				from.SendMessage( "You can't spit that far. " );
			}*/
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
}