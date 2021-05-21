using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBankingApi.Models.Requests
{
    /// <summary>
    /// Պրոֆիլի նկարի պա
    /// </summary>
    public class SaveOnlineUserPhotoRequest
    {
        /// <summary>
        /// Նկարի base64
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// Ֆայլի տեսակ
        /// </summary>
        public string Extension { get; set; }



    }
}
