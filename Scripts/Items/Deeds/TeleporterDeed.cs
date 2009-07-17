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

/* Items/Deeds/TeleporterDeed.cs
 * ChangeLog:
 *	9/13/06, Pix.
 *		Added logic to convert this deed to new teleporter pair addon deed on doubleclick and in backpack
 *  8/27/04, Adam
 *		Created.
 *		Add message when double clicked.
 */

using System;
using Server.Network;
using Server.Prompts;
using Server.Items;

namespace Server.Items
{
	public class TeleporterDeed : Item
	{
		[Constructable]
		public TeleporterDeed() : base( 0x14F0 )
		{
			base.Weight = 1.0;
			base.Name = "a teleporter deed";
		}

		public TeleporterDeed( Serial serial ) : base( serial )
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

		public override void OnDoubleClick( Mobile from )
		{
			//from.SendMessage( "Please page a GM for Teleporter instalation." );
			if( this.IsChildOf( from.Backpack ) )
			{
				from.AddToBackpack( new TeleporterAddonDeed() );
				this.Delete();
				from.SendMessage("Your old teleporter deed has been converted to a new teleporter deed.");
			}
			else
			{
				from.SendMessage("This must be in your backpack to use.");
			}
		}
	}
}


