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
/* Scripts/Mobiles/Vendors/PresetMapBuy.cs
 * Changelog
 *  07/02/05 Taran Kain
 *		Made constructor correctly set type, was causing crashes
 */

using System;
using System.Collections;
using Server.Items;

namespace Server.Mobiles
{
	public class PresetMapBuyInfo : GenericBuyInfo
	{
		private PresetMapEntry m_Entry;

		public PresetMapBuyInfo( PresetMapEntry entry, int price, int amount ) : base( entry.Name.ToString(), typeof(Server.Items.PresetMap), price, amount, 0x14EC, 0 )
		{
			m_Entry = entry;
		}

		public override object GetObject()
		{
			return new PresetMap( m_Entry );
		}
	}
}