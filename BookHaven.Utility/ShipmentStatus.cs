using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.Utility
{
	public enum ShipmentStatus
	{
        Idle = 0,
        Pending = 1,
		Approved = 2,
		Processing = 3,
		Shipped = 4,
		Cancelled = 5,
		Refunded = 6
	}
}
