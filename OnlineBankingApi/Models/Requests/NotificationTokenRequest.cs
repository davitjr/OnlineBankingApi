using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XBSecurity;

namespace OnlineBankingApi.Models.Requests
{
    public class NotificationTokenRequest
    {
        public PushNotificationService.NotificationToken Token { set; get; }

    }
}
