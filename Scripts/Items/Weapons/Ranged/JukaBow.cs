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
using Server.Network;
using Server.Items;

namespace Server.Items
{
	[FlipableAttribute( 0x13B2, 0x13B1 )]
	public class JukaBow : Bow
	{
//		public override int AosStrengthReq{ get{ return 80; } }
//		public override int AosDexterityReq{ get{ return 80; } }

		public override int OldStrengthReq{ get{ return 80; } }
		public override int OldDexterityReq{ get{ return 80; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IsModified
		{
			get{ return ( Hue == 0x453 ); }
		}

		[Constructable]
		public JukaBow()
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsModified )
			{
				from.SendMessage( "That has already been modified." );
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				from.SendMessage( "This must be in your backpack to modify it." );
			}
			else if ( from.Skills[SkillName.Fletching].Base < 100.0 )
			{
				from.SendMessage( "Only a grandmaster bowcrafter can modify this weapon." );
			}
			else
			{
				from.BeginTarget( 2, false, Targeting.TargetFlags.None, new TargetCallback( OnTargetGears ) );
				from.SendMessage( "Select the gears you wish to use." );
			}
		}

		public void OnTargetGears( Mobile from, object targ )
		{
			Gears g = targ as Gears;

			if ( g == null || !g.IsChildOf( from.Backpack ) )
			{
				from.SendMessage( "Those are not gears." ); // Apparently gears that aren't in your backpack aren't really gears at all. :-(
			}
			else if ( IsModified )
			{
				from.SendMessage( "That has already been modified." );
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				from.SendMessage( "This must be in your backpack to modify it." );
			}
			else if ( from.Skills[SkillName.Fletching].Base < 100.0 )
			{
				from.SendMessage( "Only a grandmaster bowcrafter can modify this weapon." );
			}
			else
			{
				g.Consume();

				Hue = 0x453;
				Slayer = (SlayerName)Utility.Random( 2, 25 );

				from.SendMessage( "You modify it." );
			}
		}

		public JukaBow( Serial serial ) : base( serial )
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