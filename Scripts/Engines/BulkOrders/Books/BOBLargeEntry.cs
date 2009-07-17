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

namespace Server.Engines.BulkOrders
{
	public class BOBLargeEntry
	{
		private bool m_RequireExceptional;
		private BODType m_DeedType;
		private BulkMaterialType m_Material;
		private int m_AmountMax;
		private int m_Price;
		private BOBLargeSubEntry[] m_Entries;

		public bool RequireExceptional{ get{ return m_RequireExceptional; } }
		public BODType DeedType{ get{ return m_DeedType; } }
		public BulkMaterialType Material{ get{ return m_Material; } }
		public int AmountMax{ get{ return m_AmountMax; } }
		public int Price{ get{ return m_Price; } set{ m_Price = value; } }
		public BOBLargeSubEntry[] Entries{ get{ return m_Entries; } }

		public Item Reconstruct()
		{
			LargeBOD bod = null;

			if ( m_DeedType == BODType.Smith )
				bod = new LargeSmithBOD( m_AmountMax, m_RequireExceptional, m_Material, ReconstructEntries() );
			else if ( m_DeedType == BODType.Tailor )
				bod = new LargeTailorBOD( m_AmountMax, m_RequireExceptional, m_Material, ReconstructEntries() );

			for ( int i = 0; bod != null && i < bod.Entries.Length; ++i )
				bod.Entries[i].Owner = bod;

			return bod;
		}

		private LargeBulkEntry[] ReconstructEntries()
		{
			LargeBulkEntry[] entries = new LargeBulkEntry[m_Entries.Length];

			for ( int i = 0; i < m_Entries.Length; ++i )
			{
				entries[i] = new LargeBulkEntry( null, new SmallBulkEntry( m_Entries[i].ItemType, m_Entries[i].Number, m_Entries[i].Graphic ) );
				entries[i].Amount = m_Entries[i].AmountCur;
			}

			return entries;
		}

		public BOBLargeEntry( LargeBOD bod )
		{
			m_RequireExceptional = bod.RequireExceptional;

			if ( bod is LargeTailorBOD )
				m_DeedType = BODType.Tailor;
			else if ( bod is LargeSmithBOD )
				m_DeedType = BODType.Smith;

			m_Material = bod.Material;
			m_AmountMax = bod.AmountMax;

			m_Entries = new BOBLargeSubEntry[bod.Entries.Length];

			for ( int i = 0; i < m_Entries.Length; ++i )
				m_Entries[i] = new BOBLargeSubEntry( bod.Entries[i] );
		}

		public BOBLargeEntry( GenericReader reader )
		{
			int version = reader.ReadEncodedInt();

			switch ( version )
			{
				case 0:
				{
					m_RequireExceptional = reader.ReadBool();

					m_DeedType = (BODType)reader.ReadEncodedInt();

					m_Material = (BulkMaterialType)reader.ReadEncodedInt();
					m_AmountMax = reader.ReadEncodedInt();
					m_Price = reader.ReadEncodedInt();

					m_Entries = new BOBLargeSubEntry[reader.ReadEncodedInt()];

					for ( int i = 0; i < m_Entries.Length; ++i )
						m_Entries[i] = new BOBLargeSubEntry( reader );

					break;
				}
			}
		}

		public void Serialize( GenericWriter writer )
		{
			writer.WriteEncodedInt( 0 ); // version

			writer.Write( (bool) m_RequireExceptional );

			writer.WriteEncodedInt( (int) m_DeedType );
			writer.WriteEncodedInt( (int) m_Material );
			writer.WriteEncodedInt( (int) m_AmountMax );
			writer.WriteEncodedInt( (int) m_Price );

			writer.WriteEncodedInt( (int) m_Entries.Length );

			for ( int i = 0; i < m_Entries.Length; ++i )
				m_Entries[i].Serialize( writer );
		}
	}
}�