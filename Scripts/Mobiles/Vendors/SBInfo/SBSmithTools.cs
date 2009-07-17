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

/* /Scripts/Mobiles/Vendors/SBInfo/SBSmithTools.cs
 * ChangeLog
 *  01/28/05 TK
 *		Added ores.
 *  01/23/05, Taran Kain
 *		Added all nine ingot colors
 *	4/24/04, mith
 *		Commented all items from SellList so that NPCs don't buy from players.
 */

using System; 
using System.Collections; 
using Server.Items; 

namespace Server.Mobiles 
{ 
	public class SBSmithTools: SBInfo 
	{ 
		private ArrayList m_BuyInfo = new InternalBuyInfo(); 
		private IShopSellInfo m_SellInfo = new InternalSellInfo(); 

		public SBSmithTools() 
		{ 
		} 

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } } 
		public override ArrayList BuyInfo { get { return m_BuyInfo; } } 

		public class InternalBuyInfo : ArrayList 
		{ 
			public InternalBuyInfo() 
			{ 
				Add( new GenericBuyInfo( typeof( Tongs ), 3, 14, 0xFBB, 0 ) ); 
				Add( new GenericBuyInfo( typeof( SmithHammer ), 4, 16, 0x13E3, 0 ) );
				Add(new GenericBuyInfo(typeof(IronIngot)));
				Add(new GenericBuyInfo(typeof(DullCopperIngot)));
				Add(new GenericBuyInfo(typeof(ShadowIronIngot)));
				Add(new GenericBuyInfo(typeof(CopperIngot)));
				Add(new GenericBuyInfo(typeof(BronzeIngot)));
				Add(new GenericBuyInfo(typeof(GoldIngot)));
				Add(new GenericBuyInfo(typeof(AgapiteIngot)));
				Add(new GenericBuyInfo(typeof(VeriteIngot)));
				Add(new GenericBuyInfo(typeof(ValoriteIngot)));
			} 
		} 

		public class InternalSellInfo : GenericSellInfo 
		{ 
			public InternalSellInfo() 
			{ 
				Add(typeof(IronIngot));
				Add(typeof(DullCopperIngot));
				Add(typeof(ShadowIronIngot));
				Add(typeof(CopperIngot));
				Add(typeof(BronzeIngot));
				Add(typeof(GoldIngot));
				Add(typeof(AgapiteIngot));
				Add(typeof(VeriteIngot));
				Add(typeof(ValoriteIngot));
				Add(typeof(IronOre));
				Add(typeof(DullCopperOre));
				Add(typeof(ShadowIronOre));
				Add(typeof(CopperOre));
				Add(typeof(BronzeOre));
				Add(typeof(GoldOre));
				Add(typeof(AgapiteOre));
				Add(typeof(VeriteOre));
				Add(typeof(ValoriteOre));
			} 
		} 
	} 
}