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

/* Scripts\Engines\IOBSystem\Mobiles\Guards\Types\FactionHenchman.cs
 * ChangeLog
 *	1/2/09, Adam
 *		Add/Update settings and supplies (pots etc) to support the new HybridAI
 *  12/17/08, Adam
 *		Initial creation
 */

using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Engines.IOBSystem.Attributes;

namespace Server.Engines.IOBSystem
{
	[KinFactionGuardType(KinFactionGuardCostTypes.LowCost)]
	public class FactionHenchman : BaseFactionGuard
	{
		[Constructable]
		public FactionHenchman() : base( "the henchman" )
		{
			BardImmune = true;
			
			UsesHumanWeapons = true;
			UsesBandages = true;
			UsesPotions = true;
			CanRun = true;
			CanReveal = false;

			GenerateBody( false, true );

			SetStr( 91, 115 );
			SetDex( 61, 85 );
			SetInt( 81, 95 );

			VirtualArmor = 8;

			SetSkill( SkillName.Fencing, 80.0, 90.0 );
			SetSkill( SkillName.Wrestling, 80.0, 90.0 );
			SetSkill( SkillName.Tactics, 80.0, 90.0 );
			SetSkill( SkillName.MagicResist, 80.0, 90.0 );
			SetSkill( SkillName.Healing, 80.0, 90.0 );
			SetSkill( SkillName.Anatomy, 80.0, 90.0 );

			AddItem( new StuddedChest() );
			AddItem( new StuddedLegs() );
			AddItem( new StuddedArms() );
			AddItem( new StuddedGloves() );
			AddItem( new StuddedGorget() );
			AddItem( new Boots() );
			AddItem( new Spear() );

			PackItem(new Bandage(Utility.RandomMinMax(VirtualArmor, VirtualArmor * 2)));
			PackWeakPotions( 1, 4 );
		}

		public FactionHenchman( Serial serial ) : base( serial )
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