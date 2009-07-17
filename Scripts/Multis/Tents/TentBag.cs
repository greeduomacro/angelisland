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

/* /Scripts/Multis/Tent/TentBag.cs
 * ChangeLog:
 *	08/03/06, weaver
 *		Added placement confirmation gump.
 *	07/22/06, weaver
 *		Made tent bags newbied on construction.
 *		Changed placement rules to equivalent of basic houses.
 *		Removed now obsolete HasAccountTent() function.
 *	05/23/06, weaver
 *		Removed 60 second placement delay message.
 *	05/07/06, weaver
 *		Renamed to "a rolled up tent".
 *	05/01/06, weaver
 *		Initial creation.
 */

using System;
using Server.Accounting;
using System.Collections;

using Server;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Multis.Deeds;

public class TentBag : HouseDeed, IDyable
{ 
	[Constructable] 
	public TentBag() : base( 2648, 0x3FFE, new Point3D( 0, 4, 0 ) )
	{ 
		Name = "a rolled up tent";
		Weight = 20.0; 
                LootType = LootType.Newbied;
	} 

	// Implement Dye() member... tent roofs reflect dyed bag colour

	public virtual bool Dye(Mobile from, DyeTub sender)
	{
		if( Deleted )
			return false;
		else if( RootParent is Mobile && from != RootParent )
			return false;

		Hue = sender.DyedHue;
		return true;
	}

	public TentBag( Serial serial ) : base( serial )
	{
	}

	public override BaseHouse GetHouse( Mobile owner )
	{
		return new Tent( owner, Hue );
	}

	public override int LabelNumber{ get{ return 1041211; } }
	public override Rectangle2D[] Area{ get{ return Tent.AreaArray; } }

	// Override basic deed OnPlacement() so that tent specific text is used and a non tent multi id
	// based house placement check is performed

	// Also checks for account tents as opposed to houses

	public override void OnPlacement(Mobile from, Point3D p)
	{
		if ( Deleted )
			return;

		if ( !IsChildOf( from.Backpack ) )
		{
			from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
		}
		else if ( from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse( from ) ) 
		{
			from.SendMessage( "You already own a house, you may not place a regular tent!" ); 
		}
		else
		{
			ArrayList toMove;
			Point3D center = new Point3D( p.X - Offset.X, p.Y - Offset.Y, p.Z - Offset.Z );
			HousePlacementResult res = HousePlacement.Check( from, 0x64, center, out toMove );

			switch ( res )
			{
				case HousePlacementResult.Valid:
				{
					BaseHouse house = GetHouse( from );
					house.MoveToWorld( center, from.Map );
					house.Public = true;

					Delete();

					for ( int i = 0; i < toMove.Count; ++i )
					{
						object o = toMove[i];

						if ( o is Mobile )
							((Mobile)o).Location = house.BanLocation;
						else if ( o is Item )
							((Item)o).Location = house.BanLocation;
					}

					from.SendGump( new TentPlaceGump( from, house ) );

					break;
				}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
				{
					from.SendMessage( "The tent could not be created here, Either something is blocking it, or it would not be on valid terrain." );
					break;
				}
				case HousePlacementResult.NoSurface:
				{
					from.SendMessage( "The tent could not be created here.  Part of the foundation would not be on any surface." );
					break;
				}
				case HousePlacementResult.BadRegion:
				{
					from.SendMessage( "Tents cannot be placed in this area." );
					break;
				}
				case HousePlacementResult.BadRegionTownship:
				{
					from.SendMessage("You are not authorized to build in this township.");
					break;
				}
			}
		}
	}

	public override void OnDoubleClick( Mobile from )
	{
		if ( !IsChildOf( from.Backpack ) )
		{
			from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
		}
		else
			from.Target = new HousePlacementTarget( this );
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


