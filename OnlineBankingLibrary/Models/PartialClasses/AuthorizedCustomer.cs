using System;
using System.Collections.Generic;
using System.Text;

namespace XBS
{
    public partial class AuthorizedCustomer
    {
        /// <summary>
        /// Օգտագործողի ունիկալ համար
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// true` եթե օգտագործողը վերջին հաստատման իրավունք ունեցող է, false՝ հակառակ դեպքում
        /// </summary>
        public bool IsLastConfirmer { get; set; }

        /// <summary>
        /// Եթե հաճախորդն աշխատակից է True հ․դ․ False
        /// </summary>
        public bool IsEmployee { get; set; }
    }
}


namespace XBSInfo
{
    public partial class AuthorizedCustomer
    {
        /// <summary>
        /// Օգտագործողի ունիկալ համար
        /// </summary>
        public int UserId { get; set; }

        // <summary>
        /// true` եթե օգտագործողը վերջին հաստատման իրավունք ունեցող է, false՝ հակառակ դեպքում
        /// </summary>
        public bool IsLastConfirmer { get; set; }
    }
}

namespace XBManagement
{
    public partial class AuthorizedCustomer
    {
        /// <summary>
        /// Օգտագործողի ունիկալ համար
        /// </summary>
        public int UserId { get; set; }

        // <summary>
        /// true` եթե օգտագործողը վերջին հաստատման իրավունք ունեցող է, false՝ հակառակ դեպքում
        /// </summary>
        public bool IsLastConfirmer { get; set; }
    }
}
