/*
 *	This program is the CONFIDENTIAL and PROPRIETARY property
 *	of Tomasello Software LLC. Any unauthorized use, reproduction or
 *	transfer of this computer program is strictly prohibited.
 *
 *      Copyright (c) 2004 Tomasello Software LLC.
 *	This is an unpublished work, and is subject to limited distribution and
 *	restricted disclosure only. ALL RIGHTS RESERVED.
 *
 *			RESTRICTED RIGHTS LEGEND
 *	Use, duplication, or disclosure by the Government is subject to
 *	restrictions set forth in subparagraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Items/Containers/TreasureMapChest.cs
 * ChangeLog:
 *	11/12/08, Adam
 *		- Thwart ‘fast lifting’ in CheckLift: “You thrust your hand into the chest but come up empty handed.”
 *	11/10/08, Adam
 *		- Replace hard coded drops rate fopr magic weapoins and armor with the new function MagicArmsThrottle(level)
 *		- Have MagicArmsThrottle() effectively 1/2 the current drop rate for magic weapons.
 *	07/23/08, weaver
 *		Added Free() before return in IPooledEnumerable loop.
 *	07/23/08, weaver
 *		Automated IPooledEnumerable optimizations. 1 loop updated.
 *  2/28/07, Adam
 *      Allow either explosion OR poison for the trap
 *  1/31/07, Adam
 *      - change the spawn rate from a flat 10% to 0.1 * (Level * .5);
 *      - Don't allow the milking of the chest by lifting one item from a stack
 *  11/27/06, Kit
 *      Fixed bug with chest theme type not being serialized
 *	1/21/06, Adam
 *		It's no longer required that an aligned player be wearing their IOB to get caught stealing from their kin.
 *		Detail: Removed the requirement for pm.IOBEquipped in CheckThief()
 *	1/20/06, Adam
 *		1. Add in new theme loot
 *		2. cleanup the treasure chest code by moving the theme loot into AddThemeLoot()
 *		3. Do not lock/trap Overland Themed Chests. RP. the Treasure Hunter NPC has unlocked it for you.
 *	7/13/05, erlein
 *		Added SDrop chance to weapon + armor dropping section.
 *	4/11/05, Adam
 *		Move the hueing code to Loot.ImbueWeaponOrArmor
 *		Add back normal magic weapons/armor for themed chests, but at reduced rate.
 *	04/10/05, Kitaras
 *		Removed normal magic weapons/armor from spawning in themed chests.
 *	04/09/05, Kitaras
 *		Implemented special weapon loot drops for themed chests
 *	04/07/05, Kitaras
 *		Implemented Initial themed loot drops
 *	03/31/05, Kitaras
 *		Added themed treasure chest support
 *	03/30/05, Kitaras
 *		Added code to OnItemLifted to spawn twice the normal amount of mobs
 *		for themed chests, added theme property and value to treasuechests.
 *	03/28/05, Kitaras
 *		Added Check to CheckThief to prevent controled pets with iob alignment
 *		from setting off "you have been noticed stealing from you kin"
 *	3/28/05, Adam
 *		Move weighted table selection code for weapon/armor attr to Loot.cs
 *	11/20/04, Adam
 *		Add CheckThief() method to OnItemLifted() to see if you are stealing from your kin!
 *	9/8/04, Adam
 *		decrease the chances to get the max level attribute for this level
 *		Create an unevenly weighted table for chance resolution
 *	9/6/04, Adam
 *		decrease the chances to get the max level attribute for this level
 *		from (1 in level+1) to (1 in 2 * (level+1))
 *  8/9/04, Pixie
 *		Changed the damage done when the trap is tripped on a disarm failure.
 *  8/8/04, Pixie
 *		Added functionality for tripping the trap if you fail to disarm it.
 *  7/23/04, Adam
 *		1. add a 5% chance at a slayer weapon
 * 		2. add a 10% chance at a weapon upgrade
 *	7/12/04, Adam
 *		1. Changed drop to drop 1/2 of the original number or weapons / armor.
 *  6/29/04, Adam
 *		Changed to drop scrolls appropriate for the level.
 *		Added PackScroll procedures
 *	5/19/04, pixie
 *		Modifies so the trap resets when it is tripped.
 *		Now the only way to access the items inside is by removing the
 *		trap with the Remove Trap skill.
 *	4/30/04, mith
 *		modified the chances for getting high end weapons/armor based on treasure chest level.
 *   4/27/2004, pixie
 *     Changed so telekinesis doesn't trip the trap
 */

using System;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Multis;
using Server.Mobiles;
using Server.Network;
using Server.Engines.Plants;
using Server.ContextMenus;
using Server.Engines.PartySystem;
using Server.Misc;

namespace Server.Items
{
	[FlipableAttribute( 0xE41, 0xE40 )]
	public class TreasureMapChest : LockableContainer
	{
		//TrapSensitivity modifies the chance to trip the trap
		// when someone fails to disarm it.
		private double TrapSensitivity = 1.0;


		private int m_Level;
		private DateTime m_DeleteTime;
		private Timer m_Timer;
		private Mobile m_Owner;
		private bool IsThemed;
		private ChestThemeType m_type;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Level{ get{ return m_Level; } set{ m_Level = value; } }

		//set theme type of chest
		[CommandProperty( AccessLevel.GameMaster )]
		public ChestThemeType Type{ get{ return m_type; } set{ m_type = value; } }

		//set if chest is themed or not
		[CommandProperty( AccessLevel.GameMaster )]
		public bool Themed{ get{ return IsThemed; } set{ IsThemed = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner{ get{ return m_Owner; } set{ m_Owner = value; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime DeleteTime{ get{ return m_DeleteTime; } }

		//standered [add treasuremapchest <level>
		[Constructable]
		public TreasureMapChest( int level) : this( null, level, false, ChestThemeType.None )
		{
		}
		// new [add treasuremapchest <level> <theme>
		[Constructable]
		public TreasureMapChest( int level, ChestThemeType type ) : this( null, level, true, type )
		{
		}

		public TreasureMapChest( Mobile owner, int level, bool themed ,ChestThemeType type) : base( 0xE41 )
		{
			m_Owner = owner;
			m_Level = level;
			IsThemed = themed;
			m_type = type;
			m_DeleteTime = DateTime.Now + TimeSpan.FromHours( 3.0 );

			m_Timer = new DeleteTimer( this, m_DeleteTime );
			m_Timer.Start();

			Fill( this, level, IsThemed, type );
		}

		public override int DefaultGumpID{ get{ return 0x42; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 18, 105, 144, 73 ); }
		}

		const int STATUES = 12; //12 new statue types to drop
		private static MonsterStatuetteType[] m_Monster = new MonsterStatuetteType[STATUES]
		{
			MonsterStatuetteType.SolenWorker,
			MonsterStatuetteType.TerathanAvenger,
			MonsterStatuetteType.GiantRat,
			MonsterStatuetteType.HordeDemon,
			MonsterStatuetteType.BillyGoat,
			MonsterStatuetteType.GrizzlyBear,
			MonsterStatuetteType.Ghost,
			MonsterStatuetteType.Ghoul,
			MonsterStatuetteType.SeaHorse,
			MonsterStatuetteType.Genie,
			MonsterStatuetteType.Pixie,
			MonsterStatuetteType.Unicorn,
		};

		private static object[] m_Arguments = new object[1];

		//override fill function to keep fishing and other scripts useing old fill function not needing updated.
		public static void Fill( LockableContainer cont, int level)
		{
			Fill( cont, level, false, ChestThemeType.None);
		}

		public static void Fill( LockableContainer cont, int level, bool IsThemed, ChestThemeType type )
		{
			cont.Movable = false;

			// the speial Overland Treasure Hunter NPC 'unlocks' the chest for you!
			if (TreasureTheme.IsOverlandTheme(type) == false)
			{
                cont.TrapType = Utility.RandomBool() ? TrapType.PoisonTrap : TrapType.ExplosionTrap;
				cont.TrapPower = level * 25;
				cont.Locked = true;
			}

			switch ( level )
			{
				case 1: cont.RequiredSkill = 36; break;
				case 2: cont.RequiredSkill = 76; break;
				case 3: cont.RequiredSkill = 84; break;
				case 4: cont.RequiredSkill = 92; break;
				case 5: cont.RequiredSkill = 100; break;
			}

			cont.LockLevel = cont.RequiredSkill - 10;
			cont.MaxLockLevel = cont.RequiredSkill + 40;

			// add theme loot
			AddThemeLoot(cont, level, type);

			// now for the gold
		    cont.DropItem( new Gold( level * 1000 ) );

			//if not a undead or pirate chest add scrolls
			if(type !=ChestThemeType.Pirate || type != ChestThemeType.Undead)
			{
				// adam: Changed to drop scrolls appropriatre for the level.
				for ( int i = 0; i < level * 5; ++i )
				{
					int minCircle = level;
					int maxCircle = (level + 3);
					PackScroll( cont, minCircle, maxCircle );
				}

			}

			// magic armor and weapons
			int count = MagicArmsThrottle(level);		// calc amount of magic armor and weapons to drop
			if (IsThemed == true) count /= 2;			// adam: Less loot if a themed chest because they get other goodies.
			for ( int i = 0; i < count; ++i )
			{
				Item item;
				item = Loot.RandomArmorOrShieldOrWeapon();
				item = Loot.ImbueWeaponOrArmor (item, level, 0.05, false);

				// erl: SDrop chance
				// ..
				if( Server.Engines.SDrop.SDropTest( item, CoreAI.EScrollChance ) )
				{
					// Drop a scroll instead
					EnchantedScroll escroll = Loot.GenEScroll((object) item);

					// Delete the original item
					item.Delete();

					// Re-reference item to escroll and continue
					item = (Item) escroll;
				}
				// ..

				cont.DropItem( item );
			}

			PackRegs(cont, level * 20);
			PackGems(cont, level * 10);
		}

		public static int[] m_Gems = new int[9];
		public static Type[] m_GemTypes = new Type[]
		{
			typeof( Amber ), typeof( Amethyst ),
			typeof( Citrine ), typeof( Diamond ),
			typeof( Emerald ), typeof( Ruby ),
			typeof( Sapphire ), typeof( StarSapphire ),
			typeof( Tourmaline )
		};

		//rageant type array for loot generation
		public static int[] m_Regs = new int[8];
		public static Type[] m_RegTypes = new Type[]
		{
			typeof( BlackPearl ), typeof( Bloodmoss ),
			typeof( Garlic ), typeof( Ginseng ),
			typeof( MandrakeRoot ), typeof( Nightshade ),
			typeof( SpidersSilk ), typeof( SulfurousAsh )
		};

		private static int MagicArmsThrottle(int level)
		{
			// As of 11/10/08 was ((level * 6) / 2)
			// cutting that in half.
			//return ((level * 3) / 2);

			// Adam returning to normal levels and changing sDrop globally to fail 60% of the time.
			// This is a temp measure until we redesign the sDrop rates
			return ((level * 6) / 2);
		}

		public static void PackGems(LockableContainer cont, int count)
		{
			ClearAmounts( TreasureMapChest.m_Gems );

			for ( int i = 0; i < count; ++i )
				m_Gems[Utility.Random( TreasureMapChest.m_Gems.Length )]++;

			AddItems( cont, TreasureMapChest.m_Gems, TreasureMapChest.m_GemTypes );
		}

		public static void PackRegs(LockableContainer cont, int count)
		{
			ClearAmounts( TreasureMapChest.m_Regs );

			for ( int i = 0; i < count; ++i )
				m_Regs[Utility.Random( TreasureMapChest.m_Regs.Length )]++;

			AddItems( cont, TreasureMapChest.m_Regs, TreasureMapChest.m_RegTypes );
		}

		private static void AddThemeLoot (LockableContainer cont, int level, ChestThemeType type)
		{
			MonsterStatuette mx = null;

			//switch to add in theme treasures
			switch ( type )
			{
				case ChestThemeType.Solen:
				{

					//drop are special weapon
					QuarterStaff special = new QuarterStaff();
					special.Name = "Chitanous Staff";
					cont.DropItem(Loot.ImbueWeaponOrArmor(special, 6, 0, true));

					//go into dropping normal loot

					int onlyonedrop = Utility.RandomMinMax(0,1);

					if(onlyonedrop ==0 )cont.DropItem(new Seed(PlantType.Hedge,0,false)); //new solen seed
					if(onlyonedrop ==1 )cont.DropItem(new WaterBucket() ); //new waterbucket

					if (Utility.RandomDouble() <= 0.30 ) //30% chance to drop a statue
					{
						int whichone = Utility.RandomMinMax(0,1);
						if(whichone == 0)mx = new MonsterStatuette (m_Monster[0]);
						if(whichone == 1)mx = new MonsterStatuette (m_Monster[1]);
						mx.LootType = LootType.Regular;		// not blessed
						cont.DropItem( mx );			// drop it baby!
					}
					break;
				}

				case ChestThemeType.Brigand:
				{
					//drop are special weapon
					Katana special = new Katana();
					special.Name = "Bandit's Blade";
					cont.DropItem(Loot.ImbueWeaponOrArmor(special, 6, 0, true));

					int onlyonedrop = Utility.RandomMinMax(0,1);

					if(onlyonedrop ==0 )cont.DropItem(new Brazier(true)); //new movable brazier
					if(onlyonedrop ==1 )cont.DropItem(new DecorativeBow(Utility.RandomMinMax(0,3))); //random decorative bow type

					for ( int i = 0; i < level * 5; ++i )
					{
						cont.DropItem (new PowderOfTranslocation() );  //drop powder of translocation
					}

					if (Utility.RandomDouble() <= 0.30 ) //30% chance to drop a statue
					{
						int whichone = Utility.RandomMinMax(0,1);
						if(whichone == 0)mx = new MonsterStatuette (m_Monster[2]);
						if(whichone == 1)mx = new MonsterStatuette (m_Monster[3]);
						mx.LootType = LootType.Regular;		// not blessed
						cont.DropItem( mx );			// drop it baby!
					}
					break;
				}

				case ChestThemeType.Savage:
				{
					//drop are special weapon
					ShortSpear special = new ShortSpear();
					special.Name = "Ornate Ritual Spear";
					cont.DropItem(Loot.ImbueWeaponOrArmor(special, 6, 0, true));

					int rug = Utility.RandomMinMax(0,1);
					int onlyonedrop = Utility.RandomMinMax(0,1);

					if(onlyonedrop ==0 )cont.DropItem(new SkullPole() ); //new skull pole

					if(onlyonedrop ==1 )
					{
						if(rug == 0) cont.DropItem(new BrownBearRugEastDeed() ); //new rug east
						if(rug == 1) cont.DropItem(new BrownBearRugSouthDeed() ); //new rug south
					}

					if (Utility.RandomDouble() <= .30 ) //30% chance to drop a statue
					{
						int whichone = Utility.RandomMinMax(0,1);
						if(whichone == 0)mx = new MonsterStatuette (m_Monster[4]);
						if(whichone == 1)mx = new MonsterStatuette (m_Monster[5]);
						mx.LootType = LootType.Regular;			// not blessed
						cont.DropItem( mx );				// drop it baby!
					}
					break;
				}

				case ChestThemeType.Undead:
				{
					Halberd special = new Halberd();
					special.Name = "Soul Reaver";
					cont.DropItem(Loot.ImbueWeaponOrArmor(special, 6, 0, true));

					int onlyonedrop = Utility.RandomMinMax(0,1);
					if(onlyonedrop ==0 )cont.DropItem(new BoneContainer(Utility.RandomMinMax(0,2))); //new bone container 3 differnt types 0-2
					int stone = Utility.RandomMinMax(0,3); // get random gravestone type to drop

					if(onlyonedrop ==1 )
					{
						if(stone == 0) cont.DropItem(new GraveStone1());
						if(stone == 1) cont.DropItem(new GraveStone2());
						if(stone == 2) cont.DropItem(new GraveStone3());
						if(stone == 3) cont.DropItem(new GraveStone4());
					}

					for ( int i = 0; i < level * 5; ++i )
					{
						cont.DropItem(new Moonstone()); //drop moonstones
					}

					if (Utility.RandomDouble() <= 0.30 ) //30% chance to drop a statue
					{
						int whichone = Utility.RandomMinMax(0,1);
						if(whichone == 0)mx = new MonsterStatuette (m_Monster[6]);
						if(whichone == 1)mx = new MonsterStatuette (m_Monster[7]);
						mx.LootType = LootType.Regular;			// not blessed
						cont.DropItem( mx );				// drop it baby!
					}
					break;
				}

				case ChestThemeType.Pirate:
				{

					Bow special = new Bow();
					special.Name = "Bow of the Buccaneer";
					cont.DropItem(Loot.ImbueWeaponOrArmor(special, 6, 0, true));

					int onlyonedrop = Utility.RandomMinMax(0,1);
					PirateHat hat = new PirateHat();
					hat.Hue = 0x1;
					int oars = Utility.RandomMinMax(0,1); //2 oar types

					if(onlyonedrop ==0 )
					{
						if(oars == 0) cont.DropItem(new Oars1());
						if(oars == 1) cont.DropItem(new Oars2());
					}

					if(onlyonedrop == 1 )cont.DropItem(new GenieBottle(false) ); //lamp currently disabled genie not done
					if (Utility.RandomDouble() <= 0.50 )cont.DropItem(hat); // 50% chance at black piratehat

					if (Utility.RandomDouble() <= 0.30 ) //30% chance to drop a statue
					{
						int whichone = Utility.RandomMinMax(0,1);
						if(whichone == 0)mx = new MonsterStatuette (m_Monster[8]);
						if(whichone == 1) mx = new MonsterStatuette (m_Monster[9]);
						mx.LootType = LootType.Regular;					// not blessed
						cont.DropItem( mx );						// drop it baby!
					}
					break;
				}

				case ChestThemeType.Dragon:
				{
					WarFork special = new WarFork();
					special.Name = "Claw of the Dragon";
					cont.DropItem(Loot.ImbueWeaponOrArmor(special, 6, 0, true));

					int onlyonedrop = Utility.RandomMinMax(0,1);
					//new dragonhead trophydeed type
					if(onlyonedrop ==0 ) cont.DropItem(new TrophyDeed(8757, 8756, "a dragon head trophy", "a dragon head trophy", 10 ));
					int armor = Utility.RandomMinMax(0,2); // drop 1 piece of dragonarmor

					if(onlyonedrop == 1 )
					{
						if(armor == 0) cont.DropItem(new HangingDragonChest());
						if(armor == 1) cont.DropItem(new HangingDragonLegs());
						if(armor == 2) cont.DropItem(new HangingDragonArms());
					}

					if (Utility.RandomDouble() <= 0.30 ) //30% chance to drop a statue
					{
						int whichone = Utility.RandomMinMax(0,1);
						if(whichone == 0)mx = new MonsterStatuette (m_Monster[10]);
						if(whichone == 1)mx = new MonsterStatuette (m_Monster[11]);
						mx.LootType = LootType.Regular;			// not blessed
						cont.DropItem( mx );				// drop it baby!
					}
					break;
				}

				case ChestThemeType.Lizardmen: 
				{
					if (Utility.RandomBool())
						cont.DropItem( new LizardmansStaff() ); 
					else
						cont.DropItem( new LizardmansMace() ); 
				}
					break;
				
				case ChestThemeType.Ettin:
				{
					cont.DropItem( new EttinHammer() ); 
				}
					break;
				
				case ChestThemeType.Ogre: 
				{
					cont.DropItem( new OgresClub() ); 
				}
					break;

				case ChestThemeType.Ophidian:
				{
					cont.DropItem( new OphidianBardiche() ); 
				}
					break;
				
				case ChestThemeType.Skeleton:
				{
					switch (Utility.Random(3))
					{
						case 0: cont.DropItem( new SkeletonScimitar() ); break;
						case 1: cont.DropItem( new SkeletonAxe() ); break;
						case 2: cont.DropItem( new BoneMageStaff() ); break;
					}
				}
					break;

				case ChestThemeType.Ratmen:
				{
					if (Utility.RandomBool())
						cont.DropItem( new RatmanSword() ); 
					else
						cont.DropItem( new RatmanAxe() ); 
				}
					break;

				case ChestThemeType.Orc:
				{
					switch (Utility.Random(3))
					{
						case 0: cont.DropItem( new OrcClub() ); break;
						case 1: cont.DropItem( new OrcMageStaff() ); break;
						case 2: cont.DropItem( new OrcLordBattleaxe() ); break;
					}
				}
					break;

				case ChestThemeType.Terathan:
				{
					switch (Utility.Random(3))
					{
						case 0: cont.DropItem( new TerathanStaff() ); break;
						case 1: cont.DropItem( new TerathanSpear() ); break;
						case 2: cont.DropItem( new TerathanMace() ); break;
					}
				}
					break;

				case ChestThemeType.FrostTroll:
				{
					switch (Utility.Random(3))
					{
						case 0: cont.DropItem( new FrostTrollClub() ); break;
						case 1: cont.DropItem( new TrollAxe() ); break;
						case 2: cont.DropItem( new TrollMaul() ); break;
					}
				}
					break;

			}//end switch

		}

		private static void ClearAmounts( int[] list )
		{
			for ( int i = 0; i < list.Length; ++i )
				list[i] = 0;
		}

		private static void PackScroll( LockableContainer cont, int minCircle, int maxCircle )
		{
			PackScroll( cont, Utility.RandomMinMax( minCircle, maxCircle ) );
		}

		private static void PackScroll( LockableContainer cont, int circle )
		{
			int min = (circle - 1) * 8;

			cont.DropItem( Loot.RandomScroll( min, min + 7, SpellbookType.Regular ) );
		}

		private static void AddItems( Container cont, int[] amounts, Type[] types )
		{
			for ( int i = 0; i < amounts.Length && i < types.Length; ++i )	
			{
				if ( amounts[i] > 0 )	
				{
					try
					{
						m_Arguments[0] = amounts[i];
						Item item = (Item)Activator.CreateInstance( types[i], m_Arguments );
						cont.DropItem( item );	
					}
					catch (Exception ex) { EventSink.InvokeLogException(new LogExceptionEventArgs(ex)); }
				}
			}
		}

		private ArrayList m_Lifted = new ArrayList();

		private bool CheckLoot( Mobile m, bool criminalAction )
		{
			if ( m_Owner == null || m == m_Owner )
				return true;

			Party p = Party.Get( m_Owner );

			if ( p != null && p.Contains( m ) )
				return true;

			if (TreasureTheme.IsOverlandTheme(m_type) == true)
				return true;

			Map map = this.Map;

			if ( map != null && (map.Rules & MapRules.HarmfulRestrictions) == 0 )
			{
				if ( criminalAction )
					m.CriminalAction( true );
				else
					m.SendLocalizedMessage( 1010630 ); // Taking someone else's treasure is a criminal offense!

				return true;
			}

			m.SendLocalizedMessage( 1010631 ); // You did not discover this chest!
			return false;
		}

		public override bool CheckItemUse( Mobile from, Item item )
		{
			return CheckLoot( from, item != this ) && base.CheckItemUse( from, item );
		}

		private DateTime lastLift = DateTime.Now;
        public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{	// Thwart lift macros
			TimeSpan ts = DateTime.Now - lastLift;
			lastLift = DateTime.Now;
			if (ts.TotalSeconds < 1.8)
			{	// throttle
				from.SendMessage("You thrust your hand into the chest but come up empty handed.");
				reject = LRReason.Inspecific;
				return false;
			}
			return CheckLoot( from, true ) && base.CheckLift (from, item, ref reject);
		}

		public override void OnItemLifted( Mobile from, Item item )
		{
			bool notYetLifted = !m_Lifted.Contains( item );
			from.RevealingAction();

            // prevent the player from milking the chest by removing 'one from a stack'
            if (notYetLifted)
            {
                ArrayList tx = new ArrayList(FindItemsByType(item.GetType(), true));
                tx.Remove(item);
                if (tx.Count > 0)
                {
                    foreach (Item ix in tx)
                    {
                        if (ix.Amount > 1)
                        {   // player looks to be removing one from a stack
                            // disqualify the one they lifted
                            m_Lifted.Add(item);
                            notYetLifted = false;
                            break;
                        }
                    }
                    
                    // if they lifted >= 1, and left 1 disqualify the one they lifted
                    if (notYetLifted)
                        if (item.Amount > 1)
                        {
                            m_Lifted.Add(item);
                            notYetLifted = false;
                        }
                }
            }

			if ( notYetLifted )
			{
				m_Lifted.Add( item );
                double chance = 0.1 * (Level * .5);

				if (IsThemed == true && 0.1 >= Utility.RandomDouble() )
				{
					// 10% chance to spawn 2 monsters as is a Themed chest
					TreasureTheme.Spawn( m_Level, GetWorldLocation(), Map, from, IsThemed, m_type,false,false );
					TreasureTheme.Spawn( m_Level, GetWorldLocation(), Map, from, IsThemed, m_type,false,false );
				}
                else if (IsThemed == false && chance >= Utility.RandomDouble())
					TreasureTheme.Spawn( m_Level, GetWorldLocation(), Map, from, IsThemed, m_type,false,false );

				// Adam: Insure IOB wearers are not stealing from their kin
				BaseCreature witness=null;
				if (CheckThief(from, out witness))
				{
					from.SendMessage("You have been discovered stealing from your kin!");
					if (from.Hidden)
						from.RevealingAction();

					// attack kin to make them come after you
					from.DoHarmful(witness);
				}
			}

			base.OnItemLifted( from, item );
		}

		public bool CheckThief(Mobile from, out BaseCreature witness)
		{
			witness = null;

			if (from == null || !(from is PlayerMobile))
				return false;

			PlayerMobile pm = (PlayerMobile)from;

			if (pm.IOBAlignment == IOBAlignment.None)
				return false;

			IPooledEnumerable eable = pm.GetMobilesInRange(12);
			foreach (Mobile m in eable)
			{
				
				if (m == null || !(m is BaseCreature))
					continue ;

				BaseCreature bc = (BaseCreature)m;
				if (bc.Controlled == true)
					continue;

				if (pm.IOBAlignment == bc.IOBAlignment)
				{
					witness = bc;
					eable.Free();
					return true;
				}
			}
			eable.Free();

			return false;
		}


		public override bool OnDragDropInto( Mobile from, Item item, Point3D p )
		{
			from.SendLocalizedMessage( 1048122, "", 0x8A5 ); // The chest refuses to be filled with treasure again.

			return false;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			from.SendLocalizedMessage( 1048122, "", 0x8A5 ); // The chest refuses to be filled with treasure again.

			return false;
		}


		public TreasureMapChest( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 ); // version

            writer.Write(IsThemed);
            writer.Write((int)m_type);

			writer.Write( m_Owner );

			writer.Write( (int) m_Level );
			writer.WriteDeltaTime( m_DeleteTime );
			writer.WriteItemList( m_Lifted, true );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
                case 2:
                {
                    IsThemed = reader.ReadBool();
                    m_type = (ChestThemeType)reader.ReadInt();
                    goto case 1;
                }

				case 1:
				{
					m_Owner = reader.ReadMobile();

					goto case 0;
				}
				case 0:
				{
					m_Level = reader.ReadInt();
					m_DeleteTime = reader.ReadDeltaTime();
					m_Lifted = reader.ReadItemList();

					m_Timer = new DeleteTimer( this, m_DeleteTime );
					m_Timer.Start();

					break;
				}
			}
		}

		public override void OnAfterDelete()
		{
			if ( m_Timer != null )
				m_Timer.Stop();

			m_Timer = null;

			base.OnAfterDelete();
		}

		public override void GetContextMenuEntries(Mobile from, ArrayList list)
		{
			base.GetContextMenuEntries( from, list );

			if ( from.Alive )
				list.Add( new RemoveEntry( from, this ) );
		}

		public void BeginRemove( Mobile from )
		{
			if ( !from.Alive )
				return;

			from.CloseGump( typeof( RemoveGump ) );
			from.SendGump( new RemoveGump( from, this ) );
		}

		public void EndRemove( Mobile from )
		{
			if ( Deleted || !from.InRange( GetWorldLocation(), 3 ) )
				return;

			from.SendLocalizedMessage( 1048124, "", 0x8A5 ); // The old, rusted chest crumbles when you hit it.
			this.Delete();
		}

		private class RemoveGump : Gump
		{
			private Mobile m_From;
			private TreasureMapChest m_Chest;

			public RemoveGump( Mobile from, TreasureMapChest chest ) : base( 15, 15 )
			{
				m_From = from;
				m_Chest = chest;

				Closable = false;
				Disposable = false;

				AddPage( 0 );

				AddBackground( 30, 0, 240, 240, 2620 );

				AddHtmlLocalized( 45, 15, 200, 80, 1048125, 0xFFFFFF, false, false ); // When this treasure chest is removed, any items still inside of it will be lost.
				AddHtmlLocalized( 45, 95, 200, 60, 1048126, 0xFFFFFF, false, false ); // Are you certain you're ready to remove this chest?

				AddButton( 40, 153, 4005, 4007, 1, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 75, 155, 180, 40, 1048127, 0xFFFFFF, false, false ); // Remove the Treasure Chest

				AddButton( 40, 195, 4005, 4007, 2, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 75, 197, 180, 35, 1006045, 0xFFFFFF, false, false ); // Cancel
			}

			public override void OnResponse( NetState sender, RelayInfo info )
			{
				if ( info.ButtonID == 1 )
					m_Chest.EndRemove( m_From );
			}
		}

		private class RemoveEntry : ContextMenuEntry
		{
			private Mobile m_From;
			private TreasureMapChest m_Chest;

			public RemoveEntry( Mobile from, TreasureMapChest chest ) : base( 6149, 3 )
			{
				m_From = from;
				m_Chest = chest;

				Enabled = ( from == chest.Owner || chest.Items.Count == 0);
			}

			public override void OnClick()
			{
				if ( m_Chest.Deleted || !m_From.CheckAlive() )
					return;

				m_Chest.BeginRemove( m_From );
			}
		}

		private class DeleteTimer : Timer
		{
			private Item m_Item;

			public DeleteTimer( Item item, DateTime time ) : base( time - DateTime.Now )
			{
				m_Item = item;
				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{
				m_Item.Delete();
			}
		}

		public override void OnTelekinesis( Mobile from )
		{
			//Do nothing, telekinesis doesn't work on a TMap.
		}

		public override bool OnFailDisarm(Mobile from)
		{
			bool bExploded = false;

			double rtskill = from.Skills[SkillName.RemoveTrap].Value;

			double chance = (TrapPower-rtskill)/800;

			//make sure there's some chance to trip
			if( chance <= 0 ) chance = .005; //minimum of 1/200 trip
			if( chance >= 1 ) chance = .995;
			chance *= TrapSensitivity;

			//debug message only available to non-Player level
			if( from.AccessLevel > AccessLevel.Player)
			{
				from.SendMessage("Chance to trip trap: " + chance);
			}

			if( Utility.RandomDouble() < chance )
			{ //trap is tripped, effect disarmer
				int damage = TrapPower/2 + (Utility.Random(4,12)*TrapPower/25) - (int)rtskill/5;
				int traptype = 0;
				traptype = Utility.Random(0, 3);

				switch ( traptype )
				{
					case 0: //explosion
					{
						from.SendLocalizedMessage( 502999 ); // You set off a trap!

						if ( from.InRange( GetWorldLocation(), 2 ) )
						{
							AOS.Damage( from, damage, 0, 100, 0, 0, 0 );
							from.SendLocalizedMessage( 503000 ); // Your skin blisters from the heat!
						}

						Point3D loc = GetWorldLocation();

						Effects.PlaySound( loc, Map, 0x307 );
						Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y + 1, loc.Z - 11 ), Map, 0x36BD, 15 );

						break;
					}
					case 1: //dart
					{
						from.SendLocalizedMessage( 502999 ); // You set off a trap!

						if ( from.InRange( GetWorldLocation(), 2 ) )
						{
							AOS.Damage( from, damage/2, 100, 0, 0, 0, 0 );
							from.SendLocalizedMessage( 502380 ); // A dart embeds...
						}

						Point3D loc = GetWorldLocation();

						Effects.PlaySound( loc, Map, 0x223 );
						//What effect?!?
						//Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y + 1, loc.Z - 11 ), Map, 0x36BD, 15 );
						break;
					}
					case 2: //poison
					{
						from.SendLocalizedMessage( 502999 ); // You set off a trap!

						if ( from.InRange( GetWorldLocation(), 2 ) )
						{
							Poison p = Poison.Lesser;
							if( damage >= 30 )
								p = Poison.Regular;
							if( damage >= 60 )
								p = Poison.Greater;
							if( damage >= 90 )
								p = Poison.Deadly;
							if( damage >= 100 )
								p = Poison.Lethal;

							from.ApplyPoison( from, p );
							from.SendLocalizedMessage( 503004 ); // You are enveloped...
						}

						Point3D loc = GetWorldLocation();

						Effects.PlaySound( loc, Map, 0x231 );
						Effects.SendLocationEffect( new Point3D( loc.X + 1, loc.Y + 1, loc.Z - 11 ), Map, 0x11A6, 20 );
						break;
					}
				}
				from.RevealingAction();
				bExploded = true;
			}

			return bExploded;
		}

		public override bool ExecuteTrap( Mobile from )
		{
			//In order to REQUIRE the remove trap skill for
			//Treasure Map Chests, make sure that the trap resets immediately
			//after the trap is tripped.
			TrapType originaltrap = TrapType;

			bool bReturn = base.ExecuteTrap(from);

			//reset trap!
			TrapType = originaltrap;

			return bReturn;
		}
	}
}
