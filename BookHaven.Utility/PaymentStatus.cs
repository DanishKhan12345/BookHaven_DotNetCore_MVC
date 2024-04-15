using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHaven.Utility
{
	public enum PaymentStatus
	{
		Idle = 0,
		Pending = 1,
		Approved = 2,
		ApprovedForDelayedPayment = 3,
		Rejected = 4
	}
}
