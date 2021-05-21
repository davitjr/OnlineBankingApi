using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Models;
using OnlineBankingLibrary.Services;
using OnlineBankingLibrary.Utilities;
using System.Linq;
using System.Threading.Tasks;
using XBS;
using static OnlineBankingApi.Enumerations;
using ResponseExtensions = OnlineBankingApi.Models.ResponseExtensions;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class HomePageController : ControllerBase
    {
        private readonly XBService _xbService;
        private readonly CacheHelper _cacheHelper;

        public HomePageController(XBService xBService, CacheHelper cacheHelper)
        {
            _xbService = xBService;
            _cacheHelper = cacheHelper;
        }

        /// <summary>
        /// Վերադարձնում է mobile/web համակարգերում մուտքից հետո բացվող պատուհանի համար անհրաժեշտ տվյալները 
        /// (Հաշիվներ, Ավանդներ,Վարկեր, Քարտեր)
        /// Mobile-ի դեպքում՝ ամեն պրոդուկտից 1-ական
        /// Web-ի դեպքում՝ ամեն պրոդուկտից 2-ական
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetIBankingHomePage")]
        public IActionResult GetIBankingHomePage()
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<IBankingHomePage>() { ResultCode = ResultCodes.normal };
                var result = _xbService.GetIBankingHomePage();
                SourceType sourceType = _cacheHelper.GetSourceType();
                int productsCount = 0;
                if (sourceType == SourceType.MobileBanking)
                    productsCount = 1;


                if (result.Loans.Content != null && result.Loans.Content.Count > 0)
                {
                    result.Loans.Content.RemoveAll(m => m.Quality == 10 && !m.Is_24_7);
                }


                if (result.Accounts.Content != null && result.Accounts.Content.Count > 0)
                {
                    Parallel.ForEach(result.Accounts.Content, x => {
                        x.DigitalAvailabelBanlanaceAMD = _xbService.GetLastExchangeRate(x.Currency, 2, 2) * x.AvailableBalance;
                    });
                    if (sourceType != SourceType.MobileBanking)
                    {
                        productsCount = result.Accounts.Content.Count;
                    }
                    result.Accounts.Content = result.Accounts.Content.OrderByDescending(x => x.DigitalAvailabelBanlanaceAMD).Take(productsCount).ToList();

                }

                if (result.Cards.Content != null && result.Cards.Content.Count > 0)
                {
                    result.Cards.Content.RemoveAll(x => x.SupplementaryType != SupplementaryType.Main);
                    foreach (var item in result.Cards.Content)
                    {
                        item.ArCaBalance = _xbService.GetArcaBalance(item.CardNumber);
                        item.CardAccount.ArcaBalance = item.ArCaBalance;                      
                    }


                    Parallel.ForEach(result.Cards.Content, x =>
                    {
                        if (x.ArCaBalance.HasValue)
                        {
                            x.DigitalAvailabelBanlanaceAMD = _xbService.GetLastExchangeRate(x.Currency, 2, 2) * x.ArCaBalance.Value;
                        }
                        else
                        {
                            x.DigitalAvailabelBanlanaceAMD = null;
                        }

                    });

                    if (sourceType != SourceType.MobileBanking)
                    {
                        productsCount = result.Cards.Content.Count;
                    }
                    result.Cards.Content = result.Cards.Content.OrderByDescending(x => x.DigitalAvailabelBanlanaceAMD).Take(productsCount).ToList();
                }

                if (result.Deposits.Content != null && result.Deposits.Content.Count > 0)
                {
                    Parallel.ForEach(result.Deposits.Content, x => {
                        x.DigitalAvailabelBanlanaceAMD = _xbService.GetLastExchangeRate(x.Currency, 2, 2) * x.Balance;
                    });
                    if (sourceType != SourceType.MobileBanking)
                    {
                        productsCount = result.Deposits.Content.Count;
                    }
                    result.Deposits.Content = result.Deposits.Content.OrderByDescending(x => x.DigitalAvailabelBanlanaceAMD).Take(productsCount).ToList();
                }

                if (result.Loans.Content != null && result.Loans.Content.Count > 0)
                {
                    foreach (Loan loan in result.Loans.Content)
                    {
                        if (loan.ContractDate != null)
                        {
                            loan.StartDate = loan.ContractDate ?? loan.StartDate;
                        }

                        if (loan.Is_24_7)
                        {
                            loan.CurrentCapital = loan.ContractAmount;
                        }
                    }
                    if (sourceType != SourceType.MobileBanking)
                    {
                        productsCount = result.Loans.Content.Count;
                    }
                    result.Loans.Content = result.Loans.Content.OrderByDescending(x => x.NextRepayment.RepaymentDate).Take(productsCount).ToList();
                }
                response.Result = result;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

    }
}