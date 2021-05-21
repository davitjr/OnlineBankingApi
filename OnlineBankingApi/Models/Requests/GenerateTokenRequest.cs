using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
	public class GenerateTokenRequest
	{
		/// <summary>
		/// Թոքենիզացվող քարտի ունիկալ համար
		/// </summary>
		public ulong ProductId { get; set; }
		/// <summary>
		/// Օգտագործողի անուն
		/// </summary>
		internal string MobileUserName { get; set; }
	}
}
