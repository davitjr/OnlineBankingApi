using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineBankingLibrary.Models
{
   public class JointCustomer
    {
        /// <summary>
        /// 3-րդ անձի հաճախորդի համար
        /// </summary>
        public ulong CustomerNumber { get; set; }

        /// <summary>
        /// Մասնաբաժին
        /// </summary>
        public double Part { get; set; }

        /// <summary>
        /// Անուն, ազգանուն
        /// </summary>
        public string FullName { get; set; }
    }
}
