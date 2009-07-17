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
 *	restrictions set forth in subaragraph (c)(1)(ii) of the Rights in
 * 	Technical Data and Computer Software clause at DFARS 252.227-7013.
 *
 *	Angel Island UO Shard	Version 1.0
 *			Release A
 *			March 25, 2004
 */

/* Scripts/Items/Misc/JailExitGate.cs
 * CHANGELOG
 *  11/13/05 Taran Kain
 *		Changed sentence back to using DateTime.Now, allowing players to burn off time in or out of game.
 *  11/06/05 Taran Kain
 *		Changed inmate sentence storage method to use GameTime - now players will only burn off their jailtime if they're in-game.
 *  09/01/05 Taran Kain
 *		First version. Keeps track of [jail'ed players and allows them to leave when their sentence is up.
 *		Added JEGFactory object to only allow Administrators to create JailExitGates
 */

using System;
using System.Collections;
using System.Text;
using Server;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Items
{
	public class JEGFactory : Item
	{
		[Constructable]
		public JEGFactory() : base(0xF8B)
		{
			Name = "Double click to create a JailExitGate here.";
		}
		
		public JEGFactory(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.Administrator)
			{
				new JailExitGate().MoveToWorld(Location, Map);
				this.Delete();
			}
			else
				from.SendMessage("You must have Administrator access to use this object.");
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
		}
	}

	
	public class JailExitGate : Moongate
	{
		public static JailExitGate Instance = null;
				
		private Hashtable m_Inmates;

		public JailExitGate() : base( new Point3D(829, 1081, 5), Map.Felucca ) // gate points to Oc'Nivelle Bank
		{
			Hue = 0x2D1;
			Name = "to Oc'Neville";
			m_Inmates = new Hashtable();
			VerifyHighlander();
		}

		public JailExitGate( Serial serial ) : base( serial )
		{
			VerifyHighlander();
		}

		// there can only be one!!
		private void VerifyHighlander()
		{
			if (JailExitGate.Instance != null)
			{
				m_Inmates = new Hashtable(Instance.m_Inmates);
				Instance.Delete();
			}

			JailExitGate.Instance = this;
		}

		public override bool ValidateUse(Mobile from, bool message)
		{
			if (!base.ValidateUse(from, message))
				return false;

			if (m_Inmates == null || Instance == null)
			{
				from.SendMessage("Tell a GM that the JailExitGate is broken. Hopefully they'll pity you.");
				return false;
			}
			
			if (!m_Inmates.ContainsKey(from) || !(from is PlayerMobile))
				return true;

			if (!(m_Inmates[from] is DateTime) || ((DateTime)m_Inmates[from]) <= DateTime.Now)
			{
				m_Inmates.Remove(from);
				return true;
			}
			
			TimeSpan ts = (DateTime)m_Inmates[from] - DateTime.Now;
			StringBuilder sb = new StringBuilder();
			if (ts.TotalHours >= 1)
			{
				sb.AppendFormat("{0} hours", (int)ts.TotalHours);
				ts -= TimeSpan.FromHours((int)ts.TotalHours);
				if (ts.Minutes > 0)
					sb.Append(" and ");
			}
			if (ts.Minutes > 0)
				sb.AppendFormat("{0} minutes", ts.Minutes);

			from.SendMessage("There are still {0} left in your sentence.", sb.ToString());
			return false;
		}

		public static void AddInmate(Mobile inmate, int hours)
		{
			if (Instance == null || Instance.m_Inmates == null || !(inmate is PlayerMobile))
				return;

			Instance.m_Inmates[inmate] = DateTime.Now + TimeSpan.FromHours(hours);
		}

		private void ValidateInmates()
		{
			int count = m_Inmates.Count;
			ArrayList keys = new ArrayList(m_Inmates.Keys),
				values = new ArrayList(m_Inmates.Values);

			for (int i = 0; i < count; i++)
			{
				PlayerMobile pm = keys[i] as PlayerMobile;
				if (pm == null || !(values[i] is DateTime) || ((DateTime)values[i]) <= DateTime.Now)
					m_Inmates.Remove(keys[i]);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize (writer);

			writer.Write((int)3);

			ValidateInmates();
			int count = m_Inmates.Count;
			ArrayList keys = new ArrayList(m_Inmates.Keys),
					  values = new ArrayList(m_Inmates.Values);

			writer.Write(count);
			for (int i = 0; i < count; i++)
			{
				writer.Write((Mobile)keys[i]);
				writer.Write((DateTime)values[i]);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize (reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 3:
				{
					m_Inmates = new Hashtable();
					int count = reader.ReadInt();
					for (int i = 0; i < count; i++)
					{
						Mobile m = reader.ReadMobile();
						DateTime dt = reader.ReadDateTime();
						m_Inmates.Add(m, dt);
					}
					break;
				}
				case 2:
				{
					m_Inmates = new Hashtable();
					int count = reader.ReadInt();
					for (int i = 0; i < count; i++)
					{
						Mobile m = reader.ReadMobile();
						TimeSpan ts = reader.ReadTimeSpan();
						DateTime dt = DateTime.Now + ts;
						m_Inmates.Add(m, dt);
					}
					break;
				}
				case 1:
				{
					m_Inmates = new Hashtable();
					int count = reader.ReadInt();
					for (int i = 0; i < count; i++)
					{
						Mobile m = reader.ReadMobile();
						DateTime dt = reader.ReadDateTime();
						m_Inmates.Add(m, dt);
					}
					break;
				}
				case 0:
				{
					m_Inmates = new Hashtable();
					int count = reader.ReadInt();
					for (int i = 0; i < count; i++)
					{
						Mobile m = reader.ReadMobile();
						DateTime dt = reader.ReadDateTime() + TimeSpan.FromHours(24);
						m_Inmates.Add(m, dt);
					}
					break;
				}
			}
		}
	}
}
