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
	public class BOBFilter
	{
		private int m_Type;
		private int m_Quality;
		private int m_Material;
		private int m_Quantity;

		public bool IsDefault
		{
			get{ return ( m_Type == 0 && m_Quality == 0 && m_Material == 0 && m_Quantity == 0 ); }
		}

		public void Clear()
		{
			m_Type = 0;
			m_Quality = 0;
			m_Material = 0;
			m_Quantity = 0;
		}

		public int Type
		{
			get{ return m_Type; }
			set{ m_Type = value; }
		}

		public int Quality
		{
			get{ return m_Quality; }
			set{ m_Quality = value; }
		}

		public int Material
		{
			get{ return m_Material; }
			set{ m_Material = value; }
		}

		public int Quantity
		{
			get{ return m_Quantity; }
			set{ m_Quantity = value; }
		}

		public BOBFilter()
		{
		}

		public BOBFilter( GenericReader reader )
		{
			int version = reader.ReadEncodedInt();

			switch ( version )
			{
				case 1:
				{
					m_Type = reader.ReadEncodedInt();
					m_Quality = reader.ReadEncodedInt();
					m_Material = reader.ReadEncodedInt();
					m_Quantity = reader.ReadEncodedInt();

					break;
				}
			}
		}

		public void Serialize( GenericWriter writer )
		{
			if ( IsDefault )
			{
				writer.WriteEncodedInt( 0 ); // version
			}
			else
			{
				writer.WriteEncodedInt( 1 ); // version

				writer.WriteEncodedInt( m_Type );
				writer.WriteEncodedInt( m_Quality );
				writer.WriteEncodedInt( m_Material );
				writer.WriteEncodedInt( m_Quantity );
			}
		}
	}
} 
