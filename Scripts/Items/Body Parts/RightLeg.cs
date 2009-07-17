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
using Server;

namespace Server.Items
{
	public class RightLeg : Item, ICarvable
	{
		private IOBAlignment m_IOBAlignment = IOBAlignment.None;
		public IOBAlignment IOBAlignment { get { return m_IOBAlignment; } }
		public override TimeSpan DecayTime
		{
			get
			{
				return TimeSpan.FromMinutes(15.0);
			}
		}

		[Constructable]
		public RightLeg() : base( 0x1DA4 )
		{
		}

		public RightLeg(IOBAlignment iob)
			: this()
		{
			m_IOBAlignment = iob;
		}

		public RightLeg( Serial serial ) : base( serial )
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
		}

		#region ICarvable Members

		void ICarvable.Carve(Mobile from, Item item)
		{
			Point3D loc = this.Location;
			if (this.ParentContainer != null)
			{
				if (this.ParentMobile != null)
				{
					if (this.ParentMobile != from)
					{
						from.SendMessage("You can't carve that there");
						return;
					}

					loc = this.ParentMobile.Location;
				}
				else
				{
					loc = this.ParentContainer.Location;
					if (!from.InRange(loc, 1))
					{
						from.SendMessage("That is too far away.");
						return;
					}
				}
			}

			//add blood
			Blood blood = new Blood(Utility.Random(0x122A, 5), Utility.Random(15 * 60, 5 * 60));
			blood.MoveToWorld(loc, Map);
			//add meat
			Jerky jerky = new Jerky(m_IOBAlignment);
			if (this.ParentContainer == null)
			{
				jerky.MoveToWorld(loc, Map);
			}
			else
			{
				this.ParentContainer.DropItem(jerky);
			}

			this.Delete();
		}

		#endregion
	}
}