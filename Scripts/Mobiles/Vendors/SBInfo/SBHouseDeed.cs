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

/* Scripts/Mobiles/Vendors/SBInfo/SBHouseDeed.cs
 * CHANGELOG:
 *  6/12/07, adam
 *      add check for TestCenter.Enabled == true before adding Static houses for sale.
 *      We don't want this on until we have checked in a valid StaticHousing*.xml
 *	6/11/07 - Pix
 *		Added our static house deeds!
 *	11/22/06 - Pix
 *		Added missing TwoStoryStonePlasterHouseDeed
 */

using System;
using System.Collections;
using Server.Multis.Deeds;
using Server.Misc;              // test center

namespace Server.Mobiles
{
	public class SBHouseDeed: SBInfo
	{
		private ArrayList m_BuyInfo = new InternalBuyInfo();
		private IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBHouseDeed()
		{
		}

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
		public override ArrayList BuyInfo { get { return m_BuyInfo; } }

		public class InternalBuyInfo : ArrayList
		{
			public InternalBuyInfo()
			{
                Add(new GenericBuyInfo("deed to a stone and plaster house", typeof(StonePlasterHouseDeed), 43800, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a field stone house", typeof(FieldStoneHouseDeed), 43800, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a wooden house", typeof(WoodHouseDeed), 43800, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a wood and plaster house", typeof(WoodPlasterHouseDeed), 43800, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a thatched roof cottage", typeof(ThatchedRoofCottageDeed), 43800, 20, 0x14F0, 0));

                Add(new GenericBuyInfo("deed to a small brick house", typeof(SmallBrickHouseDeed), 43800, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a small stone workshop", typeof(StoneWorkshopDeed), 60600, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a small marble workshop", typeof(MarbleWorkshopDeed), 63000, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a small stone tower", typeof(SmallTowerDeed), 88500, 20, 0x14F0, 0));

                Add(new GenericBuyInfo("deed to a sandstone house with patio", typeof(SandstonePatioDeed), 90900, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a large house with patio", typeof(LargePatioDeed), 152800, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a marble house with patio", typeof(LargeMarbleDeed), 192000, 20, 0x14F0, 0));
                
                Add(new GenericBuyInfo("deed to a brick house", typeof(BrickHouseDeed), 144500, 20, 0x14F0, 0));

                Add(new GenericBuyInfo("deed to a two-story log cabin", typeof(LogCabinDeed), 97800, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a two-story wood and plaster house", typeof(TwoStoryWoodPlasterHouseDeed), 192400, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a two-story stone and plaster house", typeof(TwoStoryStonePlasterHouseDeed), 192400, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a two-story villa", typeof(VillaDeed), 136500, 20, 0x14F0, 0));

                Add(new GenericBuyInfo("deed to a tower", typeof(TowerDeed), 433200, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a small stone keep", typeof(KeepDeed), 665200, 20, 0x14F0, 0));
                Add(new GenericBuyInfo("deed to a castle", typeof(CastleDeed), 1022800, 20, 0x14F0, 0));

                // adam: remove this when we go live
                //if (TestCenter.Enabled == true)
                {
                    System.Collections.Generic.List<Server.Multis.StaticHousing.StaticHouseDescription> shList = Server.Multis.StaticHousing.StaticHouseHelper.GetAllStaticHouseDescriptions();
                    foreach (Server.Multis.StaticHousing.StaticHouseDescription shd in shList)
                    {
                        //Server.Multis.StaticHousing.StaticDeed
                        Add(new GenericBuyInfo("deed to a " + shd.Description,
                                typeof(Server.Multis.StaticHousing.StaticDeed),
                                shd.Price,
                                20,
                                0,
                                0,
                                0x14F0,
                                0,
                                new object[] { shd.ID, shd.Description }
                                )
                            );
                    }
                }
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
				/*Add( typeof( StonePlasterHouseDeed ), 43800 );
				Add( typeof( FieldStoneHouseDeed ), 43800 );
				Add( typeof( SmallBrickHouseDeed ), 43800 );
				Add( typeof( WoodHouseDeed ), 43800 );
				Add( typeof( WoodPlasterHouseDeed ), 43800 );
				Add( typeof( ThatchedRoofCottageDeed ), 43800 );
				Add( typeof( BrickHouseDeed ), 144500 );
				Add( typeof( TwoStoryWoodPlasterHouseDeed ), 192400 );
				Add( typeof( TowerDeed ), 433200 );
				Add( typeof( KeepDeed ), 665200 );
				Add( typeof( CastleDeed ), 1022800 );
				Add( typeof( LargePatioDeed ), 152800 );
				Add( typeof( LargeMarbleDeed ), 192800 );
				Add( typeof( SmallTowerDeed ), 88500 );
				Add( typeof( LogCabinDeed ), 97800 );
				Add( typeof( SandstonePatioDeed ), 90900 );
				Add( typeof( VillaDeed ), 136500 );
				Add( typeof( StoneWorkshopDeed ), 60600 );
				Add( typeof( MarbleWorkshopDeed ), 60300 );
				Add( typeof( SmallBrickHouseDeed ), 43800 );*/
			}
		}
	}
}
