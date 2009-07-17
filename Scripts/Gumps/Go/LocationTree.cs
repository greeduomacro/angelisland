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
using System.IO;
using System.Xml;
using System.Collections;
using Server;

namespace Server.Gumps
{
	public class LocationTree
	{
		private Map m_Map;
		private ParentNode m_Root;
		private Hashtable m_LastBranch;

		public LocationTree( string fileName, Map map )
		{
			m_LastBranch = new Hashtable();
			m_Map = map;

			string path = Path.Combine( "Data/Locations/", fileName );

			if ( File.Exists( path ) )
			{
				XmlTextReader xml = new XmlTextReader( new StreamReader( path ) );

				xml.WhitespaceHandling = WhitespaceHandling.None;

				m_Root = Parse( xml );

				xml.Close();
			}
		}

		public Hashtable LastBranch
		{
			get
			{
				return m_LastBranch;
			}
		}

		public Map Map
		{
			get
			{
				return m_Map;
			}
		}

		public ParentNode Root
		{
			get
			{
				return m_Root;
			}
		}

		private ParentNode Parse( XmlTextReader xml )
		{
			xml.Read();
			xml.Read();
			xml.Read();

			return new ParentNode( xml, null );
		}
	}
} 
