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
	public class CraftRes
	{
		private Type m_Type;
		private int m_Amount;

		private string m_MessageString;
		private int m_MessageNumber;

		private string m_NameString;
		private int m_NameNumber;

		public CraftRes( Type type, int amount )
		{
			m_Type = type;
			m_Amount = amount;
		}

		public CraftRes( Type type, int name, int amount, int message ) : this( type, amount )
		{
			m_NameNumber = name;
			m_MessageNumber = message;
		}

		public CraftRes( Type type, int name, int amount, string message ) : this( type, amount )
		{
			m_NameNumber = name;
			m_MessageString = message;
		}

		public CraftRes( Type type, string name, int amount, int message ) : this( type, amount )
		{
			m_NameString = name;
			m_MessageNumber = message;
		}

		public CraftRes( Type type, string name, int amount, string message ) : this( type, amount )
		{
			m_NameString = name;
			m_MessageString = message;
		}

		public void SendMessage( Mobile from )
		{
			if ( m_MessageNumber > 0 )
				from.SendLocalizedMessage( m_MessageNumber );
			else if ( m_MessageString != null && m_MessageString != String.Empty )
				from.SendMessage( m_MessageString );
			else
				from.SendLocalizedMessage( 502925 ); // You don't have the resources required to make that item.
		}

		public Type ItemType
		{
			get { return m_Type; }
		}

		public string MessageString
		{
			get { return m_MessageString; }
		}

		public int MessageNumber
		{
			get { return m_MessageNumber; }
		}

		public string NameString
		{
			get { return m_NameString; }
		}

		public int NameNumber
		{
			get { return m_NameNumber; }
		}

		public int Amount
		{
			get { return m_Amount; }
		}
	}
}�