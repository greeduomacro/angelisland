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

/* Scripts/Items/Addons/AddonComponent.cs
 * ChangeLog
 *  7/30/06, Kit
 *		Add Forge Bellows, Rise of the animated forges!
 *  6/5/04, Pix
 *		Merged in 1.0RC0 code.
 */

using System;
using Server;
using Server.Scripts.Commands;

namespace Server.Items
{
	[Server.Engines.Craft.Anvil]
	public class AnvilComponent : AddonComponent
	{
		[Constructable]
		public AnvilComponent( int itemID ) : base( itemID )
		{
		}

		public AnvilComponent( Serial serial ) : base( serial )
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
	}

	[Server.Engines.Craft.Forge]
	public class ForgeComponent : AddonComponent
	{
		[Constructable]
		public ForgeComponent( int itemID ) : base( itemID )
		{
		}

		public ForgeComponent( Serial serial ) : base( serial )
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
	}

	[Server.Engines.Craft.Forge]
	public class ForgeBellows : AddonComponent
	{
		private bool locked;

		[Constructable]
		public ForgeBellows(int itemID) : base( itemID )
		{
		}

		public ForgeBellows( Serial serial ) : base( serial )
		{
		}

		public bool Locked
		{
			get{return locked;}
			set{locked = value;}
		}

		public override void OnDoubleClick( Mobile from )
		{
			if(!Locked)
			{
				try
				{
					from.PlaySound(43);
					this.ItemID +=3;
					if(this.ItemID == 6537 || this.ItemID == 6525)
					{
						if(((AddonComponent)Addon.Components[1]) != null)
							((AddonComponent)Addon.Components[1]).ItemID += 3;
					}
					if(this.ItemID == 6549 || this.ItemID == 6561)
					{
						if(((AddonComponent)Addon.Components[2]) != null)
							((AddonComponent)Addon.Components[2]).ItemID += 3;
					}

					Locked = true;

					new BellowTimer(this).Start();
				}
				catch(Exception exc)
				{
					LogHelper.LogException(exc);
					System.Console.WriteLine("catch I Item at {0}: Send to Zen please : ", this.Location);
					System.Console.WriteLine("Exception caught in ForgeBellows.DoubleClick: " + exc.Message);
					System.Console.WriteLine(exc.StackTrace);
				}
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	

		private class BellowTimer : Timer
		{
			private	ForgeBellows m_bellows;
			
			public BellowTimer(ForgeBellows bellow ) : base( TimeSpan.FromSeconds( 2.0 ) )
			{
				Priority = TimerPriority.TwoFiftyMS;
				m_bellows = bellow;
			}

			protected override void OnTick()
			{
				try
				{
					m_bellows.ItemID -= 3;
					if(m_bellows.ItemID == 6534 || m_bellows.ItemID == 6522)
					{
						if(((AddonComponent)m_bellows.Addon.Components[1]) != null)
							((AddonComponent)m_bellows.Addon.Components[1]).ItemID -= 3;
					}

					if(m_bellows.ItemID == 6546 || m_bellows.ItemID == 6558)
					{
						if(((AddonComponent)m_bellows.Addon.Components[1]) != null)
							((AddonComponent)m_bellows.Addon.Components[2]).ItemID -= 3;
					}
					
					m_bellows.Locked = false;
				}
				catch(Exception exc)
				{
					LogHelper.LogException(exc);
					System.Console.WriteLine("catch I Item at {0}: Send to Zen please: ", m_bellows.Location);
					System.Console.WriteLine("Exception caught in ForgeBellows.BellowTimer: " + exc.Message);
					System.Console.WriteLine(exc.StackTrace);
				}
			}
		}
	}

	public class LocalizedAddonComponent : AddonComponent
	{
		private int m_LabelNumber;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Number
		{
			get{ return m_LabelNumber; }
			set{ m_LabelNumber = value; InvalidateProperties(); }
		}

		public override int LabelNumber{ get{ return m_LabelNumber; } }

		[Constructable]
		public LocalizedAddonComponent( int itemID, int labelNumber ) : base( itemID )
		{
			m_LabelNumber = labelNumber;
		}

		public LocalizedAddonComponent( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_LabelNumber );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_LabelNumber = reader.ReadInt();
					break;
				}
			}
		}
	}

	public class AddonComponent : Item, IChopable
	{
		private Point3D m_Offset;
		private BaseAddon m_Addon;

		[CommandProperty( AccessLevel.GameMaster )]
		public BaseAddon Addon
		{
			get
			{
				return m_Addon;
			}
			set
			{
				m_Addon = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Point3D Offset
		{
			get
			{
				return m_Offset;
			}
			set
			{
				m_Offset = value;
			}
		}

		[Hue, CommandProperty( AccessLevel.GameMaster )]
		public override int Hue
		{
			get
			{
				return base.Hue;
			}
			set
			{
				base.Hue = value;

				if ( m_Addon != null && m_Addon.ShareHue )
					m_Addon.Hue = value;
			}
		}

		[Constructable]
		public AddonComponent( int itemID ) : base( itemID )
		{
			Movable = false;
			ApplyLightTo( this );
		}

		public AddonComponent( Serial serial ) : base( serial )
		{
		}

		public void OnChop( Mobile from )
		{
			if ( m_Addon != null && from.InRange( GetWorldLocation(), 3 ) )
				m_Addon.OnChop( from );
			else
				from.SendLocalizedMessage( 500446 ); // That is too far away.
		}

		public override void OnLocationChange( Point3D old )
		{
			if ( m_Addon != null )
				m_Addon.Location = new Point3D( X - m_Offset.X, Y - m_Offset.Y, Z - m_Offset.Z );
		}

		public override void OnMapChange()
		{
			if ( m_Addon != null )
				m_Addon.Map = Map;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( m_Addon != null )
				m_Addon.Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( m_Addon );
			writer.Write( m_Offset );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Addon = reader.ReadItem() as BaseAddon;
					m_Offset = reader.ReadPoint3D();

					if ( m_Addon != null )
						m_Addon.OnComponentLoaded( this );

					ApplyLightTo( this );

					break;
				}
			}
		}

		public static void ApplyLightTo( Item item )
		{
			if ( (item.ItemData.Flags & TileFlag.LightSource) == 0 )
				return; // not a light source

			int itemID = item.ItemID;

			for ( int i = 0; i < m_Entries.Length; ++i )
			{
				LightEntry entry = m_Entries[i];
				int[] toMatch = entry.m_ItemIDs;
				bool contains = false;

				for ( int j = 0; !contains && j < toMatch.Length; ++j )
					contains = ( itemID == toMatch[j] );

				if ( contains )
				{
					item.Light = entry.m_Light;
					return;
				}
			}
		}

		private static LightEntry[] m_Entries = new LightEntry[]
			{
				new LightEntry( LightType.WestSmall, 1122, 1123, 1124, 1141, 1142, 1143, 1144, 1145, 1146, 2347, 2359, 2360, 2361, 2362, 2363, 2364, 2387, 2388, 2389, 2390, 2391, 2392 ),
				new LightEntry( LightType.NorthSmall, 1131, 1133, 1134, 1147, 1148, 1149, 1150, 1151, 1152, 2352, 2373, 2374, 2375, 2376, 2377, 2378, 2401, 2402, 2403, 2404, 2405, 2406 ),
				new LightEntry( LightType.Circle300, 6526, 6538 )
			};

		private class LightEntry
		{
			public LightType m_Light;
			public int[] m_ItemIDs;

			public LightEntry( LightType light, params int[] itemIDs )
			{
				m_Light = light;
				m_ItemIDs = itemIDs;
			}
		}
	}
}