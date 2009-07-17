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
 
 /* /Scripts/Items/FlowersPlants/CottonPlant.cs
  *
  *	ChangeLog:
  * 2/11/07, Pix
  *		Finally added range check to picking cotton.
  *	5/26/04 Created by smerX
  *
  */
 
using System;

namespace Server.Items
{
	public class CottonPlant : Item
	{
		[Constructable]
		public CottonPlant() : base( Utility.RandomList( 0xc51, 0xc52, 0xc53, 0xc54 ) )
		{
			Weight = 0;
			Name = "a cotton plant";
			Movable = false;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if (!from.InRange(GetWorldLocation(), 1))
			{
				SendLocalizedMessageTo(from, 501816); // You are too far away to do that.
			}
			else
			{
				Cotton cotton = new Cotton();
				cotton.MoveToWorld(new Point3D(this.X, this.Y, this.Z), this.Map);

				this.Delete();
			}
		}

		public CottonPlant(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}