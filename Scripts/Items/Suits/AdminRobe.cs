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

/* Scripts/Items/Suits/AdminRobe.cs
 * ChangeLog
 *  7/21/06, Rhiannon
 *		Added Owner access level to Godly robe
 *	6/27/06, Adam
 *		- (re)set the player mobile titles automatically if not Godly
 *		- clear fame and karma (titles) if not Godly
 *		- reset color on robe load
 *		- Add Godly version of robes for Adam And Jade
 *	4/17/06, Adam
 *		Explicitly set name
 *	11/07/04, Jade
 *		Changed hue to 0x1.
 */

using System;
using Server;

namespace Server.Items
{
	public class AdminRobe : BaseSuit
	{
		private const int m_hue = 0x0;	// Admin color

		[Constructable]
		public AdminRobe() : base( AccessLevel.Administrator, m_hue, 0x204F )
		{
			Name = "Administrator Robe";
		}

		public AdminRobe( Serial serial ) : base( serial )
		{
		}

		public override bool OnEquip( Mobile from )
		{
			if (base.OnEquip( from ) == true )
			{
				from.Title = "a shard administrator";
				from.Fame = 0;
				from.Karma = 0;
				return true;
			}
			else return false;
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

			if (Hue != m_hue)
				Hue = m_hue;
		}
	}

	public class GodlyRobe : BaseSuit
	{
		private const int m_hue = 0x1;	// godly colors (Adam and Jade Only)

		[Constructable]
		public GodlyRobe() : base( AccessLevel.Owner, m_hue, 0x204F )
		{
			Name = "Godly Robe";
		}

		public GodlyRobe( Serial serial ) : base( serial )
		{
		}

		public override bool OnEquip( Mobile from )
		{
			if (base.OnEquip( from ) == true )
			{	// gods can have any title they want
				// gods can have fame/karma titles as well
				return true;
			}
			else return false;
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

			if (Hue != m_hue)
				Hue = m_hue;
		}
	}
}