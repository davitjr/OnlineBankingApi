using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models
{
	public class GenerateTokenResponse
	{
		public string JWTToken { get; set; }
		public string CipheredCardInfo { get; set; }
	}
}
