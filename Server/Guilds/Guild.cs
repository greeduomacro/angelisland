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

/* Server/Guilds/Guild.cs
 * CHANGELOG:
 * 12/14/05 Kit,
 *		Added Case search for names/abbreviatons to Prevent Orc oRc from working.
 */

using System;
using System.Collections;
using Server.Items;

namespace Server.Guilds
{
	public enum GuildType
	{
		Regular,
		Chaos,
		Order
	}

	public abstract class BaseGuild
	{
		private int m_Id;

		public BaseGuild(int Id)//serialization ctor
		{
			m_Id = Id;
			m_GuildList.Add(m_Id, this);
			if (m_Id + 1 > m_NextID)
				m_NextID = m_Id + 1;
		}

		public BaseGuild()
		{
			m_Id = m_NextID++;
			m_GuildList.Add(m_Id, this);
		}

		public int Id { get { return m_Id; } }

		public abstract void Deserialize(GenericReader reader);
		public abstract void Serialize(GenericWriter writer);

		public abstract string Abbreviation { get; set; }
		public abstract string Name { get; set; }
		public abstract GuildType Type { get; set; }
		public abstract bool Disbanded { get; }
		public abstract void OnDelete(Mobile mob);

		private static Hashtable m_GuildList = new Hashtable();
		private static int m_NextID = 1;

		public static Hashtable List
		{
			get
			{
				return m_GuildList;
			}
		}

		public static BaseGuild Find(int id)
		{
			return (BaseGuild)m_GuildList[id];
		}

		public static BaseGuild FindByName(string name)
		{
			foreach (BaseGuild g in m_GuildList.Values)
			{
				if (g.Name.ToLower() == name.ToLower())
					return g;
			}

			return null;
		}

		public static BaseGuild FindByAbbrev(string abbr)
		{
			foreach (BaseGuild g in m_GuildList.Values)
			{
				if (g.Abbreviation.ToLower() == abbr.ToLower())
					return g;
			}

			return null;
		}

		public static BaseGuild[] Search(string find)
		{
			string[] words = find.ToLower().Split(' ');
			ArrayList results = new ArrayList();

			foreach (BaseGuild g in m_GuildList.Values)
			{
				bool match = true;
				string name = g.Name.ToLower();
				for (int i = 0; i < words.Length; i++)
				{
					if (name.IndexOf(words[i]) == -1)
					{
						match = false;
						break;
					}
				}

				if (match)
					results.Add(g);
			}

			return (BaseGuild[])results.ToArray(typeof(BaseGuild));
		}
	}
}
