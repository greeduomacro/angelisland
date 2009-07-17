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

/* Items/Deeds/GenderChangeDeed.cs
 * ChangeLog:
 *	11/16/04 Darva
 *		Created file
 *		Made it change your gender when double clicked, removing all facial hair.
 */

using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server;

namespace Server.Items
{
	public class GenderChangeDeed : Item
	{
		[Constructable]
		public GenderChangeDeed() : base( 0x14F0 )
		{
			base.Weight = 1.0;
			base.Name = "a gender change deed";
		}

		public GenderChangeDeed( Serial serial ) : base( serial )
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
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); //This must be in your backpack
			}
			else if (from.BodyMod != 0)
			{
				from.SendMessage( "You must be in your normal form to change your gender.");
			}
			else
			{
				
				Body body;
				if (from.Female == false)
				{
					body = new Body(401);
				}
				else
				{
					body = new Body(400);
				}
				from.Body = body;
				from.Female = !from.Female;
				if (from.Beard != null)
					from.Beard.Delete();	
				from.SendMessage ("Your gender has been changed.");
				BaseArmor.ValidateMobile(from);
				this.Delete();
			}
			
		}
	}
}


