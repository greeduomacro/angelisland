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
using Server;
using Server.Engines.Craft;

namespace Server.Items
{
	[FlipableAttribute( 0x13E4, 0x13E3 )]
	public class AncientSmithyHammer : BaseTool
	{
		private int m_Bonus;
		private SkillMod m_SkillMod;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Bonus
		{
			get
			{
				return m_Bonus;
			}
			set
			{
				m_Bonus = value;
				InvalidateProperties();

				if ( m_Bonus == 0 )
				{
					if ( m_SkillMod != null )
						m_SkillMod.Remove();

					m_SkillMod = null;
				}
				else if ( m_SkillMod == null && Parent is Mobile )
				{
					m_SkillMod = new DefaultSkillMod( SkillName.Blacksmith, true, m_Bonus );
					((Mobile)Parent).AddSkillMod( m_SkillMod );
				}
				else if ( m_SkillMod != null )
				{
					m_SkillMod.Value = m_Bonus;
				}
			}
		}

		public override void OnAdded( object parent )
		{
			base.OnAdded( parent );

			if ( m_Bonus != 0 && parent is Mobile )
			{
				if ( m_SkillMod != null )
					m_SkillMod.Remove();

				m_SkillMod = new DefaultSkillMod( SkillName.Blacksmith, true, m_Bonus );
				((Mobile)parent).AddSkillMod( m_SkillMod );
			}
		}

		public override void OnRemoved( object parent )
		{
			base.OnRemoved( parent );

			if ( m_SkillMod != null )
				m_SkillMod.Remove();

			m_SkillMod = null;
		}

		public override CraftSystem CraftSystem{ get{ return DefBlacksmithy.CraftSystem; } }
		public override int LabelNumber{ get{ return 1045127; } } // ancient smithy hammer

		[Constructable]
		public AncientSmithyHammer( int bonus ) : this( bonus, 600 )
		{
		}

		[Constructable]
		public AncientSmithyHammer( int bonus, int uses ) : base( uses, 0x13E4 )
		{
			m_Bonus = bonus;
			Weight = 8.0;
			Layer = Layer.OneHanded;
			Hue = 0x482;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Bonus != 0 )
				list.Add( 1060451, "#1042354\t{0}", m_Bonus.ToString() ); // ~1_skillname~ +~2_val~
		}

		public AncientSmithyHammer( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_Bonus );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 0:
				{
					m_Bonus = reader.ReadInt();
					break;
				}
			}

			if ( m_Bonus != 0 && Parent is Mobile )
			{
				if ( m_SkillMod != null )
					m_SkillMod.Remove();

				m_SkillMod = new DefaultSkillMod( SkillName.Blacksmith, true, m_Bonus );
				((Mobile)Parent).AddSkillMod( m_SkillMod );
			}

			if ( Hue == 0 )
				Hue = 0x482;
		}
	}
} 
