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

/* Scripts\Multis\HouseSign.cs
 * CHANGELOG
 *	9/2/07, Adam
 *		Add a auto-resume-decay system so that we can feeze for a set amount of time.
 *	7/27/07, Adam
 *		Add SuppressRegion property to turn on region suppression 
 *	6/25/07, Adam
 *		make StaticHousingSign Constructable
 *	6/25/06, Adam
 *		Add StaticHousingSign for use on static build houses before they are captured
 *			and converted into a new StaticHouse
 *	2/21/06, Pix
 *		Added SecurePremises flag.
 *	8/28/05, Pix
 *		Made FreezeDecay property's logic actually correct ;-p
 *	8/25/05, Pix
 *		Change the NeverDecay property to FreezeDecay (made logic easier to see).
 *		Changed to be readable by councellor+ and settable by Admin.
 *	8/4/05, Pix
 *		Change to house decay.
 *	6/11/05, Pix
 *		Added BanLocation to set/see the Ban Location of a house.
 *	6/10/04, Pix
 *		Changes for House decay
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Multis;
using Server.Gumps;

namespace Server.Multis
{
  
	public class HouseSign : Item
	{
		private BaseHouse m_Owner;
		private Mobile m_OrgOwner;

		public HouseSign( BaseHouse owner ) : base( 0xBD2 )
		{
			m_Owner = owner;
			m_OrgOwner = m_Owner.Owner;
			Name = "a house sign";
			Movable = false;
		}

		public HouseSign( Serial serial ) : base( serial )
		{
		}

		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );

			if( from.AccessLevel >= AccessLevel.GameMaster )
			{
				base.LabelTo( from, "[GM Info Only: Collapse Time: {0}]", DateTime.Now + TimeSpan.FromMinutes(m_Owner.DecayMinutesStored) );
			}

			base.LabelTo( from, m_Owner.DecayState() );
		}

        [CommandProperty(AccessLevel.GameMaster)]
		public BaseHouse Owner
		{
			get
			{
				return m_Owner;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool SecurePremises
		{
			get
			{
				return m_Owner.SecurePremises;
			}
			set
			{
				m_Owner.SecurePremises = value;
			}
		}

		[CommandProperty(AccessLevel.Seer, AccessLevel.Seer)]
		public bool SuppressRegion
		{
			get
			{
				return m_Owner.SuppressRegion;
			}
			set
			{
				m_Owner.SuppressRegion = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public bool FreezeDecay
		{
			get
			{
				return m_Owner.m_NeverDecay;
			}
			set
			{
				m_Owner.m_NeverDecay = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public TimeSpan RestartDecay
		{
			get
			{
				return m_Owner.RestartDecay;
			}
			set
			{
				m_Owner.RestartDecay = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Point3D BanLocation
		{
			get
			{
				return m_Owner.BanLocation;
			}
			set
			{
				m_Owner.SetBanLocation( value );
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public double HouseDecayMinutesStored
		{
			get
			{ 
				return m_Owner.DecayMinutesStored;
			}
			set
			{ 
				m_Owner.DecayMinutesStored = value;
			}
		}


		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile OriginalOwner
		{
			get
			{
				return m_OrgOwner;
			}
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_Owner != null && !m_Owner.Deleted )
				m_Owner.Delete();
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add( 1061638 ); // A House Sign
		}

		public override bool ForceShowProperties
		{ 
			get
			{ 
				return ObjectPropertyList.Enabled; 
			} 
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			list.Add( 1061639, Name == "a house sign" ? "nothing" : Utility.FixHtml( Name ) ); // Name: ~1_NAME~
			list.Add( 1061640, (m_Owner == null || m_Owner.Owner == null) ? "nobody" : m_Owner.Owner.Name ); // Owner: ~1_OWNER~

			if ( m_Owner != null )
				list.Add( m_Owner.Public ? 1061641 : 1061642 ); // This House is Open to the Public : This is a Private Home
		}

		public override void OnDoubleClick( Mobile m )
		{
			if ( m_Owner != null )
			{
				if ( m_Owner.IsFriend( m ) )
					m.SendLocalizedMessage( 501293 ); // Welcome back to the house, friend!

				if ( m_Owner.IsAosRules )
					m.SendGump( new HouseGumpAOS( HouseGumpPageAOS.Information, m, m_Owner ) );
				else
					m.SendGump( new HouseGump( m, m_Owner ) );
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Owner );
			writer.Write( m_OrgOwner );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				/*case 1:
				{

					goto case 0;
				}*/
				case 0:
				{
					m_Owner = reader.ReadItem() as BaseHouse;
					m_OrgOwner = reader.ReadMobile();

					break;
				}
			}
		}
	}

	public class StaticHouseSign : Item
	{
		private Mobile m_OrgOwner;
		private DateTime m_BuiltOn;

		[Constructable]
		public StaticHouseSign()
			: base(0xBD2)
		{
			m_OrgOwner = null;
			Name = "a static house sign";
			Movable = false;
			m_BuiltOn = DateTime.Now;
		}

		public StaticHouseSign(Serial serial)
			: base(serial)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile OriginalOwner
		{
			get { return m_OrgOwner; }
			set { m_OrgOwner = value; }
		}

		public DateTime BuiltOn
		{
			get { return m_BuiltOn; }
			set { m_BuiltOn = value; }
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add(1061638); // A House Sign
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version

			writer.Write(m_OrgOwner);
			writer.Write(m_BuiltOn);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
				{
					m_OrgOwner = reader.ReadMobile();
					m_BuiltOn = reader.ReadDateTime();
					break;
				}
			}
		}
	}
}