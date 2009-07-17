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

/* /Scripts/Gumps/Properties/SetCustomEnumGump.cs
 * Changelog:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Reflection;
using System.Collections;
using Server;
using Server.Network;

namespace Server.Gumps
{
	public class SetCustomEnumGump : SetListOptionGump
	{
		private string[] m_Names;

		public SetCustomEnumGump( PropertyInfo prop, Mobile mobile, object o, Stack stack, int propspage, ArrayList list, string[] names ) : base( prop, mobile, o, stack, propspage, list, names, null )
		{
			m_Names = names;
		}

		public override void OnResponse( NetState sender, RelayInfo relayInfo )
		{
			int index = relayInfo.ButtonID - 1;

			if ( index >= 0 && index < m_Names.Length )
			{
				try
				{
					MethodInfo info = m_Property.PropertyType.GetMethod( "Parse", new Type[]{ typeof( string ) } );

					Server.Scripts.Commands.CommandLogging.LogChangeProperty( m_Mobile, m_Object, m_Property.Name, m_Names[index] );

					if ( info != null )
						m_Property.SetValue( m_Object, info.Invoke( null, new object[]{ m_Names[index] } ), null );
					else if ( m_Property.PropertyType == typeof( Enum ) || m_Property.PropertyType.IsSubclassOf( typeof( Enum ) ) )
						m_Property.SetValue( m_Object, Enum.Parse( m_Property.PropertyType, m_Names[index], false ), null );
				}
				catch
				{
					m_Mobile.SendMessage( "An exception was caught. The property may not have changed." );
				}
			}

			m_Mobile.SendGump( new PropertiesGump( m_Mobile, m_Object, m_Stack, m_List, m_Page ) );
		}
	}
}