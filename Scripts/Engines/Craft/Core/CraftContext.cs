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

namespace Server.Engines.Craft
{
	public enum CraftMarkOption
	{
		MarkItem,
		DoNotMark,
		PromptForMark
	}

	public class CraftContext
	{
		private ArrayList m_Items;
		private int m_LastResourceIndex;
		private int m_LastResourceIndex2;
		private int m_LastGroupIndex;
		private bool m_DoNotColor;
		private CraftMarkOption m_MarkOption;

		public ArrayList Items{ get{ return m_Items; } }
		public int LastResourceIndex{ get{ return m_LastResourceIndex; } set{ m_LastResourceIndex = value; } }
		public int LastResourceIndex2{ get{ return m_LastResourceIndex2; } set{ m_LastResourceIndex2 = value; } }
		public int LastGroupIndex{ get{ return m_LastGroupIndex; } set{ m_LastGroupIndex = value; } }
		public bool DoNotColor{ get{ return m_DoNotColor; } set{ m_DoNotColor = value; } }
		public CraftMarkOption MarkOption{ get{ return m_MarkOption; } set{ m_MarkOption = value; } }

		public CraftContext()
		{
			m_Items = new ArrayList();
			m_LastResourceIndex = -1;
			m_LastResourceIndex2 = -1;
			m_LastGroupIndex = -1;
		}

		public CraftItem LastMade
		{
			get
			{
				if ( m_Items.Count > 0 )
					return (CraftItem)m_Items[0];

				return null;
			}
		}

		public void OnMade( CraftItem item )
		{
			m_Items.Remove( item );

			if ( m_Items.Count == 10 )
				m_Items.RemoveAt( 9 );

			m_Items.Insert( 0, item );
		}
	}
} 
