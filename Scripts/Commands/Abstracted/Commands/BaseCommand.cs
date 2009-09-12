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

/* Scripts/Commands/Abstracted/Commands/BaseCommand.cs
 * CHANGELOG
 *	1/3/09, Adam
 *		Remove Begin() and End() calls from around ExecuteList() and move them to 
 *			the RunCommand in BaseCommandImplementor so they get called for Target as well as Area
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */


using System;
using System.Collections;
using Server;
using Server.Gumps;

namespace Server.Scripts.Commands
{
	public enum ObjectTypes
	{
		Both,
		Items,
		Mobiles,
		All
	}

	public abstract class BaseCommand
	{
		private string[] m_Commands;
		private AccessLevel m_AccessLevel;
		private CommandSupport m_Implementors;
		private ObjectTypes m_ObjectTypes;
		private bool m_ListOptimized;
		private string m_Usage;
		private string m_Description;

		public bool ListOptimized
		{
			get { return m_ListOptimized; }
			set { m_ListOptimized = value; }
		}

		public string[] Commands
		{
			get { return m_Commands; }
			set { m_Commands = value; }
		}

		public string Usage
		{
			get { return m_Usage; }
			set { m_Usage = value; }
		}

		public string Description
		{
			get { return m_Description; }
			set { m_Description = value; }
		}

		public AccessLevel AccessLevel
		{
			get { return m_AccessLevel; }
			set { m_AccessLevel = value; }
		}

		public ObjectTypes ObjectTypes
		{
			get { return m_ObjectTypes; }
			set { m_ObjectTypes = value; }
		}

		public CommandSupport Supports
		{
			get { return m_Implementors; }
			set { m_Implementors = value; }
		}

		public BaseCommand()
		{
			m_Responses = new ArrayList();
			m_Failures = new ArrayList();
		}

		public static bool IsAccessible(Mobile from, object obj)
		{
			if (from.AccessLevel >= AccessLevel.Administrator || obj == null)
				return true;

			Mobile mob;

			if (obj is Mobile)
				mob = (Mobile)obj;
			else if (obj is Item)
				mob = ((Item)obj).RootParent as Mobile;
			else
				mob = null;

			if (mob == null || mob == from || from.AccessLevel > mob.AccessLevel)
				return true;

			return false;
		}

		public virtual void Begin(CommandEventArgs e) { }
		public virtual void End(CommandEventArgs e) { }

		public virtual void ExecuteList(CommandEventArgs e, ArrayList list)
		{
			for (int i = 0; i < list.Count; ++i)
				Execute(e, list[i]);
		}

		public virtual void Execute(CommandEventArgs e, object obj)
		{
		}

		public virtual bool ValidateArgs(BaseCommandImplementor impl, CommandEventArgs e)
		{
			return true;
		}

		private ArrayList m_Responses, m_Failures;

		private class MessageEntry
		{
			public string m_Message;
			public int m_Count;

			public MessageEntry(string message)
			{
				m_Message = message;
				m_Count = 1;
			}

			public override string ToString()
			{
				if (m_Count > 1)
					return String.Format("{0} ({1})", m_Message, m_Count);

				return m_Message;
			}
		}

		public void AddResponse(string message)
		{
			for (int i = 0; i < m_Responses.Count; ++i)
			{
				MessageEntry entry = (MessageEntry)m_Responses[i];

				if (entry.m_Message == message)
				{
					++entry.m_Count;
					return;
				}
			}

			if (m_Responses.Count == 10)
				return;

			m_Responses.Add(new MessageEntry(message));
		}

		public void AddResponse(Gump gump)
		{
			m_Responses.Add(gump);
		}

		public void LogFailure(string message)
		{
			for (int i = 0; i < m_Failures.Count; ++i)
			{
				MessageEntry entry = (MessageEntry)m_Failures[i];

				if (entry.m_Message == message)
				{
					++entry.m_Count;
					return;
				}
			}

			if (m_Failures.Count == 10)
				return;

			m_Failures.Add(new MessageEntry(message));
		}

		public void Flush(Mobile from, bool flushToLog)
		{
			if (m_Responses.Count > 0)
			{
				for (int i = 0; i < m_Responses.Count; ++i)
				{
					object obj = m_Responses[i];

					if (obj is MessageEntry)
					{
						from.SendMessage(((MessageEntry)obj).ToString());

						if (flushToLog)
							CommandLogging.WriteLine(from, ((MessageEntry)obj).ToString());
					}
					else if (obj is Gump)
					{
						from.SendGump((Gump)obj);
					}
				}
			}
			else
			{
				for (int i = 0; i < m_Failures.Count; ++i)
					from.SendMessage(((MessageEntry)m_Failures[i]).ToString());
			}

			m_Responses.Clear();
			m_Failures.Clear();
		}
	}
}