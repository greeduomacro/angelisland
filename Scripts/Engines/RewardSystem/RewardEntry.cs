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

/* /Scripts/Engines/Reward System/RewardEntry.cs
 * Created 5/23/04 by mith
 * ChangeLog
 */

using System;

namespace Server.Engines.RewardSystem
{
	public class RewardEntry
	{
		private RewardList m_List;
		private RewardCategory m_Category;
		private Type m_ItemType;
		private int m_Name;
		private string m_NameString;
		private object[] m_Args;

		public RewardList List{ get{ return m_List; } set{ m_List = value; } }
		public RewardCategory Category{ get{ return m_Category; } }
		public Type ItemType{ get{ return m_ItemType; } }
		public int Name{ get{ return m_Name; } }
		public string NameString{ get{ return m_NameString; } }
		public object[] Args{ get{ return m_Args; } }

		public Item Construct()
		{
			try
			{
				Item item = Activator.CreateInstance( m_ItemType, m_Args ) as Item;

				if ( item is IRewardItem )
					((IRewardItem)item).IsRewardItem = true;

				return item;
			}
			catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

			return null;
		}

		public RewardEntry( RewardCategory category, int name, Type itemType, params object[] args )
		{
			m_Category = category;
			m_ItemType = itemType;
			m_Name = name;
			m_Args = args;
			category.Entries.Add( this );
		}

		public RewardEntry( RewardCategory category, string name, Type itemType, params object[] args )
		{
			m_Category = category;
			m_ItemType = itemType;
			m_NameString = name;
			m_Args = args;
			category.Entries.Add( this );
		}
	}
}