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

/* /Scripts/Mobiles/Vendors/GenericSell.cs
 * Changelog
 *	01/23/05	Taran Kain
 *		Added logic to support Resource Pool.
 */

using System;
using System.Collections;
using Server.Items;
using Server.Engines.ResourcePool;

namespace Server.Mobiles
{
	public class GenericSellInfo : IShopSellInfo
	{
		private Hashtable m_Table = new Hashtable();
		private ArrayList m_MyTypes = new ArrayList();
		private Type[] m_Types;

		public GenericSellInfo()
		{
		}

		public void Add( Type type, int price )
		{
			m_Table[type] = price;
			m_MyTypes.Add( type );
			m_Types = null;
		}

		public void Add(Type type)
		{
			if (ResourcePool.IsPooledResource(type))
			{
				Add(type, -1);
				if (m_MyTypes.IndexOf(typeof(CommodityDeed)) == -1)
					Add(typeof(CommodityDeed), 0);
			}
		}

		public int GetSellPriceFor( Item item )
		{
			int price = (int)m_Table[item.GetType()];
			if (price == -1) // shouldn't ever be asking for this anyway, but for sanity
				return (int)ResourcePool.GetWholesalePrice(item.GetType());

			if ( item is BaseArmor )
			{
				BaseArmor armor = (BaseArmor)item;

				if ( armor.Quality == ArmorQuality.Low )
					price = (int)( price * 0.60 );
				else if ( armor.Quality == ArmorQuality.Exceptional )
					price = (int)( price * 1.25 );

				price += 100 * (int)armor.Durability;

				price += 100 * (int)armor.ProtectionLevel;

				if ( price < 1 )
					price = 1;
			}

			else if ( item is BaseWeapon )
			{
				BaseWeapon weapon = (BaseWeapon)item;

				if ( weapon.Quality == WeaponQuality.Low )
					price = (int)( price * 0.60 );
				else if ( weapon.Quality == WeaponQuality.Exceptional )
					price = (int)( price * 1.25 );

				price += 100 * (int)weapon.DurabilityLevel;

				price += 100 * (int)weapon.DamageLevel;

				if ( price < 1 )
					price = 1;
			}
			else if ( item is BaseBeverage )
			{
				int price1 = price, price2 = price;

				if ( item is Pitcher )
				{ price1 = 3; price2 = 5; }
				else if ( item is BeverageBottle )
				{ price1 = 3; price2 = 3; }
				else if ( item is Jug )
				{ price1 = 6; price2 = 6; }

				BaseBeverage bev = (BaseBeverage)item;

				if ( bev.IsEmpty || bev.Content == BeverageType.Milk )
					price = price1;
				else
					price = price2;
			}

			return price;
		}

		public int GetBuyPriceFor( Item item )
		{
			return (int)( 1.90 * GetSellPriceFor( item ) );
		}

		public Type[] Types
		{
			get
			{
				if ( m_Types == null )
					m_Types = (Type[])m_MyTypes.ToArray( typeof( Type ) );

				return m_Types;
			}
		}

		public string GetNameFor( Item item )
		{
			if ( item.Name != null )
				return item.Name;
			else
				return item.LabelNumber.ToString();
		}

		public bool IsSellable( Item item )
		{
			//if ( item.Hue != 0 )
				//return false;

			if (item is CommodityDeed)
				return IsInList( ((CommodityDeed)item).Commodity.GetType());

			return IsInList( item.GetType() );
		}
	 
		public bool IsResellable( Item item )
		{
			//if ( item.Hue != 0 )
				//return false;

			if (item is CommodityDeed)
				return IsInList( ((CommodityDeed)item).Commodity.GetType());

			return IsInList( item.GetType() );
		}

		public bool IsInList( Type type )
		{
			Object o = m_Table[type];

			if ( o == null )
				return false;
			else
				return true;
		}
	}
}
