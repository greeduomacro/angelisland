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

namespace Server.Mobiles
{
	public class SBWaiter : SBInfo
	{
		private ArrayList m_BuyInfo = new InternalBuyInfo();
		private IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBWaiter()
		{
		}

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
		public override ArrayList BuyInfo { get { return m_BuyInfo; } }

		public class InternalBuyInfo : ArrayList
		{
			public InternalBuyInfo()
			{
				Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Ale, 7, 20, 0x99F, 0 ) );
				Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Wine, 7, 20, 0x9C7, 0 ) );
				Add( new BeverageBuyInfo( typeof( BeverageBottle ), BeverageType.Liquor, 7, 20, 0x99B, 0 ) );
				Add( new BeverageBuyInfo( typeof( Jug ), BeverageType.Cider, 13, 20, 0x9C8, 0 ) );
				Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Milk, 7, 20, 0x9F0, 0 ) );
				Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Ale, 11, 20, 0x1F95, 0 ) );
				Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Cider, 11, 20, 0x1F97, 0 ) );
				Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Liquor, 11, 20, 0x1F99, 0 ) );
				Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Wine, 11, 20, 0x1F9B, 0 ) );
				Add( new BeverageBuyInfo( typeof( Pitcher ), BeverageType.Water, 11, 20, 0x1F9D, 0 ) );
				Add( new GenericBuyInfo( typeof( Pitcher ), 7, 20, 0xFF6, 0 ) );
				Add( new GenericBuyInfo( typeof( BreadLoaf ), 7, 10, 0x103B, 0 ) );
				Add( new GenericBuyInfo( typeof( CheeseWheel ), 25, 10, 0x97E, 0 ) );
				Add( new GenericBuyInfo( typeof( CookedBird ), 17, 20, 0x9B7, 0 ) );
				Add( new GenericBuyInfo( typeof( LambLeg ), 8, 20, 0x160A, 0 ) );
				// TODO: Bowl of *, tomato soup, baked pie
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
			}
		}
	}
} 
