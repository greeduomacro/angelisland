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

namespace Server.Items
{
	/// <summary>
	/// Also known as the Haymaker, this attack dramatically increases the damage done by a weapon reaching its mark.
	/// </summary>
	public class CrushingBlow : WeaponAbility
	{
		public CrushingBlow()
		{
		}

		public override int BaseMana{ get{ return 25; } }
		public override double DamageScalar{ get{ return 1.5; } }


		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
				return;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1060090 ); // You have delivered a crushing blow!
			defender.SendLocalizedMessage( 1060091 ); // You take extra damage from the crushing attack!

			defender.PlaySound( 0x1E1 );
			defender.FixedParticles( 0, 1, 0, 9946, EffectLayer.Head );

			Effects.SendMovingParticles( new Entity( Serial.Zero, new Point3D( defender.X, defender.Y, defender.Z + 50 ), defender.Map ), new Entity( Serial.Zero, new Point3D( defender.X, defender.Y, defender.Z + 20 ), defender.Map ), 0xFB4, 1, 0, false, false, 0, 3, 9501, 1, 0, EffectLayer.Head, 0x100 );
		}
	}
} 
