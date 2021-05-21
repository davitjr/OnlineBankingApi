using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static OnlineBankingApi.Filters.CardStatementRequestValidation;

namespace OnlineBankingApi.Models.Requests
{
    [VehicleViolationByPsnVehRequestValidation]
    public class VehicleViolationByPsnVehRequest
    {
        [Required(ErrorMessage = "Հաշվառման վկայագիրը (տեխ. անձնագիր) լրացված չէ։")]
        public string Psn { set; get; }

        [Required(ErrorMessage = "Հաշվառման համարանիշը լրացված չէ։")]
        public string VehNum { set; get; }
    }
}
