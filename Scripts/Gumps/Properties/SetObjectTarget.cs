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

/* /Scripts/Gumps/Properties/SetObjectTarget.cs
 * Changelog:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Reflection;
using System.Collections;
using Server;
using Server.Items;
using Server.Targeting;

namespace Server.Gumps
{
	public class SetObjectTarget : Target
	{
		private PropertyInfo m_Property;
		private Mobile m_Mobile;
		private object m_Object;
		private Stack m_Stack;
		private Type m_Type;
		private int m_Page;
		private ArrayList m_List;

		public SetObjectTarget( PropertyInfo prop, Mobile mobile, object o, Stack stack, Type type, int page, ArrayList list ) : base( -1, false, TargetFlags.None )
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Type = type;
			m_Page = page;
			m_List = list;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			try
			{
				if ( m_Type == typeof( Type ) )
					targeted = targeted.GetType();
				else if ( (m_Type == typeof( BaseAddon ) || m_Type.IsAssignableFrom( typeof( BaseAddon ) )) && targeted is AddonComponent )
					targeted = ((AddonComponent)targeted).Addon;

				if ( m_Type.IsAssignableFrom( targeted.GetType() ) )
				{
					Server.Scripts.Commands.CommandLogging.LogChangeProperty( m_Mobile, m_Object, m_Property.Name, targeted.ToString() );
					m_Property.SetValue( m_Object, targeted, null );
				}
				else
				{
					m_Mobile.SendMessage( "That cannot be assigned to a property of type : {0}", m_Type.Name );
				}
			}
			catch
			{
				m_Mobile.SendMessage( "An exception was caught. The property may not have changed." );
			}
		}

		protected override void OnTargetFinish( Mobile from )
		{
			if ( m_Type == typeof( Type ) )
				from.SendGump( new PropertiesGump( m_Mobile, m_Object, m_Stack, m_List, m_Page ) );
			else
				from.SendGump( new SetObjectGump( m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List ) );
		}
	}
}