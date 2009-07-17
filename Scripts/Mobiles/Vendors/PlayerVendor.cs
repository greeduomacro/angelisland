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

/* Scripts/Mobiles/Vendors/PlayerVendor.cs
 * CHANGELOG
 *	1/22/08, Adam
 *		Credit the landlord if they are a Shopkeeper and one of their RentalVendors sells an item
 *      - Vendors will no longer allow you to move something if you cannot the restock charges
 *          "I am not holding enough gold to cover the restock charges."
 *	1/20/08, Adam
 *		Add support for the new Shopkeeper skill
 *  1/14/08, Adam
 *      - Add support for Commission based PricingModel
 *		- Add a random delay to PayTimer() of up to a full UODay(5 * 24) to prevent all vendors from firing at the same time (server lag prevention)
 *  1/13/08, Adam
 *      scale price based on server population. Example:
 *      100 player online = 100% of usual charges
 *      90 online = 90% of usual charges
 *      50 online = 50% of usual charges
 *  12/17/07, Adam
 *      - replace old check for AccessLevel >= Administrator and replace it with the StaffOwned property
 *      - Don't drop StaffOwned() goods if the vendor drops (i.e., if the staff char is deleted.)
 *	11/26/07, Adam
 *		Fix Vendor Exploit:
 *		1. in ChargePerDay(): If the items total more than a signed int can hold (2+billion) the system logs the error 
 *			to VendorExploit.log and sets the total = int.MaxValue whereby hitting the vendor owner with max fees (4,294,986)
 *		2. prevent item additions from reaching signed integer overflow by capping prices if the total inventory > 100,000,000
 *		3. Do not factor in not-for-sale items in ChargePerDay() as they have a price of -1 whereby reducing normal fees
 *	7/5/07, Adam
 *		Add automatic pricing of StaticDeeds (new housing system) for admin chars.
 *		This was done to avoid errors introduced by human error.
 *	7/01/07, Pix
 *		Removed charging if this vendor is owned by an admin.
 *  5/18/07, Adam
 *      Add ValidSellItems property to force check and report corruption
 *	3/25/07, Pix
 *		Added LogHelper stuff to ValidateSell items so we can see how big a problem this is.
 *	3/22/07, Pix
 *		Re-added ValidateSellItems (there's no harm in leaving safety checks!):
 *		now we validate the items in the sell list before we calculate the fees.
 *	2/27/07, Pix
 *		Added PlayerVendorStaffCommands - commands that let staff diagnose problems.
 *  07/18/06, Kit
 *		Added Canbeharmful override to prevent explosion potions from still causeing
 *		aggres record even when unable to deal damage.
 *  07/02/06, Kit
 *		InitOutfit/Body overrides
 *  06/16/05, Kit
 *		Merged in vendor code from 1.0 for drag/drop of items fixing phantom item issue
 *		removed validateSellItems()
 *  06/12/05 Taran Kain
 *		Added ValidateSellItems() to filter out bogus entries before charging fee
 *		Added check to see if we can add item to pack before adding to SellItems list in CheckNonlocalDrop() to prevent phantom items in the first place
 *  06/03/05, TK
 *		In ClothesUpdate, changed ln 282 to check for null instead of check if is PlayerVendor
 *		m is always PlayerVendor, may sometimes be null
 *	04/22/05, Kitaras
 *		fixed bug in clothesupdate()
 *	04/22/05, Kitaras
 *		Changed previous change to only hue footware hue 0 on creation leaving other cloths.
 *	04/22/05, Kitaras
 *		Changed cloths back to normal, however all outfits are now wiped and set to hue 0 on creation.
 *	04/17/05, Kitaras
 *		Added LOS check for equipping/unequipping items from player vendors.
 *	04/16/05, Kitaras	
 *		Added BaseHouse to be passed via constructor, added various functions, House, CollectGold,
 *		 GiveGold, GetItems, Destroy, CanInteractWith to support rental vendor contracts. Updated Dismiss to call
 *		 destroy function, updated displaypaperdoll, allowed owners to dress their vendors via paperdoll, set basic cloths
 *		 to loot type of newbie and unmovable so as to never drop when vendor is dismissed
 *	03/22/05, erlein
 *		Altered OnSingleClickContained() so it uses line buffer instead
 *		of concatenation.
 *  02/15/05, Pixie
 *		CHANGED FOR RUNUO 1.0.0 MERGE.
 *	1/17/05, Darva
 *			Concatinated Price and Description of PlayerVendor items so that
 *			the price won't roll over the top and dissapear.
 *	10/13/04, Darva
 *			Removed last changes.
 *	10/20/04, Darva
 *			Made vendor refuse to take items on which traps are enabled.
 *			Made dropping trapped containers directly on vendor not work either.
 *	10/15/04 - Pix
 *		Fixed the chopping off of the first word of the description for not-for-sale items.
 *    10/14/04, Darva
 *			Changed the items description after bounceback.
 *    10/13/04, Darva
 *			Added OnSubItemAdded override to fix Vendor's as storage.
 *			Items added that way now cost 1 gold, and say "Exploit attempted"
 *	9/24/04, Pix
 *		Fixed the <B></B> tags in the seller's descriptions.
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Prompts;
using Server.Targeting;
using Server.Misc;
using Server.Multis;
using Server.Scripts.Commands;			// log helper

namespace Server.Mobiles
{
	public class VendorItem
	{
		private int m_Price;
		private string m_Description;
		private Item m_Item;
		private bool m_Valid;

		public VendorItem(Item item, int price, string description)
		{
			m_Item = item;
			m_Price = price;
			m_Description = description;
			m_Valid = true;
		}

		public void Invalidate()
		{
			m_Valid = false;
		}

		public Item Item { get { return m_Item; } }
		public int Price { get { return m_Price; } set { m_Price = value; } }
		public string Description { get { return m_Description; } set { m_Description = value; } }

		public bool Valid { get { return m_Valid; } }
		public bool IsForSale { get { return (m_Item != null && !m_Item.Deleted && m_Price > 0); } }
	}

	public class VendorBackpack : Backpack
	{
		[Constructable]
		public VendorBackpack()
		{
			Layer = Layer.Backpack;
			Weight = 1.0;
		}

		public override int MaxWeight { get { return 0; } }

		public override bool CheckContentDisplay(Mobile from)
		{
			object root = this.RootParent;

			if (root is PlayerVendor && ((PlayerVendor)root).IsOwner(from))
				return true;
			else
				return base.CheckContentDisplay(from);
		}


		public override bool IsAccessibleTo(Mobile m)
		{
			return true;
		}

		public override bool CheckItemUse(Mobile from, Item item)
		{
			if (!base.CheckItemUse(from, item))
				return false;

			object root = this.RootParent;

			if (root is PlayerVendor && ((PlayerVendor)root).IsOwner(from))
				return true;

			if (item is Container || item is Engines.BulkOrders.BulkOrderBook)
				return true;

			from.SendLocalizedMessage(500447); // That is not accessible.
			return false;
		}

		public override bool CheckTarget(Mobile from, Target targ, object targeted)
		{
			if (!base.CheckTarget(from, targ, targeted))
				return false;

			object root = this.RootParent;

			if (root is PlayerVendor && ((PlayerVendor)root).IsOwner(from))
				return true;

			return (targ is PlayerVendor.PVBuyTarget);
		}

		public override void OnSingleClickContained(Mobile from, Item item)
		{
			if (RootParent is PlayerVendor)
			{
				PlayerVendor vend = (PlayerVendor)RootParent;
				VendorItem vi = (VendorItem)vend.SellItems[item];

				//Pix: 9/24/04 - changed the following to NOT use the localized string
				// so that we don't get the <B>...</B> tags with some client versions.

				// erl: 03/22/04 - changed to use line buffer and cleared up excess
				// commented code

				if (vi != null)
				{
					ArrayList LineBuffer = new ArrayList();

					if (vi.IsForSale)
						LineBuffer.Add(string.Format("Price: {0}", vi.Price.ToString()));
					else
						LineBuffer.Add(string.Format("Price: Not for Sale"));

					if (vi.Description != null && vi.Description != "")
						LineBuffer.Add(string.Format("Seller's Description: \"{0}\"", vi.Description));

					foreach (string line in LineBuffer)
						item.LabelTo(from, line);

				}
			}

			base.OnSingleClickContained(from, item);
		}

		public VendorBackpack(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}

	public enum PricingModel { ModifiedOSI, ClassicOSI, Commission };

	public class PlayerVendor : Mobile
	{
		private Hashtable m_SellItems;
		private Mobile m_Owner;
		private int m_BankAccount;
		private int m_HoldGold;
		private Timer m_PayTimer;
		private BaseHouse m_House;

		private Hashtable m_memory = new Hashtable();		// remember items recently seen
		public Hashtable Memory
		{
			get { return m_memory; }
		}   

        public PricingModel m_PricingModel;
        [CommandProperty(AccessLevel.GameMaster)]
        public PricingModel PricingModel
        {
            get { return m_PricingModel; }
            set { m_PricingModel = value; }
        }   

        private bool m_Destroying;
		public bool Destroying
		{
			get { return m_Destroying; }
			set { m_Destroying = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get { return m_Owner; } set { m_Owner = value; } }

		public Hashtable SellList { get { return m_SellItems; } set { m_SellItems = value; } }
		public override bool CanBeDamaged()
		{
			return m_Destroying;
		}

		public PlayerVendor(Mobile owner, BaseHouse house)
		{
			Owner = owner;
			House = house;
			m_BankAccount = 1000;
			m_HoldGold = 0;
			m_SellItems = new Hashtable();
            m_PricingModel = PricingModel.ModifiedOSI;   // default pricing model

			// default this based on the mobile placing the vendor
			if (this.Owner != null && this.Owner.AccessLevel > AccessLevel.Player)
				this.StaffOwned = true;

			CantWalk = true;

			if (!Core.AOS)
				NameHue = 0x35;

			InitStats(75, 75, 75);
			InitBody();
			InitOutfit();
			UpdateClothes(this);
			m_PayTimer = new PayTimer(this);
			m_PayTimer.Start();
		}

		public override DeathMoveResult GetInventoryMoveResultFor(Item item)
		{
			return DeathMoveResult.MoveToCorpse;
		}

		public PlayerVendor(Serial serial)
			: base(serial)
		{
		}


		public void UpdateClothes(object obj)
		{
			PlayerVendor m = obj as PlayerVendor;

			if (m != null)
			{
				Item[] items = new Item[1];
				items[0] = m.FindItemOnLayer(Layer.Shoes);

				if (items[0] != null && items[0] is BaseClothing)
				{
					items[0].Hue = 0;
				}
			}
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)2);//version

			// version 2
			writer.Write((int)m_PricingModel);

			//version 1
			writer.Write((Item)House);

			writer.Write(m_Owner);
			writer.Write(m_BankAccount);
			writer.Write(m_HoldGold);

			ArrayList list = new ArrayList(m_SellItems.Values);

			writer.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				VendorItem vi = (VendorItem)list[i];
				writer.Write(vi.Item);
				writer.Write(vi.Price);
				writer.Write(vi.Description);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						m_PricingModel = (PricingModel)reader.ReadInt();
						goto case 1;
					}
				case 1:
					{

						House = (BaseHouse)reader.ReadItem();
						goto case 0;
					}
				case 0:
					{
						m_Owner = reader.ReadMobile();
						m_BankAccount = reader.ReadInt();
						m_HoldGold = reader.ReadInt();

						int count = reader.ReadInt();
						m_SellItems = new Hashtable();
						for (int i = 0; i < count; i++)
						{
							Item item = reader.ReadItem();
							int p = reader.ReadInt();
							string d = reader.ReadString();

							if (item != null && !item.Deleted)
								m_SellItems[item] = new VendorItem(item, p, d);
						}
						break;
					}
			}

			if (version < 1)
			{

				House = BaseHouse.FindHouseAt(this);
			}

			m_PayTimer = new PayTimer(this);
			m_PayTimer.Start();

			Blessed = false;

			if (Core.AOS && NameHue == 0x35)
				NameHue = -1;
		}

		public override void InitBody()
		{
			Hue = Utility.RandomSkinHue();
			SpeechHue = 0x3B2;

			if (!Core.AOS)
				NameHue = 0x35;

			if (this.Female = Utility.RandomBool())
			{
				this.Body = 0x191;
				this.Name = NameList.RandomName("female");
			}
			else
			{
				this.Body = 0x190;
				this.Name = NameList.RandomName("male");
			}
		}

		public override void InitOutfit()
		{
			WipeLayers();
			Item shirt = new FancyShirt(Utility.RandomNeutralHue());
			shirt.Layer = Layer.InnerTorso;
			AddItem(shirt);

			Item pants = new LongPants(Utility.RandomNeutralHue());
			pants.Layer = Layer.Pants;
			AddItem(pants);

			Item sash = new BodySash(Utility.RandomNeutralHue());
			sash.Layer = Layer.MiddleTorso;
			AddItem(sash);

			Item boots = new Boots(Utility.RandomNeutralHue());
			boots.Layer = Layer.Shoes;
			AddItem(boots);

			Item cloak = new Cloak(Utility.RandomNeutralHue());
			cloak.Layer = Layer.Cloak;
			AddItem(cloak);

			Item hair = new Item(Utility.RandomList(0x203B, 0x2049, 0x2048, 0x204A));
			hair.Hue = Utility.RandomNondyedHue();
			hair.Layer = Layer.Hair;
			hair.Movable = false;
			AddItem(hair);

			Container pack = new VendorBackpack();
			pack.Movable = false;
			AddItem(pack);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int BankAccount
		{
			get { return m_BankAccount; }
			set { m_BankAccount = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int HoldGold
		{
			get { return m_HoldGold; }
			set { m_HoldGold = value; }
		}

        [CommandProperty(AccessLevel.GameMaster)]
		public int ChargePerDay
		{
			get
			{
				// before we calculate the charge, make sure all's valid
				this.ValidateSellItems();

                // no daily charge for commission vendors
                if (PricingModel == PricingModel.Commission)
					return 20;

				int total = 0;
				foreach (VendorItem v in m_SellItems.Values)
				{
					// don't add in not-for-sale stuff
					if (v.Price < 0)
						continue;

					// look for a special overflow case
					if (total > 0 && ((total + v.Price) < 0))
					{	// don't let players wrap the total thus resulting in a cheap fee!
						total = int.MaxValue;
						try 
						{
							LogHelper log = new LogHelper("VendorExploit.log", false, false);
							if (this.Owner != null)
								log.Log(LogType.Mobile, this.Owner, "Vendor owner");
							log.Log(LogType.Mobile, this, "Vendor");
							log.Log(LogType.Text, String.Format("Charging max fees against a total inventory worth of {0} gold", InventoryWorth));
							log.Finish();
						}
						catch (Exception ex) { LogHelper.LogException(ex); }
						break;
					}
					total += v.Price;
				}

				total -= 500;

				if (total < 0)
					total = 0;
                
                // old calc
				//return 20 + (total / 500);

                // new calc
                total /= 500;

                // scale on number of players playing with a floor of N
				if (PricingModel == PricingModel.ModifiedOSI && Connections < 100)
					total = (int)(total * (Connections * .01));

                // add in 20 gold as the usual (OSI) base
                int charge = 20 + total;

                return charge;
			}
		}

		private int Connections
		{
			get { return Network.NetState.Instances.Count < CoreAI.ConnectionFloor ? CoreAI.ConnectionFloor : Network.NetState.Instances.Count; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public long InventoryWorth
		{
			get
			{
				long total = 0;
				foreach (VendorItem v in m_SellItems.Values)
				{
                    if (v.Price > 0)
					    total += v.Price;
				}

				return total;
			}
		}

		#region ValidateSellItems

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ValidSellItems
		{
			get { return ValidateSellItems() == 0; }
		}

		//Pix: Re-added this function because I want the safeguard before we charge
		// fees.
		public int ValidateSellItems()
		{
			int invalid = 0;

			try
			{
				Hashtable newsellitems = new Hashtable();

				Scripts.Commands.LogHelper lh = null;

				foreach (VendorItem vi in m_SellItems.Values)
				{
					if (vi == null)
					{
						if (lh == null)
						{
							lh = new Server.Scripts.Commands.LogHelper("ValidateSellItems.log");
						}

						System.Text.StringBuilder sb = new System.Text.StringBuilder();
						sb.Append("Found null VendorItem entry in vendor: ");
						sb.Append(this.Serial);
						lh.Log(sb.ToString());
						invalid++;
						continue;
					}

					if (vi.Item != null && vi.Item.Deleted == false)
					{
						Item t = vi.Item;

						if (t == null)
						{
							if (lh == null)
							{
								lh = new Server.Scripts.Commands.LogHelper("ValidateSellItems.log");
							}

							System.Text.StringBuilder sb = new System.Text.StringBuilder();
							sb.Append("Found null item entry in vendor: ");
							sb.Append(this.Serial);
							lh.Log(sb.ToString());
							invalid++;
						}
						else
						{
							while (t != null && t.Parent != this.Backpack)
							{
								t = t.Parent as Item;
							}

							if (t != null && t.Parent == this.Backpack)
							{
								newsellitems[vi.Item] = vi;
							}
							else
							{
								if (lh == null)
								{
									lh = new Server.Scripts.Commands.LogHelper("ValidateSellItems.log");
								}

								System.Text.StringBuilder sb = new System.Text.StringBuilder();
								sb.Append("Found item-not-on-vendor entry in vendor: ");
								sb.Append(this.Serial);
								sb.Append(".  Item serial is: ");
								sb.Append(vi.Item.Serial);
								lh.Log(sb.ToString());
								invalid++;
							}
						}
					}
				}

				if (lh != null)
				{
					lh.Finish();
				}

				// patch it
				m_SellItems = newsellitems;
			}
			catch (Exception e)
			{
				Scripts.Commands.LogHelper.LogException(e);
			}

			return invalid;
		}
		#endregion

		public Hashtable SellItems { get { return m_SellItems; } }

		public override bool IsOwner(Mobile m)
		{
			return (m == m_Owner || m.AccessLevel >= AccessLevel.GameMaster);
		}

		public override bool OnBeforeDeath()
		{
			if (!base.OnBeforeDeath())
				return false;

			Item shoes = this.FindItemOnLayer(Layer.Shoes);

			if (shoes is Sandals)
				shoes.Hue = 0;

			return true;
		}

		public override bool CanBeHarmful(Mobile target, bool message, bool ignoreOurBlessedness, bool ignoreOurDeadness)
		{
			return false;
		}

		public override bool AllowEquipFrom(Mobile from)
		{
			if (IsOwner(from) && from.InRange(this, 3) && from.InLOS(this))
				return true;

			return base.AllowEquipFrom(from);
		}

		public override void OnAfterDelete()
		{
			m_PayTimer.Stop();
		}

		public override bool IsSnoop(Mobile from)
		{
			return false;
		}

		public VendorItem GetVendorItem(Item item)
		{
			return (VendorItem)m_SellItems[item];
		}

		public class VendorItemMemory
		{
			private DateTime m_GracePeriod = DateTime.MinValue;	// grace period expired by default
			public DateTime GracePeriod
			{
				get { return m_GracePeriod; }
				set { m_GracePeriod = value; }
			}
			public VendorItemMemory()
			{
				// one hour grace period
				m_GracePeriod = DateTime.Now.AddMinutes((double)CoreAI.GracePeriod);
			}
		}

		// add up all ietms in thie container (if this item is a container)
		public void TotalCollectionPrice(Item item, ref int price)
		{
			VendorItem vi = GetVendorItem(item);
			if (vi != null && vi.Price > 0) price += vi.Price;
			for (int i = 0; i < item.Items.Count; i++)
				TotalCollectionPrice((Item)item.Items[i], ref price);
		}

        public int TotalCollectionPrice(Item item)
        {
            int price = 0;
            TotalCollectionPrice(item, ref price);
            return price;
        }

		private enum RestockCharge { Record, Moved }
		private bool ProcessRestockCharge(RestockCharge mode, VendorItem vi)
		{	// no restock charges for OSI style charges
			if (PricingModel != PricingModel.Commission || vi == null)
				return false;

			if (mode == RestockCharge.Record)
			{
				if (m_memory.ContainsKey(vi.Item) == false)
					m_memory[vi.Item] = new VendorItemMemory();
			}
			else if (mode == RestockCharge.Moved)
			{
				if (m_memory.ContainsKey(vi.Item))
				{	// check to see if we are within the grace period
					VendorItemMemory vim = m_memory[vi.Item] as VendorItemMemory;
					if (vim == null) return false;
					if (DateTime.Now > vim.GracePeriod)
					{	// reset graceperiod
						// one hour grace period
						vim.GracePeriod = DateTime.Now.AddMinutes((double)CoreAI.GracePeriod);
						return true;	// user should be charged
					}
				}
				else
				{	// first time touched after a restart or daily cleanup
					VendorItemMemory vim = new VendorItemMemory();	// grace periods do not span server restarts
					vim.GracePeriod = DateTime.MinValue;			// ensure this item is outside grace period
					m_memory[vi.Item] = vim;						// record it
					return ProcessRestockCharge(mode, vi);			// reenter and process this 'out of grace move'
				}
			}

			// no special action
			return false;
		}

		public VendorItem SetVendorItem(Item item, int price, string description)
		{
			RemoveVendorItem(item);

			VendorItem vi = new VendorItem(item, price, description);
			m_SellItems[item] = vi;
			ProcessRestockCharge(RestockCharge.Record, vi);	// record this item for future restocking charges

			item.InvalidateProperties();

			return vi;
		}

		private void RemoveVendorItem(Item item)
		{
			VendorItem vi = GetVendorItem(item);

			if (vi != null)
			{
				vi.Invalidate();
				m_SellItems.Remove(item);

				foreach (Item subItem in item.Items)
					RemoveVendorItem(subItem);

				item.InvalidateProperties();
			}
		}

		private bool CanBeVendorItem(Item item)
		{
			Item parent = item.Parent as Item;

			if (parent == this.Backpack)
				return true;

			if (parent is Container)
			{
				VendorItem parentVI = GetVendorItem(parent);

				if (parentVI != null)
					return !parentVI.IsForSale;
			}

			return false;
		}

		public override void OnSubItemAdded(Item item)
		{
			base.OnSubItemAdded(item);

			// admins get special automatic pricing of certain items... our new houses for instance
            if (StaffOwned)
			{
				if (item is Multis.StaticHousing.StaticDeed)
				{
					Multis.StaticHousing.StaticDeed sd = item as Multis.StaticHousing.StaticDeed;
					if (GetVendorItem(item) == null && CanBeVendorItem(item))
					{
						int price = Multis.StaticHousing.StaticHouseHelper.GetPrice(sd.HouseID);
						SetVendorItem(item, price, "");
						return;
					}
				}
			}

			if (GetVendorItem(item) == null && CanBeVendorItem(item))
			{
				SetVendorItem(item, 999, "");
			}
		}

		public override void OnSubItemRemoved(Item item)
		{
			base.OnSubItemRemoved(item);

			if (item.GetBounce() == null)
				RemoveVendorItem(item);
		}

		public override void OnSubItemBounceCleared(Item item)
		{
			base.OnSubItemBounceCleared(item);

			if (!CanBeVendorItem(item))
				RemoveVendorItem(item);
		}

		public override void OnItemRemoved(Item item)
		{
			base.OnItemRemoved(item);

			if (item == this.Backpack)
			{
				foreach (Item subItem in item.Items)
				{
					RemoveVendorItem(subItem);
				}
			}
		}

		public override bool OnDragDrop(Mobile from, Item item)
		{
			if (IsOwner(from))
			{
				if (item is Gold)
				{
					SayTo(from, 503210); // I'll take that to fund my services.
					m_BankAccount += item.Amount;
					item.Delete();
					return true;
				}
				else
				{

					bool newItem = (GetVendorItem(item) == null);

					if (this.Backpack != null && this.Backpack.TryDropItem(from, item, false))
					{
						if (newItem)
							OnItemGiven(from, item);

						return true;
					}

					else
					{
						SayTo(from, 503211); // I can't carry any more.
						return false;
					}
				}
			}
			else
			{
				SayTo(from, 503209);// I can only take item from the shop owner.
				return false;
			}
		}


		public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			if (IsOwner(from))
			{
				if (GetVendorItem(item) == null)
				{
					// We must wait until the item is added
					Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(NonLocalDropCallback), new object[] { from, item });
				}

				return true;
			}
			else
			{
				SayTo(from, 503209); // I can only take item from the shop owner.
				return false;
			}
		}

		private void NonLocalDropCallback(object state)
		{
			object[] aState = (object[])state;

			Mobile from = (Mobile)aState[0];
			Item item = (Item)aState[1];

			OnItemGiven(from, item);
		}

		private void OnItemGiven(Mobile from, Item item)
		{
			// no prompring for special staff 'auto-priced' items
            if (item is Multis.StaticHousing.StaticDeed && StaffOwned)
				return;

			VendorItem vi = GetVendorItem(item);

			if (vi != null)
			{
				string name;
				if (item.Name != null && item.Name != "")
					name = item.Name;
				else
					name = "#" + item.LabelNumber.ToString();

				from.SendLocalizedMessage(1043303, name); // Type in a price and description for ~1_ITEM~ (ESC=not for sale)
				from.Prompt = new VendorPricePrompt(this, vi);
			}
		}

		public void RemoveInfo(Item item)
		{
			m_SellItems.Remove(item);
			for (int i = 0; i < item.Items.Count; i++)
				RemoveInfo((Item)item.Items[i]);

			item.InvalidateProperties();
		}

		public override bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (IsOwner(from))
			{
                // process restocking charge if any
				if (ProcessRestockCharge(RestockCharge.Moved, GetVendorItem(item)) == true)	
				{	// get the item/container(and contents) price
					int price = TotalCollectionPrice(item);
					// if it's a not-for-sale container that holds something for sale
					if (GetVendorItem(item) != null && GetVendorItem(item).Price == -1 &&  price > 0)
					{
						VendorItemMemory vim = m_memory[item] as VendorItemMemory;
						if (vim != null) vim.GracePeriod = DateTime.MinValue; // clear the grace period
						this.SayTo(from, "That container must be emptied before it can be moved.");
						return false;
					}
					else
					{
						// if moving the item would kill the vendor, simply warn the user
                        if ((this.BankAccount + this.HoldGold) < (int)(price * Commission))
                        {
                            VendorItemMemory vim = m_memory[item] as VendorItemMemory;
                            if (vim != null) vim.GracePeriod = DateTime.MinValue; // clear the grace period
                            this.SayTo(from, "I am not holding enough gold to cover the restock charges.");
                            return false;
                        }
                        else
                            // charge the user the restocking fee
						    Charge((int)(price * Commission), ChargeReason.Restock);

						// and tell them about it
						if (price > 0)
							this.SayTo(from, String.Format("I must charge you {0} gold to restock that item.", (int)(price * Commission)));
					}
				}
				RemoveInfo(item);
				return true;
			}
			else
			{
				SayTo(from, 503223);// If you'd like to purchase an item, just ask.
				return false;
			}
		}


		public override void OnDoubleClick(Mobile from)
		{

			if (IsOwner(from))
			{
				from.SendGump(new PlayerVendorOwnerGump(this, from));
			}
			else
			{
				Container pack = Backpack;

				if (pack != null)
				{
					SayTo(from, 503208);// Take a look at my goods.
					pack.DisplayTo(from);
				}
			}
		}

		public override void GetChildProperties(ObjectPropertyList list, Item item)
		{
			base.GetChildProperties(list, item);

			VendorItem vi = (VendorItem)m_SellItems[item];

			if (vi != null)
			{
				if (!vi.IsForSale)
					list.Add(1043307); // Price: Not for sale.
				else if (vi.Price <= 0)
					list.Add(1043306); // Price: FREE!
				else
					list.Add(1043304, vi.Price.ToString()); // Price: ~1_COST~

				if (vi.Description != null && vi.Description.Length > 0)
					list.Add(1043305, vi.Description); // Description: ~1_DESC~
			}
		}

		//return house
		public BaseHouse House
		{
			get { return m_House; }
			set
			{
				m_House = value;
			}
		}

		//new gold collection routine allows player to withdraw specific amount
		public void CollectGold(Mobile to)
		{
			if (HoldGold > 0)
			{
				SayTo(to, "How much of the {0} that I'm holding would you like?", HoldGold.ToString());
				to.SendMessage("Enter the amount of gold you wish to withdraw (ESC = CANCEL):");

				to.Prompt = new CollectGoldPrompt(this);
			}
			else
			{
				SayTo(to, 503215); // I am holding no gold for you.
			}
		}

		public int GiveGold(Mobile to, int amount)
		{
			if (amount <= 0)
				return 0;

			if (amount > HoldGold)
			{
				SayTo(to, "I'm sorry, but I'm only holding {0} gold for you.", HoldGold.ToString());
				return 0;
			}

			int amountGiven = Banker.DepositUpTo(to, amount);
			HoldGold -= amountGiven;

			if (amountGiven > 0)
			{
				to.SendLocalizedMessage(1060397, amountGiven.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
			}

			if (amountGiven == 0)
			{
				SayTo(to, 1070755); // Your bank box cannot hold the gold you are requesting.  I will keep the gold until you can take it.
			}
			else if (amount > amountGiven)
			{
				SayTo(to, 1070756); // I can only give you part of the gold now, as your bank box is too full to hold the full amount.
			}
			else if (HoldGold > 0)
			{
				SayTo(to, 1042639); // Your gold has been transferred.
			}
			else
			{
				SayTo(to, 503234); // All the gold I have been carrying for you has been deposited into your bank account.
			}

			return amountGiven;
		}

		//arraylist to get all items in vendor backpack used for destroying vendors
		protected ArrayList GetItems()
		{
			ArrayList list = new ArrayList();

			foreach (Item item in this.Items)
			{
				if (item.Movable && item != this.Backpack)
					list.Add(item);
			}

			if (this.Backpack != null)
			{
				list.AddRange(this.Backpack.Items);
			}

			return list;
		}

		public virtual void Destroy(bool toBackpack)
		{
			Item shoes = this.FindItemOnLayer(Layer.Shoes);

			if (shoes is Sandals)
				shoes.Hue = 0;

			ArrayList list = GetItems();

            // don't drop stuff owned by an administrator
            if (StaffOwned == false)
			    if (list.Count > 0 || HoldGold > 0) // if you have items of gold
			    {

				    if ((toBackpack || House == null) && this.Map != Map.Internal) // Move to backpack
				    {
					    Container backpack = new Backpack();

					    foreach (Item item in list)
					    {
						    if (item.Movable != false) // only drop items which are moveable
							    backpack.DropItem(item);
					    }

					    backpack.MoveToWorld(this.Location, this.Map);
				    }
			    }

			Delete();
		}

		public void Dismiss(Mobile from)
		{
			Container pack = this.Backpack;

			if (pack != null && pack.Items.Count > 0)
			{
				SayTo(from, 503229); // Thou canst replace me until thy removest all the item from my stock.
				return;
			}

			GiveGold(from, HoldGold);

			if (m_HoldGold > 0)
			{
				from.AddToBackpack(new BankCheck(m_HoldGold));
				m_HoldGold = 0;
			}

			Destroy(true);
		}


		public override void DisplayPaperdollTo(Mobile m)
		{

			if (IsOwner(m))
			{
				base.DisplayPaperdollTo(m);
			}
			else if (CanInteractWith(m, false))
			{
				OpenBackpack(m);
			}
		}

		public void OpenBackpack(Mobile from)
		{
			if (this.Backpack != null)
			{
				SayTo(from, IsOwner(from) ? 1010642 : 503208); // Take a look at my/your goods.

				this.Backpack.DisplayTo(from);
			}
		}

		public override bool HandlesOnSpeech(Mobile from)
		{
			return (from.GetDistanceToSqrt(this) <= 3);
		}

		public bool WasNamed(string speech)
		{
			string name = this.Name;

			return (name != null && Insensitive.StartsWith(speech, name));
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			Mobile from = e.Mobile;

			if (e.Handled)
				return;

			Regions.TownshipRegion tr = Regions.TownshipRegion.GetTownshipAt(this);
			if (tr != null)
			{
				if (tr.TStone != null)
				{
					if (tr.TStone.IsEnemy(from as PlayerMobile))
					{
						if (!IsOwner(from))
						{
							//if they're an enemy and not the owner, ignore them
							from.SendMessage("You are an enemy of this township, so this vendor refuses his services.");
							return;
						}
					}
				}
			}

			if (e.HasKeyword(0x3C) || (e.HasKeyword(0x171) && WasNamed(e.Speech))) // vendor buy, *buy*
			{
				if (IsOwner(from))
				{
					SayTo(from, 503212); // You own this shop, just take what you want.
				}
				else
				{
					from.SendLocalizedMessage(503213);// Select the item you wish to buy.
					from.Target = new PVBuyTarget();
					e.Handled = true;
				}
			}
			else if (e.HasKeyword(0x3D) || (e.HasKeyword(0x172) && WasNamed(e.Speech))) // vendor browse, *browse
			{
				Container pack = Backpack;

				if (pack != null)
				{
					SayTo(from, IsOwner(from) ? 1010642 : 503208);// Take a look at my/your goods.
					pack.DisplayTo(from);
					e.Handled = true;
				}
			}
			else if (e.HasKeyword(0x3E) || (e.HasKeyword(0x173) && WasNamed(e.Speech))) // vendor collect, *collect
			{
				if (IsOwner(from))
				{
					CollectGold(from);
					e.Handled = true;
				}
			}
			else if (e.HasKeyword(0x3F) || (e.HasKeyword(0x174) && WasNamed(e.Speech))) // vendor status, *status
			{
				if (IsOwner(from))
				{
					from.SendGump(new PlayerVendorOwnerGump(this, from));
					e.Handled = true;
				}
				else
				{
					SayTo(from, 503226); // What do you care? You don't run this shop.	
				}
			}
			else if (e.HasKeyword(0x40) || (e.HasKeyword(0x175) && WasNamed(e.Speech))) // vendor dismiss, *dismiss
			{
				if (IsOwner(from))
					Dismiss(from);
			}
			else if (e.HasKeyword(0x41) || (e.HasKeyword(0x176) && WasNamed(e.Speech))) // vendor cycle, *cycle
			{
				if (IsOwner(from))
					this.Direction = this.GetDirectionTo(from);
			}
		}

		public bool CanInteractWith(Mobile from, bool ownerOnly)
		{
			if (!from.CanSee(this) || !Utility.InUpdateRange(from, this) || !from.CheckAlive())
				return false;

			if (ownerOnly)
				return IsOwner(from);

			if (House != null && House.IsBanned(from) && !IsOwner(from))
			{
				from.SendLocalizedMessage(1062674); // You can't shop from this home as you have been banned from this establishment.
				return false;
			}

			Regions.TownshipRegion tr = Regions.TownshipRegion.GetTownshipAt(this);
			if (tr != null)
			{
				if (tr.TStone != null)
				{
					if (tr.TStone.IsEnemy(from as PlayerMobile))
					{
						if (!IsOwner(from))
						{
							//if they're an enemy and not the owner, ignore them
							from.SendMessage("You can't shop from this home as you have been decared an enemy of the township.");
							return false;
						}
					}
				}
			}

			return true;
		}

        public double Commission
        {
            get { return CoreAI.Commission; }
        }

        public enum ChargeReason
        {
            Unknown,
            Wage,
            Restock,
            Commission,
        }

		/*public void Charge(int pay)
		{
			Charge(pay, false);
		}*/

        public void Charge(int pay, ChargeReason reason)
        {
            //if we're an admin-owned vendor, then don't charge!
            if (this.StaffOwned)
                return;

			// award skill points for sales
			ProcessAwards(pay, reason);

            if (this.BankAccount < pay)
            {
                pay -= this.BankAccount;
                this.BankAccount = 0;

                if (this.HoldGold < pay)
                {
                    this.Say(503235); // I regret nothing!postal
                    this.Destroying = true;
                    this.Blessed = false;
                    this.Kill();
                }
                else
                {
                    this.HoldGold -= pay;
                }
            }
            else
            {
                this.BankAccount -= pay;
            }
        }

		/// <summary>
		/// Our new Shopkeeper skill rewards the Shopkeeper with ShopkeeperPoints for sales and normal vendor 
		/// charges. This may seem broken and strange, but we cannot reward for vendor income via sales as 
		/// this is too easily exploited (and costs the Shopkeeper nothing.) The only reasonable way to award 
		/// points is through cost to the shop owner as this will most accurately reflect both commitment and 
		/// success. One cost we do not credit the Shopkeeper with is restocking charges. This is certainly 
		/// not 'professional behavior' and should not be rewarded; however, we DO penalize the Shopkeeper 
		/// by removing points of the restock caused the vendor to die because there were insufficient funds.
        /// --
        /// because we cap vendor inventory near 100,000,000, the highest one-time charge would be
        /// that of commission type sales, and today that would be 7% or 7,000,000 resulting in a 
        /// worst case scenario of a 70,000,000 penalty.
		/// </summary>
		/// <param name="pay"></param>
		/// <param name="penalty"></param>
        public void ProcessAwards(int pay, ChargeReason reason)
		{   // only award points to shopkeepers (ones that have purchased the book)
			PlayerMobile pm = (Owner as PlayerMobile);

            if (reason == PlayerVendor.ChargeReason.Commission && this.PricingModel != PricingModel.Commission)
                return;

            if (pm == null)
                return;

			// punish the lame or cheater (includes penalty charges)
            // do not share the punishment with the 'shop owner'
            if ((this.BankAccount + this.HoldGold) < pay)
            {   // no matter what the reason, if the vendor does not have enough gold to handle the chargs, pass the penalty on to the owner
                ApplyAwards(pm, -(pay * 10));
            }
            else
            {	// reward the hard working, but do not credit penalty charges
                if (reason == ChargeReason.Wage || reason == ChargeReason.Commission)
                {
                    if (this is RentedVendor)
                    {   // if the owner of the shop is a 'shopkeeper', then we must share our shopkeeper points with her.
                        //  give the shop owner of a rented vendor 25% of the profit
                        ApplyAwards((this as RentedVendor).Landlord as PlayerMobile, pay * .25);

                        //  give the owner of a rented vendor 75% of the profit
                        ApplyAwards(pm, pay * .75);
                    }
                    else
                    {
                        // 100% of the points go to owners of regular vendors
                        ApplyAwards(pm, pay);
                    }
                }
                else if (reason == ChargeReason.Restock)
                    // If the shopkeeper is paying a penalty, charge them reverse points, fame and karma
                    ApplyAwards(pm, -pay);
            }
		}

        /// <summary>
        /// Calculate the shopkeepers fame and karma based on these 'gold' equivelents
        /// troll: Fame = 3500; Karma = -3500; PackGold( 50, 100 );
        /// for each 75 gold you get 3500 fame and -3500 karma
        /// orc: Fame = 1500; Karma = -1500; PackGold( 25, 50 );
        /// for each 37.5 gold you get 1500 fame and -1500 karma
        /// </summary>
        /// <param name="pm"></param>
        /// <param name="award"></param>
        private void ApplyAwards(PlayerMobile pm, double award)
        {
            if (pm is PlayerMobile == false || pm.Shopkeeper == false)
                return;
            
            // cap up-side bonuses to sales of items > 5K to prevent 1 item sales of 1,000,000 to esentially buy your points etc.
            // capping will make it VERY expensive to buy your skills, fame and karma, but not hurt the honest shopkeeper
            double awardPoints = (award > 5000) ? 5000 : award;

            // create a sliding scale for fame/karma that SOMEWHAT models creature awards
            double slide;
            if (award < 100)
                slide = award * 40.0;
            else if (award < 200)
                slide = award * (40.0 / 4);
            else if (award < 300)
                slide = award * (40.0 / 6);
            else if (award < 400)
                slide = award * (40.0 / 8);
            else if (award < 500)
                slide = award * (40.0 / 10);
            else if (award < 600)
                slide = award * (40.0 / 12);
            else if (award < 700)
                slide = award * (40.0 / 14);
            else if (award < 800)
                slide = award * (40.0 / 16);
            else if (award < 900)
                slide = award * (40.0 / 18);
            else
                slide = award;
            
            // for now just use the same value for each
            double KarmaPoints = slide;
            double FamePoints = slide;

            // add subtract reward points
            pm.ShopkeeperPoints += awardPoints;

            Titles.AwardFame(pm, (int)(FamePoints) / 100, true);
            Titles.AwardKarma(pm, (int)(KarmaPoints) / 100, true);
        }

		private class CollectGoldPrompt : Prompt
		{
			private PlayerVendor m_Vendor;

			public CollectGoldPrompt(PlayerVendor vendor)
			{
				m_Vendor = vendor;
			}

			public override void OnResponse(Mobile from, string text)
			{
				if (!m_Vendor.CanInteractWith(from, true))
					return;

				text = text.Trim();

				int amount;
				try
				{
					amount = Convert.ToInt32(text);
				}
				catch
				{
					amount = 0;
				}

				GiveGold(from, amount);
			}

			public override void OnCancel(Mobile from)
			{
				if (!m_Vendor.CanInteractWith(from, true))
					return;

				GiveGold(from, 0);
			}

			private void GiveGold(Mobile to, int amount)
			{
				if (amount <= 0)
				{
					m_Vendor.SayTo(to, "Very well. I will hold on to the money for now then.");
				}
				else
				{
					m_Vendor.GiveGold(to, amount);
				}
			}
		}

		private class PayTimer : Timer
		{
			private PlayerVendor m_Vendor;

			public PayTimer(PlayerVendor vend)
				// add a random delay of up to a full UODay(5 * 24) to prevent all vendors from firing at the same time (server lag prevention)
				: base(TimeSpan.FromMinutes(Clock.MinutesPerUODay + Utility.Random((int)Clock.MinutesPerUODay)), TimeSpan.FromMinutes(Clock.MinutesPerUODay))
			{
				m_Vendor = vend;
				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{   
                // daily cost
                m_Vendor.Charge(m_Vendor.ChargePerDay, ChargeReason.Wage);

				// each UO day we flush expired 'memories' of expired item grace periods
				ArrayList Cleanup = new ArrayList();
				foreach (DictionaryEntry de in m_Vendor.Memory)
				{
					VendorItemMemory vim = de.Value as VendorItemMemory;
					if (vim != null)
						if (DateTime.Now > vim.GracePeriod)
							Cleanup.Add(de.Key);
				}

				foreach (object key in Cleanup)
					if (m_Vendor.Memory.Contains(key))
						m_Vendor.Memory.Remove(key);
			}
		}

		public class PVBuyTarget : Target
		{
			public PVBuyTarget()
				: base(3, false, TargetFlags.None)
			{
				AllowNonlocal = true;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item)
				{
					Item item = (Item)targeted;

					PlayerVendor vendor = item.RootParent as PlayerVendor;

					if (vendor == null)
						return;

					VendorItem vi = (VendorItem)vendor.SellItems[item];

					if (vi != null)
					{
						if (vi.IsForSale)
							from.SendGump(new PlayerVendorBuyGump(item, vendor, vi));
						else
							vendor.SayTo(from, 503202); // This item is not for sale.

						return;
					}

					vendor.SayTo(from, 503216); // You can't buy that.
				}
			}
		}
	}
}

namespace Server.Prompts
{
	public class VendorPricePrompt : Prompt
	{
		private PlayerVendor m_Vendor;
		//private Item m_Item;
		//private Item m_Cont;
		private VendorItem m_VI;

		public VendorPricePrompt(PlayerVendor vendor, VendorItem vi)
		{
			m_Vendor = vendor;
			m_VI = vi;
		}

		private void SetInfo(Mobile from, int price, string description)
		{
			Item item = m_VI.Item;

			bool setPrice = false;

			if (price <= 0) // Not for sale
			{
				price = -1;

				if (item is Container)
				{
					if (item is LockableContainer && ((LockableContainer)item).Locked)
						m_Vendor.SayTo(from, 1043298); // Locked items may not be made not-for-sale.
					else if (item.Items.Count > 0)
						m_Vendor.SayTo(from, 1043299); // To be not for sale, all items in a container must be for sale.
					else
						setPrice = true;
				}
				else if (item is BaseBook || item is Engines.BulkOrders.BulkOrderBook)
				{
					setPrice = true;
				}
				else if (item is Multis.StaticHousing.StaticDeed && m_Vendor.StaffOwned)
				{
					Multis.StaticHousing.StaticDeed sd = item as Multis.StaticHousing.StaticDeed;
					int DefaultPrice = Multis.StaticHousing.StaticHouseHelper.GetPrice(sd.HouseID);
					m_Vendor.SetVendorItem(item, DefaultPrice, "");
				}
				else
				{
					m_Vendor.SayTo(from, 1043301); // Only the following may be made not-for-sale: books, containers, keyrings, and items in for-sale containers.
					m_Vendor.SetVendorItem(item, 999, description);
				}
			}
			else
			{	// Adam: Because m_Vendor.ChargePerDay can go negative due to int-wrap (at 2billion) we cap the total worth
				//	allowed on a vendor. This should avoid any int-wrap-exploits.
				//	Note: One KNOWN exploit was having 2.2 billion on the vendor. This goes negative and is set to 0 in
				//	ChargePerDay thus resulting in a minimum 20 gold charge. (So you could sell all you wanted for 20 gp per UO day.)
				int max = (m_Vendor.InventoryWorth < 100000000) ? 100000000 : 1000;
				if (price > max)
				{
					price = max;
					from.SendMessage("You cannot price this item above {0} gold.  The price has been adjusted.", max);
				}

				setPrice = true;
			}

			if (setPrice)
			{
				m_Vendor.SetVendorItem(item, price, description);
			}
			else
			{
				m_VI.Description = description;
			}
		}


		public override void OnResponse(Mobile from, string text)
		{
			int space = text.IndexOf(' ');
			if (space == -1)
				space = text.Length;
			int price = 0;
			string desc = "";

			try
			{
				price = Convert.ToInt32(text.Substring(0, space));
			}
			catch
			{
				price = 0;
			}

			if (price > 0)
			{
				if (space < text.Length)
					desc = text.Substring(space + 1);
			}
			else
			{
				desc = text;
			}

			SetInfo(from, price, desc);
		}

		public override void OnCancel(Mobile from)
		{
			SetInfo(from, 0, "");
		}
	}
}


#region PV Staff Commands
public class PlayerVendorStaffCommands
{
	public static void Initialize()
	{
		Server.Commands.Register("PVListItems", AccessLevel.Counselor, new CommandEventHandler(PVListItems_OnCommand));
		Server.Commands.Register("PVFixCharges", AccessLevel.Counselor, new CommandEventHandler(PVFixCharges_OnCommand));
	}

	public static void PVListItems_OnCommand(CommandEventArgs e)
	{
		e.Mobile.SendMessage("Target the player vendor to fix");
		e.Mobile.Target = new PVListTarget();
	}
	public static void PVFixCharges_OnCommand(CommandEventArgs e)
	{
		e.Mobile.SendMessage("Target the player vendor to fix");
		e.Mobile.Target = new PVFixTarget();
	}

	private class PVFixTarget : Target
	{
		public PVFixTarget()
			: base(11, false, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!(targeted is PlayerVendor))
			{
				from.SendMessage("This is only for PlayerVendors.");
				return;
			}

			PlayerVendor pv = targeted as PlayerVendor;

			if (pv == null)
			{
				from.SendMessage("That was null.");
			}
			else
			{
				from.SendMessage("This doesn't do anything yet.");
			}
		}
	}
	private class PVListTarget : Target
	{
		public PVListTarget()
			: base(11, false, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!(targeted is PlayerVendor))
			{
				from.SendMessage("This is only for PlayerVendors.");
				return;
			}

			PlayerVendor pv = targeted as PlayerVendor;

			if (pv == null)
			{
				from.SendMessage("That was null.");
			}
			else
			{
				int count = 0;
				from.SendMessage("BEGIN LIST");
				foreach (VendorItem v in pv.SellItems.Values)
				{
					from.SendMessage("---{0}---", count);
					if (v == null)
					{
						from.SendMessage("VendorItem[{0}] is null", count);
					}
					else
					{
						if (v.Item == null)
						{
							from.SendMessage("VendorItem[{0}].Item is null", count);
						}
						if (v.Valid == false)
						{
							from.SendMessage("VendorItem[{0}].Valid is false", count);
						}
						if (v.IsForSale == false)
						{
							from.SendMessage("VendorItem[{0}].IsForSale is false", count);
						}

						from.SendMessage("d:{0} p:{1}", v.Description, v.Price);
					}
					count++;
				}
				from.SendMessage("END LIST");
			}
		}
	}


}
#endregion
