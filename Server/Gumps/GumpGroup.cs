/***************************************************************************
 *                                GumpGroup.cs
 *                            -------------------
 *   begin                : May 1, 2002
 *   copyright            : (C) The RunUO Software Team
 *   email                : info@runuo.com
 *
 *   $Id: GumpGroup.cs,v 1.1 2005/02/22 00:58:07 adam Exp $
 *   $Author: adam $
 *   $Date: 2005/02/22 00:58:07 $
 *
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
using Server.Network;

namespace Server.Gumps
{
	public class GumpGroup : GumpEntry
	{
		private int m_Group;

		public GumpGroup( int group )
		{
			m_Group = group;
		}

		public int Group
		{
			get
			{
				return m_Group;
			}
			set
			{
				Delta( ref m_Group, value );
			}
		}

		public override string Compile()
		{
			return String.Format( "{{ group {0} }}", m_Group );
		}

		private static byte[] m_LayoutName = Gump.StringToBuffer( "group" );

		public override void AppendTo( DisplayGumpFast disp )
		{
			disp.AppendLayout( m_LayoutName );
			disp.AppendLayout( m_Group );
		}
	}
}