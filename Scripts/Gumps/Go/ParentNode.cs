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
using System.Xml;
using System.Collections;
using Server;

namespace Server.Gumps
{
	public class ParentNode
	{
		private ParentNode m_Parent;
		private object[] m_Children;

		private string m_Name;

		public ParentNode( XmlTextReader xml, ParentNode parent )
		{
			m_Parent = parent;

			Parse( xml );
		}

		private void Parse( XmlTextReader xml )
		{
			if ( xml.MoveToAttribute( "name" ) )
				m_Name = xml.Value;
			else
				m_Name = "empty";

			if ( xml.IsEmptyElement )
			{
				m_Children = new object[0];
			}
			else
			{
				ArrayList children = new ArrayList();

				while ( xml.Read() && xml.NodeType == XmlNodeType.Element )
				{
					if ( xml.Name == "child" )
					{
						ChildNode n = new ChildNode( xml, this );

						children.Add( n );
					}
					else
					{
						children.Add( new ParentNode( xml, this ) );
					}
				}

				m_Children = children.ToArray();
			}
		}

		public ParentNode Parent
		{
			get
			{
				return m_Parent;
			}
		}

		public object[] Children
		{
			get
			{
				return m_Children;
			}
		}

		public string Name
		{
			get
			{
				return m_Name;
			}
		}
	}
} 
