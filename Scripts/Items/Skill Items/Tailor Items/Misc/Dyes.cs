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
using Server.Targeting;
using Server.HuePickers;

namespace Server.Items
{
	public class Dyes : Item
	{
		[Constructable]
		public Dyes() : base( 0xFA9 )
		{
			Weight = 3.0;
		}

		public Dyes( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( Weight == 0.0 )
				Weight = 3.0;
		}

		public override void OnDoubleClick( Mobile from )
		{
			from.SendLocalizedMessage( 500856 ); // Select the dye tub to use the dyes on.
			from.Target = new InternalTarget();
		}

		private class InternalTarget : Target
		{
			public InternalTarget() : base( 1, false, TargetFlags.None )
			{
			}

			private class InternalPicker : HuePicker
			{
				private DyeTub m_Tub;

				public InternalPicker( DyeTub tub ) : base( tub.ItemID )
				{
					m_Tub = tub;
				}

				public override void OnResponse( int hue )
				{
					m_Tub.DyedHue = hue;
				}
			}

			private static void SetTubHue( Mobile from, object state, int hue )
			{
				((DyeTub)state).DyedHue = hue;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is DyeTub )
				{
					DyeTub tub = (DyeTub) targeted;

					if ( tub.Redyable )
					{
						if ( tub.CustomHuePicker == null )
							from.SendHuePicker( new InternalPicker( tub ) );
						else
							from.SendGump( new CustomHuePickerGump( from, tub.CustomHuePicker, new CustomHuePickerCallback( SetTubHue ), tub ) );
					}
					else if ( tub is BlackDyeTub )
					{
						from.SendLocalizedMessage( 1010092 ); // You can not use this on a black dye tub.
					}
					else
					{
						from.SendMessage( "That dye tub may not be redyed." );
					}
				}
				else
				{
					from.SendLocalizedMessage( 500857 ); // Use this on a dye tub.
				}
			}
		}
	}
}�