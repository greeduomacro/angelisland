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

/* Items/Body Parts/Head.cs
 * CHANGELOG:
 *  01/20/06 Taran Kain
 *		Changed cast in loading PlayerMobile to make sure we're not creating an invalid cast
 *	5/16/04, Pixie
 *		Head now contains information for Bounty system.
 */

using System;
using Server;

using Server.Mobiles;

namespace Server.Items
{
	public class Head : Item, ICarvable
	{
		public override TimeSpan DecayTime
		{
			get
			{
				return TimeSpan.FromMinutes(15.0);
			}
		}

		private PlayerMobile player;
		private DateTime created;

		[Constructable]
		public Head() : this( null )
		{
			player = null;
			created = DateTime.Now;
		}

		[Constructable]
		public Head( string name ) : base( 0x1DA0 )
		{
			Name = name;
			Weight = 1.0;
			player = null;
			created = DateTime.Now;
		}

		public Head( string name, PlayerMobile m ) : base( 0x1DA0 )
		{
			Name = name;
			Weight = 2.0;
			player = m;
			created = DateTime.Now;
		}

		public Head( Serial serial ) : base( serial )
		{
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool IsPlayerHead
		{
			get
			{
				if( player == null )
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public PlayerMobile Player
		{
			get{ return player; }
			set
			{ 
				player = value;
				Name = String.Format( "the head of {0}", player.Name );
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime Time
		{
			get{ return created; }
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			if( player != null )
			{
				writer.Write( (int) 1 );
				writer.Write( player );
			}
			else
			{
				writer.Write( (int) 0 );
			}
			writer.Write( created );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch( version )
			{
				case 1:
				{
					int iContainsMobile = reader.ReadInt();
					if( iContainsMobile == 1 )
					{
						// don't want to use C-style hard cast here, as ReadMobile can return a valid Mobile that's not a PlayerMobile
						// as cast will just make it null, in that case
						// Note from adam: It's also possible that it's a PM that's loaded, but a *different* PM.. don't know how to handle this atm. Not critical.
						player = reader.ReadMobile() as PlayerMobile;
					}
					created = reader.ReadDateTime();
					goto case 0;
				}
				case 0:
				{
					break;
				}
			}
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
			//add brain			//add skull
			if (Player == null)
			{
				if (this.ParentContainer == null)
				{
					new Brain().MoveToWorld(loc, Map);
					new Skull().MoveToWorld(loc, Map);
				}
				else
				{
					this.ParentContainer.DropItem(new Brain());
					this.ParentContainer.DropItem(new Skull());
				}
			}
			else
			{
				if (this.ParentContainer == null)
				{
					new Brain(Player.Name).MoveToWorld(loc, Map);
					new Skull(Player.Name).MoveToWorld(loc, Map);
				}
				else
				{
					this.ParentContainer.DropItem(new Brain(Player.Name));
					this.ParentContainer.DropItem(new Skull(Player.Name));
				}
			}

			this.Delete();
		}

		#endregion
	}
}