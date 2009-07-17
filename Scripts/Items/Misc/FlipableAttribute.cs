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

/* Items/Misc/FlipableAttribute.cs
 * ChangeLog:
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using System.Reflection;
using Server.Targeting;

namespace Server.Items
{
	public class FlipCommandHandlers
	{
		public static void Initialize()
		{
			Server.Commands.Register( "Flip", AccessLevel.GameMaster, new CommandEventHandler( Flip_OnCommand ) );
		}

		[Usage( "Flip" )]
		[Description( "Turns an item." )]
		public static void Flip_OnCommand( CommandEventArgs e )
		{
			e.Mobile.Target = new FlipTarget();
		}

		private class FlipTarget : Target
		{
			public FlipTarget() : base( -1, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Item )
				{
					Item item = (Item)targeted;

					if ( item.Movable == false && from.AccessLevel == AccessLevel.Player )
						return;

					Type type = targeted.GetType();    

					FlipableAttribute [] AttributeArray = (FlipableAttribute []) type.GetCustomAttributes(typeof(FlipableAttribute), false);
            
					if( AttributeArray.Length == 0 )
					{
						return ;
					}

					FlipableAttribute fa = AttributeArray[0];

					fa.Flip( (Item)targeted);
				}
			}
		}
	}

	[AttributeUsage( AttributeTargets.Class )]
	public class DynamicFlipingAttribute : Attribute
	{
		public DynamicFlipingAttribute()
		{
		}
	}

	[AttributeUsage( AttributeTargets.Class )]
	public class FlipableAttribute : Attribute
	{
		private int[] m_ItemIDs;

		public int[] ItemIDs
		{
			get{ return m_ItemIDs; }
		}

		public FlipableAttribute() : this ( null )
		{
		}
		
		public FlipableAttribute( params int[] itemIDs )
		{
			m_ItemIDs = itemIDs;
		}

		public virtual void Flip( Item item )
		{
			if ( m_ItemIDs == null )
			{
				try
				{
					MethodInfo flipMethod = item.GetType().GetMethod( "Flip", Type.EmptyTypes );
					if ( flipMethod != null )
						flipMethod.Invoke( item, new object[0] );
				}
				catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }

			}
			else
			{
				int index = 0;
				for ( int i = 0; i < m_ItemIDs.Length; i++ )
				{
					if ( item.ItemID == m_ItemIDs[i] )
					{
						index = i + 1;
						break;
					}
				}

				if ( index > m_ItemIDs.Length - 1)
					index = 0;
				
				item.ItemID = m_ItemIDs[index];
			}
		}
	}
}
