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
	public class CraftSubRes
	{
		private Type m_Type;
		private double m_ReqSkill;
		private string m_NameString;
		private int m_NameNumber;
		private int m_GenericNameNumber;
		private object m_Message;

		public CraftSubRes( Type type, string name, double reqSkill, object message )
		{
			m_Type = type;
			m_NameString = name;
			m_ReqSkill = reqSkill;
			m_Message = message;
		}

		public CraftSubRes( Type type, int name, double reqSkill, object message )
		{
			m_Type = type;
			m_NameNumber = name;
			m_ReqSkill = reqSkill;
			m_Message = message;
		}

		public CraftSubRes( Type type, int name, double reqSkill, int genericNameNumber, object message )
		{
			m_Type = type;
			m_NameNumber = name;
			m_ReqSkill = reqSkill;
			m_GenericNameNumber = genericNameNumber;
			m_Message = message;
		}

		public Type ItemType
		{
			get { return m_Type; }
		}

		public string NameString
		{
			get { return m_NameString; }
		}

		public int NameNumber
		{
			get { return m_NameNumber; }
		}

		public int GenericNameNumber
		{
			get { return m_GenericNameNumber; }
		}

		public object Message
		{
			get { return m_Message; }
		}

		public double RequiredSkill
		{
			get { return m_ReqSkill; }
		}
	}
}�