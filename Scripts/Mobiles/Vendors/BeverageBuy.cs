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
	public class BeverageBuyInfo : GenericBuyInfo
	{
		private BeverageType m_Content;

		public BeverageBuyInfo( Type type, BeverageType content, int price, int amount, int itemID, int hue ) : this( null, type, content, price, amount, itemID, hue )
		{
		}

		public BeverageBuyInfo( string name, Type type, BeverageType content, int price, int amount, int itemID, int hue ) : base( name, type, price, amount, itemID, hue )
		{
			m_Content = content;

			if ( type == typeof( Pitcher ) )
				Name = (1048128 + (int)content).ToString();
			else if ( type == typeof( BeverageBottle ) )
				Name = (1042959 + (int)content).ToString();
			else if ( type == typeof( Jug ) )
				Name = (1042965 + (int)content).ToString();
		}

		public override object GetObject()
		{
			return Activator.CreateInstance( Type, new object[]{ m_Content } );
		}
	}
} 
