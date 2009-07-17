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

/* /Scripts/Multis/Tent/SiegeTent.cs
 * ChangeLog:
 *	08/14/06, weaver
 *		Modified component construction to pass BaseHouse reference to the backpack.
 *	05/22/06, weaver
 *		Added initial 24 hour decay time.
 *		Added overrides to disable refreshing.
 *		Set default price to 0
 *	05/18/06, weaver
 *		Initial creation. 
 */

using System;
using Server;
using Server.Items;
using Server.Gumps;
using Server.Multis.Deeds;

namespace Server.Multis
{
	
	public class SiegeTent : BaseHouse
	{
		public static Rectangle2D[] AreaArray = new Rectangle2D[]{ new Rectangle2D(-3,-3,7,7 ), new Rectangle2D( -1, 4, 3, 1 ) };
		public override Rectangle2D[] Area{ get{ return AreaArray; } }
		public override int DefaultPrice{ get{ return 0; } }
		public override HousePlacementEntry ConvertEntry{ get{ return HousePlacementEntry.TwoStoryFoundations[0]; } }
		
		private TentBedRoll m_Tentbed;
		private TentBackpack m_Tentpack;

		public TentBackpack Tentpack
		{
			get
			{
				return m_Tentpack;
			}
		}

		private int m_RoofHue;

		public SiegeTent( Mobile owner, int Hue ) : base ( 0xFFE, owner, 270, 2, 2 )
		{
			m_RoofHue = Hue;

			// wea: this gets called after the base class, overriding it 
			DecayMinutesStored = 60 * 24;   // 24 hours!!
		}

		public SiegeTent( Serial serial ) : base( serial )
		{
		}

		public void GenerateTent()
		{
			TentWalls walls = new TentWalls( TentStyle.Siege );
			TentRoof roof = new TentRoof( m_RoofHue );
			TentFloor floor = new TentFloor();
			            						
			walls.MoveToWorld( this.Location, this.Map );
			roof.MoveToWorld( this.Location, this.Map );
			floor.MoveToWorld( this.Location, this.Map );
			
			Addons.Add( walls );
			Addons.Add( roof );
			Addons.Add( floor );

			// Create tent bed
			m_Tentbed = new TentBedRoll( this );
			m_Tentbed.MoveToWorld( new Point3D( this.X, this.Y + 1, this.Z ), this.Map );
			m_Tentbed.Movable = false;

			// Create secute tent pack within the tent
			m_Tentpack = new TentBackpack( this );
			m_Tentpack.MoveToWorld( new Point3D( this.X-1, this.Y-1, this.Z), this.Map );
			SecureInfo info = new SecureInfo( (Container) m_Tentpack, SecureLevel.Anyone );
			m_Tentpack.IsSecure = true;
			this.Secures.Add( info );
			m_Tentpack.Movable = false;
			m_Tentpack.Hue = m_RoofHue;
		}

		public override void MoveToWorld( Point3D location, Map map )
		{
			base.MoveToWorld( location, map );
			GenerateTent();
		}

		public override void OnDelete()
		{
			m_Tentbed.Delete();
			m_Tentpack.Delete();
			base.OnDelete();
		}

		public override HouseDeed GetDeed() 
		{
			return new SiegeTentBag();
		}

		// Override standard decay handling so no refresh takes place

		public override void Refresh()
		{
			return;
		}

		public override void RefreshHouseOneDay()
		{
			return;
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)0 );//version

			writer.Write( m_Tentbed );
			writer.Write( m_Tentpack );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			m_Tentbed = (TentBedRoll) reader.ReadItem();
			m_Tentpack = (TentBackpack) reader.ReadItem();
		}
	}
}
