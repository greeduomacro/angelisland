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

/* /Scripts/Mobiles/Vendors/SBInfo/SBWeaver.cs
 * ChangeLog
 *  02/04/05 TK
 *		Added new cloth redirects
 *  01/28/05 TK
 *		Added BoltOfCloth, UncutCloth
 *	01/23/05, Taran Kain
 *		Added cloth.
 *	4/24/04, mith
 *		Commented all items from SellList so that NPCs don't buy from players.
 */

using System; 
using System.Collections; 
using Server.Items; 

namespace Server.Mobiles 
{ 
	public class SBWeaver: SBInfo 
	{ 
		private ArrayList m_BuyInfo = new InternalBuyInfo(); 
		private IShopSellInfo m_SellInfo = new InternalSellInfo(); 

		public SBWeaver() 
		{ 
		} 

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } } 
		public override ArrayList BuyInfo { get { return m_BuyInfo; } } 

		public class InternalBuyInfo : ArrayList 
		{ 
			public InternalBuyInfo() 
			{ 
				Add( new GenericBuyInfo( typeof( Scissors ), 13, 20, 0xF9F, 0 ) );  
				Add( new GenericBuyInfo( typeof( Dyes ), 8, 20, 0xFA9, 0 ) ); 
				Add( new GenericBuyInfo( typeof( DyeTub ), 9, 20, 0xFAB, 0 ) ); 
				Add( new GenericBuyInfo( typeof( Cloth ) ) ); 
			} 
		} 

		public class InternalSellInfo : GenericSellInfo 
		{ 
			public InternalSellInfo() 
			{ 
				Add(typeof(Cloth));
				Add(typeof(BoltOfCloth));
				Add(typeof(UncutCloth));
				Add(typeof(Cotton));
				Add(typeof(DarkYarn));
				Add(typeof(LightYarn));
				Add(typeof(LightYarnUnraveled));
				Add(typeof(Wool));
/*				Add( typeof( Scissors ), 6 ); 
 *				Add( typeof( Dyes ), 4 ); 
 *				Add( typeof( DyeTub ), 4 ); 
 *				Add( typeof( BoltOfCloth ), 60 ); 
 *				Add( typeof( LightYarnUnraveled ), 9 );
 *				Add( typeof( LightYarn ), 9 );
 *				Add( typeof( DarkYarn ), 9 );
 */
			} 
		} 
	} 
}