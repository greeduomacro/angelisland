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

namespace Server
{
	public class UsageAttribute : Attribute
	{
		private string m_Usage;

		public string Usage { get { return m_Usage; } }

		public UsageAttribute(string usage)
		{
			m_Usage = usage;
		}
	}

	public class DescriptionAttribute : Attribute
	{
		private string m_Description;

		public string Description { get { return m_Description; } }

		public DescriptionAttribute(string description)
		{
			m_Description = description;
		}
	}

	public class AliasesAttribute : Attribute
	{
		private string[] m_Aliases;

		public string[] Aliases { get { return m_Aliases; } }

		public AliasesAttribute(params string[] aliases)
		{
			m_Aliases = aliases;
		}
	}
}
