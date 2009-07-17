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

/* Items/Misc/KeyRing.cs
 * ChangeLog:
 *  5/19/07, Adam
 *      Add support for lockable private houses
 *	11/05/04, Darva
 *			Fixed locking public houses error message.
 *	10/29/04 - Pix
 *		Made keys with keyvalue 0 (blank) not lock/unlock doors when they're on keyrings.
 *    10/23/04, Darva
 *			Added code to prevent locking public houses, but allow currently locked public
 *			houses to be unlocked.
 *	9/4/04, mith
 *		OnDragDrop(): Copied Else block from Spellbook, to prevent people dropping things on book to have it bounce back to original location.
 *	8/26/04, Pix
 *		Made it so keys and keyrings must be in your pack to use.
 *	6/24/04, Pix
 *		KeyRing change - contained keys don't decay (set to non-movable when put on keyring,
 *		and movable when taken off).
 *		Also - GM+ can view the contents of a keyring.
 *	5/18/2004
 *		Added handling of (un)locking/(dis)abling of tinker made traps
 *	5/02/2004, pixie
 *		Changed to be a container...
 *		Now you can doubleclick the keyring, target the keyring, and it'll dump all the keys
 *		into your pack akin to OSI.
 *   4/26/2004, pixie
 *     Initial Version
 */



using System;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Targeting;
using System.Collections;
using Server.Multis;

namespace Server.Items
{
	/// <summary>
	/// Summary description for KeyRing.
	/// </summary>
	public class KeyRing : BaseContainer
	{
		private const int ZEROKEY_ITEMID = 0x1011;
		private const int ONEKEY_ITEMID = 0x1769;
		private const int THREEKEY_ITEMID = 0x176A;
		private const int MANYKEY_ITEMID = 0x176B;
		private const int MAX_KEYS = 10;
		private int m_MaxRange;

		private ArrayList m_Keys;


		[Constructable]
		public KeyRing() : base( 0x1011 )
		{
			//
			// TODO: Add constructor logic here
			//
			m_Keys = new ArrayList(MAX_KEYS);
			m_MaxRange = 3;
		}

		public KeyRing( Serial serial ) : base( serial )
		{
		} 

		public bool IsKeyOnRing(uint keyid)
		{
			bool bReturn = false;
			
			Item[] keys = FindItemsByType( typeof(Key) );
			foreach( Item i in keys )
			{
				if( i is Key )
				{
					Key k = (Key)i;
					if( keyid == k.KeyValue )
					{
						bReturn = true;
						break;
					}
				}
			}

			return bReturn;
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxRange
		{
			get{ return m_MaxRange; }
			set{ m_MaxRange = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Count
		{
			get{ return GetAmount(typeof(Key)); }
		}

		public void UpdateItemID()
		{
			if( Count == 0 )
				ItemID = ZEROKEY_ITEMID;
			else if( Count == 1 || Count == 2 )
				ItemID = ONEKEY_ITEMID;
			else if( Count == 3 || Count == 4 )
				ItemID = THREEKEY_ITEMID;
			else if( Count > 4 )
				ItemID = MANYKEY_ITEMID;
		}

		public override bool OnDragDrop( Mobile from, Item item )
		{
			bool bReturn = false;
			if( item is Key )
			{
				if( Count < MAX_KEYS )
				{
					bReturn = base.OnDragDrop(from, item);
					if( bReturn )
					{
						item.Movable = false;
					}
					UpdateItemID();
				}
				else
				{
					from.SendMessage("That key can't fit on that key ring.");
					bReturn = false;
				}
			}
			else
			{
				// Adam: anything other than a key will get dropped into your backpack
				// (so your best sword doesn't get dropped on the ground.)
				from.AddToBackpack( item );
				//	For richness, we add the drop sound of the item dropped.
				from.PlaySound( item.GetDropSound() );
				return true;
			}

			return bReturn;
		}

		public override void OnDoubleClick( Mobile from )
		{
			Target t;

			if( from.AccessLevel >= AccessLevel.GameMaster )
			{
				base.OnDoubleClick(from);
			}

			if ( Count > 0 )
			{
				t = new RingUnlockTarget( this );
				from.SendMessage( "What do you wish to unlock?" );
				from.Target = t;
			}
			else
			{
				from.SendMessage("The keyring contains no keys");
			}
		}
		
		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );

			string descr = "";
			if( Count > 0 )
			{
				descr = string.Format("{0} keys", Count);
			}
			else
			{
				descr = "Empty";
			}

			this.LabelTo( from, descr );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			if( Count > 0 )
			{
				string descr = string.Format("{0} Keys", Count);
				list.Add( descr );
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			writer.Write( (int) m_MaxRange );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			m_MaxRange = reader.ReadInt();
			if(m_MaxRange == 0)
			{
				m_MaxRange = 3;
			}
		}


		private class RingUnlockTarget : Target
		{
			private KeyRing m_KeyRing;

			public RingUnlockTarget( KeyRing keyring ) : base( keyring.MaxRange, false, TargetFlags.None )
			{
				m_KeyRing = keyring;
				CheckLOS = false;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				int number;

				if ( targeted == m_KeyRing )
				{
					number = -1;
					//remove keys from keyring
					Item[] keys = m_KeyRing.FindItemsByType( typeof(Key) );
					foreach( Item i in keys )
					{
						if( i is Key ) //doublecheck!
						{
							if( from is PlayerMobile)
							{
								Container b = ((PlayerMobile)from).Backpack;
								if( b != null )
								{
                                    b.DropItem(i);
									i.Movable = true;
								}
							}
						}
					}
					m_KeyRing.UpdateItemID();
					from.SendMessage("You remove all the keys.");
				}
				else if ( targeted is ILockable )
				{
					number = -1;

					ILockable o = (ILockable)targeted;

					if( m_KeyRing.IsKeyOnRing( o.KeyValue ) && o.KeyValue != 0 )
					{
						if ( o is BaseDoor && !((BaseDoor)o).UseLocks() )
						{
							number = 501668; // This key doesn't seem to unlock that.
						}
						else if( o is BaseDoor && !(m_KeyRing.IsChildOf( from.Backpack )) )
						{
							from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
						}
						else
						{
							if (o is BaseHouseDoor)
							{
								BaseHouse home; 
								home = ((BaseHouseDoor)o).FindHouse();
								/*if (home.Public == true)
								{
									if (o.Locked != true)
									from.SendMessage("You cannot lock a public house.");
									o.Locked = false;
									return;
								}*/
							}

							o.Locked = !o.Locked;

							if ( o is LockableContainer )
							{
								LockableContainer cont = (LockableContainer)o;

								if( cont.TinkerMadeTrap )
								{
									if( cont.TrapType != TrapType.None )
									{
										if( cont.TrapEnabled )
										{
											from.SendMessage("You leave the trap enabled.");
										}
										else
										{
											from.SendMessage("You leave the trap disabled.");
										}
									}
								}

								if ( cont.LockLevel == -255 )
									cont.LockLevel = cont.RequiredSkill - 10;
							}

							if ( targeted is Item )
							{
								Item item = (Item)targeted;

								if ( o.Locked )
									item.SendLocalizedMessageTo( from, 1048000 );
								else
									item.SendLocalizedMessageTo( from, 1048001 );
							}
						}
					}
					else
					{
						number = 501668; // This key doesn't seem to unlock that.
					}
				}
				else
				{
					number = 501666; // You can't unlock that!
				}

				if ( number != -1 )
				{
					from.SendLocalizedMessage( number );
				}
			}
		}//end RingUnlockTarget



	}
}
