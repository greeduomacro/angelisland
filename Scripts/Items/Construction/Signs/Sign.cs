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
using Server;

namespace Server.Items
{
	public enum SignFacing
	{
		North,
		West
	}

	public enum SignType
	{
		Library,
		DarkWoodenPost,
		LightWoodenPost,
		MetalPostC,
		MetalPostB,
		MetalPostA,
		MetalPost,
		Bakery,
		Tailor,
		Tinker,
		Butcher,
		Healer,
		Mage,
		Woodworker,
		Customs,
		Inn,
		Shipwright,
		Stables,
		BarberShop,
		Bard,
		Fletcher,
		Armourer,
		Jeweler,
		Tavern,
		ReagentShop,
		Blacksmith,
		Painter,
		Provisioner,
		Bowyer,
		WoodenSign,
		BrassSign,
		ArmamentsGuild,
		ArmourersGuild,
		BlacksmithsGuild,
		WeaponsGuild,
		BardicGuild,
		BartersGuild,
		ProvisionersGuild,
		TradersGuild,
		CooksGuild,
		HealersGuild,
		MagesGuild,
		SorcerersGuild,
		IllusionistGuild,
		MinersGuild,
		ArchersGuild,
		SeamensGuild,
		FishermensGuild,
		SailorsGuild,
		ShipwrightsGuild,
		TailorsGuild,
		ThievesGuild,
		RoguesGuild,
		AssassinsGuild,
		TinkersGuild,
		WarriorsGuild,
		CavalryGuild,
		FightersGuild,
		MerchantsGuild,
		Bank,
		Theatre
	}

	public class Sign : BaseSign
	{
		[Constructable]
		public Sign( SignType type, SignFacing facing ) : base( ( 0xB95 + (2 * (int)type) ) + (int)facing )
		{
		}

		[Constructable]
		public Sign( int itemID ) : base( itemID )
		{
		}

		public Sign( Serial serial ) : base( serial )
		{
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
}�