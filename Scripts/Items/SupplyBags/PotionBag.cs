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

/* Scripts\Items\SupplyBags\PotionBag.cs
 * ChangeLog
 *	4/4/08, Adam
 *		first time checkin
 */

using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class PotionBag : Backpack
	{
		[Constructable]
		public PotionBag()
			: this(1)
		{
			Movable = true;
			Hue = 0x979;
			Name = "a Potion Bag";
		}
		[Constructable]
		public PotionBag(int amount)
		{
			// Begin bag of potion kegs
			this.Name = "Various Potion Kegs";
			PlaceItemIn(this, 45, 149, MakePotionKeg(PotionEffect.CureGreater, 0x2D));
			PlaceItemIn(this, 69, 149, MakePotionKeg(PotionEffect.HealGreater, 0x499));
			PlaceItemIn(this, 93, 149, MakePotionKeg(PotionEffect.PoisonDeadly, 0x46));
			PlaceItemIn(this, 117, 149, MakePotionKeg(PotionEffect.RefreshTotal, 0x21));
			PlaceItemIn(this, 141, 149, MakePotionKeg(PotionEffect.ExplosionGreater, 0x74));
			PlaceItemIn(this, 93, 82, new Bottle(1000));
		}

		public PotionBag(Serial serial)
			: base(serial)
		{
		}

		private void PlaceItemIn(Container parent, int x, int y, Item item)
		{
			parent.AddItem(item);
			item.Location = new Point3D(x, y, 0);
		}

		private Item MakePotionKeg(PotionEffect type, int hue)
		{
			PotionKeg keg = new PotionKeg();

			keg.Held = 100;
			keg.Type = type;
			keg.Hue = hue;

			return keg;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version 
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
