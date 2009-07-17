using System;
using Server;
using Server.Items;
using Server.Spells;
using Server.Mobiles;
using Server.Network;
using System.Collections;

namespace Server.Regions
{
	public class TownshipRegion : Server.Regions.CustomRegion
	{
		public TownshipRegion(TownshipStone townstone, Map map) : base(townstone, map)
		{
			Setup();
		}
		public TownshipRegion(RegionControl rc, Map map) : base(rc, map)
		{
			Setup();
		}

		private void Setup()
		{
			this.IsGuarded = false;
		}

		private bool IsControllerGood()
		{
			return (this.m_Controller != null && this.m_Controller is TownshipStone);
		}

		public static TownshipRegion GetTownshipAt(Mobile m)
		{
			CustomRegion c = CustomRegion.FindDRDTRegion(m);
			if (c is TownshipRegion)
			{
				return (TownshipRegion)c;
			}

			return null;
		}

		public override void OnEnter(Mobile m)
		{
			if( IsControllerGood() )
			{
				//forward to controller, which keeps track of everything
				((TownshipStone)this.m_Controller).OnEnter( m );
			}

			base.OnEnter (m);
		}

		public override void OnExit(Mobile m)
		{
			if( IsControllerGood() )
			{
				//forward to controller, which keeps track of everything
				((TownshipStone)this.m_Controller).OnExit( m );
			}

			base.OnExit (m);
		}

		public bool CanBuildHouseInTownship(Mobile m)
		{
			PlayerMobile pm = m as PlayerMobile;
			if (pm != null)
			{
				if (IsControllerGood())
				{
					return ((TownshipStone)this.m_Controller).CanBuildHouseInTownship(pm);
				}
				else
				{
					//if bad controller, default to yes.
					return true;
				}
			}

			return false;
		}

		public override bool IsNoMurderZone
		{
			get
			{
				if( IsControllerGood() )
				{
					return !((TownshipStone)this.m_Controller).MurderZone;
				}
				else
				{
					return base.IsNoMurderZone;
				}
			}
		}

		public override bool IsMobileCountable(Mobile aggressor)
		{
			if (IsControllerGood())
			{
				return ((TownshipStone)this.m_Controller).IsMobileCountable(aggressor);
			}
			else
			{
				return base.IsMobileCountable(aggressor);
			}
		}

		public TownshipStone TStone
		{
			get
			{
				if( IsControllerGood() )
				{
					return (TownshipStone)this.m_Controller;
				}
				else
				{
					return null;
				}
			}
		}


	}
}
