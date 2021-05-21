using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models
{
    /// <summary>
    /// Արագ օվերդրաֆտի սկիզբ և վերջ հարցման պատասխան
    /// </summary>
    public class FastOverdraftDates
    {
        /// <summary>
        /// Սկիզբ
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Վերջ
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
