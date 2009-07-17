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
using System.Collections;
using Server.Items;
using Server.Multis;

namespace Server.Mobiles
{
	public class SBShipwright : SBInfo
	{
		private ArrayList m_BuyInfo = new InternalBuyInfo();
		private IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBShipwright()
		{
		}

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
		public override ArrayList BuyInfo { get { return m_BuyInfo; } }

		public class InternalBuyInfo : ArrayList
		{
			public InternalBuyInfo()
			{
				Add( new GenericBuyInfo( "1041205", typeof( SmallBoatDeed ), 12500, 20, 0x14F2, 0 ) );
				Add( new GenericBuyInfo( "1041206", typeof( SmallDragonBoatDeed ), 12500, 20, 0x14F2, 0 ) );
				Add( new GenericBuyInfo( "1041207", typeof( MediumBoatDeed ), 14200, 20, 0x14F2, 0 ) );
				Add( new GenericBuyInfo( "1041208", typeof( MediumDragonBoatDeed ), 14200, 20, 0x14F2, 0 ) );
				Add( new GenericBuyInfo( "1041209", typeof( LargeBoatDeed ), 15900, 20, 0x14F2, 0 ) );
				Add( new GenericBuyInfo( "1041210", typeof( LargeDragonBoatDeed ), 15900, 20, 0x14F2, 0 ) );
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
				// Can you sell deeds back?
				/*Add( typeof( SmallBoatDeed ), 6250 );
				Add( typeof( SmallDragonBoatDeed ), 6250 );
				Add( typeof( MediumBoatDeed ), 7100 );
				Add( typeof( MediumDragonBoatDeed ), 7100 );
				Add( typeof( LargeBoatDeed ), 7950 );
				Add( typeof( LargeDragonBoatDeed ), 7950 );*/
			}
		}
	}
} 
