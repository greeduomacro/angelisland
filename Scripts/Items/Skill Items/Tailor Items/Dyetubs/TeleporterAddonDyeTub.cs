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

/* Scripts\Items\Skill Items\Tailor Items\Dyetubs\TeleporterAddonDyeTub.cs
 * CHANGELOG:
 *	9/14/06 - Pixie
 *		Initial Version
 */

using System;
using Server;
using Server.Multis;
using Server.Targeting;

namespace Server.Items
{
	public class TeleporterAddonDyeTub : DyeTub
	{
		public override bool AllowDyables{ get{ return false; } }
		public override bool AllowRunebooks{ get{ return false; } }

		public override CustomHuePicker CustomHuePicker{ get{ return CustomHuePicker.LeatherDyeTub; } }

		private int m_Uses;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Uses
		{
			get { return m_Uses; }
			set { m_Uses = value; }
		}

		[Constructable]
		public TeleporterAddonDyeTub()
		{
			m_Uses = 10;
			Name = "teleporter dye tub";
		}

		public override void OnSingleClick( Mobile from )
		{
			this.LabelTo( from, "teleporter dye tub" );
			this.LabelTo( from, string.Format("{0} uses remaining", this.m_Uses) );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 1 ) )
			{
				from.SendMessage( "Target the teleporter to paint." );
				from.Target = new InternalTarget( this );
			}
			else
			{
				from.SendLocalizedMessage( 500446 ); // That is too far away.
			}
		}

		public TeleporterAddonDyeTub( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (int) m_Uses );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_Uses = reader.ReadInt();
					break;
				}
			}
		}

	
		private class InternalTarget : Target
		{
			private TeleporterAddonDyeTub m_Tub;

			public InternalTarget( TeleporterAddonDyeTub tub ) : base( 1, false, TargetFlags.None )
			{
				m_Tub = tub;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is TeleporterAC )
				{
					Item item = (Item)targeted;

					if ( !from.InRange( m_Tub.GetWorldLocation(), 1 ) || !from.InRange( item.GetWorldLocation(), 1 ) )
					{
						from.SendLocalizedMessage( 500446 ); // That is too far away.
					}
					else
					{
						bool okay = false;

						BaseHouse house = BaseHouse.FindHouseAt( item );

						if ( house == null )
							from.SendMessage( "The house seems to be missing." );
						else if ( !house.IsCoOwner( from ) )
							from.SendLocalizedMessage( 501023 ); // You must be the owner to use this item.
						else
							okay = true;

						if ( okay )
						{
							m_Tub.m_Uses--;
							item.Hue = m_Tub.DyedHue;
							from.PlaySound( 0x23E );

							if( m_Tub.m_Uses <= 0 )
							{
								m_Tub.Delete();
							}
						}
					}
				}
				else
				{
					from.SendMessage( "That is not a teleporter." );
				}
			}
		}
	
	
	}
}
