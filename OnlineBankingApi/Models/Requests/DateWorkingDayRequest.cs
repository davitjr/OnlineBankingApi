using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [DateWorkingDayRequestValidation]
    public class DateWorkingDayRequest
    {
        public DateTime DateWorkingDay { get; set; }
    }
}
