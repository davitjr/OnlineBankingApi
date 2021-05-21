using OnlineBankingApi.Filters;


namespace OnlineBankingApi.Models.Requests
{
    [ChangeUserPasswordRequestValidation]
    public class ChangeUserPasswordRequest
    {
        public string Password { set; get; }
        public string NewPassword { set; get; }
        public string RetypeNewPassword { set; get; }
    }
}
