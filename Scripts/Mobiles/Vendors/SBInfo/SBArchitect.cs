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

/* /Scripts/Mobiles/Vendors/SBInfo/SBArchitect.cs
 * ChangeLog
 *  9/29/07, plasma
 *    Added DarkWoodGateHouseDoorDeed, LightWoodHouseDoorDeed, LightWoodGateHouseDoorDeed, 
 *    StrongWoodHouseDoorDeed, SmallIronGateHouseDoorDeed, SecretLightStoneHouseDoorDeed,
 *    SecretLightWoodHouseDoorDeed, SecretDarkWoodHouseDoorDeed and RattanHouseDoorDeed to the buy list     
 *  5/6/07, Adam
 *      Add StorageTaxCredits, and LockboxBuildingPermit deeds to the shopping list
 *	2/27/07, Pix
 *		Added CellHouseDoorDeed
 *  12/05/06 Taran Kain
 *      Added IronGateHouseDoorDeed.
 *	9/05/06 Taran Kain
 *		Added MetalHouseDoorDeed, DarkWoodHouseDoorDeed, SecretStoneHouseDoorDeed to vendor sell list.
 *  9/26/04, Jade
 *      Added SurveyTool to the vendor inventory.
 *	4/29/04, mith
 *		removed Core.AOS check so that Architects will sell house placement tools even if AOS is disabled.
 */

using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
	public class SBArchitect : SBInfo
	{
		private ArrayList m_BuyInfo = new InternalBuyInfo();
		private IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBArchitect()
		{
		}

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
		public override ArrayList BuyInfo { get { return m_BuyInfo; } }

		public class InternalBuyInfo : ArrayList
		{
			public InternalBuyInfo()
			{
				Add( new GenericBuyInfo( "1041280", typeof( InteriorDecorator ), 10000, 20, 0xFC1, 0 ) );
        Add( new GenericBuyInfo( "Survey Tool", typeof( SurveyTool ), 5000, 20, 0x14F6, 0));
        Add(new GenericBuyInfo(typeof(MetalHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(DarkWoodHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(DarkWoodGateHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(LightWoodHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(LightWoodGateHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(StrongWoodHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(IronGateHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(SmallIronGateHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(CellHouseDoorDeed), 25000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(RattanHouseDoorDeed), 25000, 20, 0x14F0, 0));				
        //secret doors              
        Add(new GenericBuyInfo(typeof(SecretStoneHouseDoorDeed), 28000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(SecretLightStoneHouseDoorDeed), 28000, 20, 0x14F0, 0));       
        Add(new GenericBuyInfo(typeof(SecretLightWoodHouseDoorDeed), 28000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo(typeof(SecretDarkWoodHouseDoorDeed), 28000, 20, 0x14F0, 0)); 
        Add(new GenericBuyInfo("30 day tax credit: Lockbox", typeof(StorageTaxCredits), 30000, 20, 0x14F0, 0));
        Add(new GenericBuyInfo("A building permit: Lockbox", typeof(LockboxBuildingPermit), 15000, 20, 0x14F0, 0));

                //if ( Core.AOS )
				//Add( new GenericBuyInfo( "1060651", typeof( HousePlacementTool ), 601, 20, 0x14F6, 0 ));
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
				Add( typeof( InteriorDecorator ), 5000 );

				if ( Core.AOS )
					Add( typeof( HousePlacementTool ), 301 );
			}
		}
	}
}