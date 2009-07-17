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
using Server.Mobiles;

namespace Server.Items
{
	public class SlayerEntry
	{
		private SlayerGroup m_Group;
		private SlayerName m_Name;
		private Type[] m_Types;

		public SlayerGroup Group{ get{ return m_Group; } set{ m_Group = value; } }
		public SlayerName Name{ get{ return m_Name; } }
		public Type[] Types{ get{ return m_Types; } }

		public SlayerEntry( SlayerName name, params Type[] types )
		{
			m_Name = name;
			m_Types = types;
		}

		public bool Slays( Mobile m )
		{
			Type t = m.GetType();

			for ( int i = 0; i < m_Types.Length; ++i )
				if ( m_Types[i] == t )
					return true;

			return false;
		}
	}
}�