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

namespace Server.Engines.Craft
{
	public class CraftSubResCol : System.Collections.CollectionBase
	{
		private Type m_Type;
		private string m_NameString;
		private int m_NameNumber;
		private bool m_Init;

		public bool Init
		{
			get { return m_Init; }
			set { m_Init = value; }
		}
				
		public Type ResType
		{
			get { return m_Type; }
			set { m_Type = value; }
		}

		public string NameString
		{
			get { return m_NameString; }
			set { m_NameString = value; }
		}

		public int NameNumber
		{
			get { return m_NameNumber; }
			set { m_NameNumber = value; }
		}

		public CraftSubResCol()
		{
			m_Init = false;
		}

		public void Add( CraftSubRes craftSubRes )
		{
			List.Add( craftSubRes );
		}

		public void Remove( int index )
		{
			if ( index > Count - 1 || index < 0 )
			{
			}
			else
			{
				List.RemoveAt( index );
			}
		}

		public CraftSubRes GetAt( int index )
		{
			return ( CraftSubRes ) List[index];
		}

		public CraftSubRes SearchFor( Type type )
		{
			for ( int i = 0; i < List.Count; i++ )
			{
				CraftSubRes craftSubRes = ( CraftSubRes )List[i];
				if ( craftSubRes.ItemType == type )
				{
					return craftSubRes;
				}
			}
			return null;
		}
	}
}�