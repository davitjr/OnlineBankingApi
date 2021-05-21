using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi
{
    public class Enumerations
    {
        public enum ResultCodes : ushort
        {
            normal = 1,
            failed = 2,
            notAuthorized = 3,
            validationError = 4,
            frontEndValidationError = 5
        }
    }
}
