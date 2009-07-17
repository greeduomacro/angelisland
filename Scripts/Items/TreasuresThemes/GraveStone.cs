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

/* Scripts/Items/TreasureThemes/GraveStone.cs
 * CHANGELOG
 *	04/07/05, Kitaras	
 *		Initial Creation
 */

using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Misc;

namespace Server.Items
{
  
	public class BaseGraveStone: Item 
	{	
		private string m_Description;

		[CommandProperty( AccessLevel.GameMaster )]
		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
				InvalidateProperties();
			}
		}
      	
		
        	public BaseGraveStone(Serial serial) : base( serial )
        	{
			Weight = 93.0;
			Name = "";	
		}

		public BaseGraveStone(int itemID) : base(itemID ) 
		{
			Weight = 93.0;
			Name = "";
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			writer.Write( m_Description );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			

			int version = reader.ReadInt();
			m_Description = reader.ReadString();
		}

		public override void OnSingleClick( Mobile from )
		{
			if ( m_Description != null && m_Description.Length > 0 )
				LabelTo( from, m_Description );

			base.OnSingleClick( from );
		}
      
  	}

	[Flipable( 4465, 4466 )]  
	public class GraveStone1: BaseGraveStone
	{	
		

       		[Constructable]
		public GraveStone1() : base(4466) // 4 differnt stone each haveing 2 directions
		{
		
		}

        	public GraveStone1(Serial serial) : base( serial )
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

	[Flipable( 4476, 4475 )]  
	public class GraveStone2: BaseGraveStone
	{	
		
       		[Constructable]
		public GraveStone2() : base(4476) // 4 differnt stone each haveing 2 directions
		{
			Weight = 95.0;
		}

        	public GraveStone2(Serial serial) : base( serial )
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

	[Flipable( 4473, 4474 )]  
	public class GraveStone3: BaseGraveStone
	{	
		
       		[Constructable]
		public GraveStone3() : base(4473) 
		{
			Weight = 97.0;
		}

        	public GraveStone3(Serial serial) : base( serial )
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

	[Flipable( 4477, 4478 )]  
	public class GraveStone4: BaseGraveStone
	{	
	
       		[Constructable]
		public GraveStone4() : base(4477) // 4 differnt stone each haveing 2 directions
		{
			Weight = 98.0;
		}

        	public GraveStone4(Serial serial) : base( serial )
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

}
