using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingApi.Filters;
using OnlineBankingApi.Models;
using OnlineBankingApi.Models.Requests;
using OnlineBankingApi.Utilities;
using OnlineBankingLibrary.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OnlineBankingLibrary.Utilities;
using XBS;
using XBSInfo;
using static OnlineBankingApi.Enumerations;
using System.Linq;

namespace OnlineBankingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TypeFilter(typeof(AuthorizationFilter))]
    public class ContractController : ControllerBase
    {
        private readonly ContractManager _contractManager;
        private readonly ReportService _reportService;
        private readonly ReportManager _reportManager;
        private readonly CacheHelper _cacheHelper;
        private readonly XBService _xbService;

        public ContractController(ReportService reportService, ContractManager contractManager, ReportManager reportManager, CacheHelper cacheHelper, XBService xBService)
        {
            _reportService = reportService;
            _contractManager = contractManager;
            _reportManager = reportManager;
            _cacheHelper = cacheHelper;
            _xbService = xBService;
        }



        /// <summary>
        /// Դրամական միջոցների գրավի պայմանագիր
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="productType">վարկային գիծ Type = 2 , վարկ type = 1</param>
        /// <param name="fromApprove"></param>
        /// <returns></returns>
        [HttpPost("GetLoansDramContract")]
        public IActionResult GetLoansDramContract(LoanContractRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.ResultCode = ResultCodes.normal;
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xbService.GetLoansDramContract(request.DocId, request.ProductType, request.FromApprove, authorizedCustomer.CustomerNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Արագ օվերդրաֆտի պայմանագիր (PDF)
        /// </summary>
        /// <returns></returns>
        [HttpPost("PrintFastOverdraftContract")]
        public IActionResult PrintFastOverdraftContract(PrintContractRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.ResultCode = ResultCodes.normal;
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _contractManager.PrintFastOverdraftContract(request.Id, authorizedCustomer.CustomerNumber, false);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ավանդի գրավով վարկի պայմանագրի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("PrintDepositLoanContract")]
        public IActionResult PrintDepositLoanContract(PrintContractRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _xbService.PrintDepositLoanContract(request.Id, authorizedCustomer.CustomerNumber, false);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ավանդի գրավով վարկային գծի պայմանագրի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("PrintDepositCreditLineContract")]
        public IActionResult PrintDepositCreditLineContract(PrintContractRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                XBS.AuthorizedCustomer authorizedCustomer = _cacheHelper.GetAuthorizedCustomer();
                response.Result = _contractManager.PrintDepositCreditLineContract(request.Id, authorizedCustomer.CustomerNumber, false);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է ԱՔՌԱ համաձայնագրի տեքստը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetACRAAgreementText")]
        public IActionResult GetACRAAgreementText()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = "<!DOCTYPE html> <html> <body> «Սույնով տալիս եմ իմ համաձայնությունը, որ.<br />•&emsp;" +
                    "«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից հարցում կատարվի «ԱՔՌԱ Քրեդիտ Ռեփորթինգ» ՓԲԸ - ին և վերջինիս խնդրում եմ «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ին " +
                    "տրամադրել իմ ներկա և անցյալ ֆինանսական պարտավորությունների վերաբերյալ տեղեկություններ, ինչպես նաև այլ տվյալներ, որոնք «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի " +
                    "կողմից կարող են հաշվի առնվել ինձ հետ վարկային (փոխառության և այլն) պայմանագիր կնքելու վերաբերյալ որոշում կայացնելիս: <br />•&emsp;«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի " +
                    "կողմից վարկային (փոխառության և այլն) պայմանագիր կնքելու դեպքում տվյալ վարկային(փոխառության և այլն) պայմանագրի գործողության ողջ ընթացքում ցանկացած պահի առանց ինձ " +
                    "նախապես տեղյակ պահելու «ԱՔՌԱ Քրեդիտ Ռեփորթինգ» ՓԲԸ - ն «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ին տրամադրի իմ ապագա ֆինանսական պարտավորությունների վերաբերյալ " +
                    "տեղեկություններ, ինչպես նաև այլ տվյալներ: <br />•&emsp;«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից սեփականության կամ ընդհանուր սեփականության իրավունքով ինձ " +
                    "պատկանող գույքերի վերաբերյալ հարցում կատարի ՀՀ ԿԱ Անշարժ գույքի կադաստրի պետական կոմիտե և ստանա սպառիչ տեղեկատվություն իմ գույքային իրավունքների, այդ թվում` " +
                    "դրանց ծագման հիմքերի վերաբերյալ, ինչպես նաև կադաստրային գործից ստանա սեփականության իրավունքի վկայականի, հատակագծի և այլ անհրաժեշտ փաստաթղթերի պատճենները` մինչև " +
                    "իմ վարկային(փոխառության և այլն) պարտավորության լրիվ կատարումը: <br />•&emsp;«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ն սեփականության կամ ընդհանուր սեփականության իրավունքով ինձ " +
                    "պատկանող տրանսպորտային միջոցների վերաբերյալ հարցում կատարի ՀՀ Ճանապարհային ոստիկանություն և ստանա սպառիչ տեղեկատվություն տրանսպորտային միջոցների նկատմամբ իմ գույքային " +
                    "իրավունքների, այդ թվում` դրանց ծագման հիմքերի վերաբերյալ, ինչպես նաև ստանա սեփականության իրավունքի վկայականի և այլ անհրաժեշտ փաստաթղթերի պատճենները` մինչև իմ " +
                    "վարկային(փոխառության և այլն) պարտավորության լրիվ կատարումը: <br />•&emsp;«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ն հարցում կատարի «Հայաստանի ավտոապահովագրողների բյուրո» " +
                    "ԻԱՄ - ին և ապահովագրական ընկերություններին և ստանա ցանկացած տեղեկատվություն ինձ սեփականության իրավունքով պատկանող տրանսպորտային միջոցների ապահովագրության " +
                    "վերաբերյալ(այդ թվում՝ ապահովադրի և շահառուների վերաբերյալ տեղեկություններ)` մինչև իմ վարկային(փոխառության և այլն) պարտավորության լրիվ կատարումը: <br />•&emsp;" +
                    "«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ն հարցում կատարի «Նորք» սոցիալական ծառայությունների տեխնոլոգիական և իրազեկման կենտրոն հիմնադրամ, և խնդրում եմ վերջինիս տրամադրել " +
                    "իմ վերաբերյալ ցանկացած տեղեկատվություն, որը կարող է կիրառվել Բանկի կողմից: <br />•&emsp;«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից ինձ ուղարկվեն ծանուցումներ՝ " +
                    "«ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից մատուցվող ծառայությունների հետ կապված ցանկացած տեղեկատվության վերաբերյալ:<br />•&emsp;Քարտային հաշվի դրական մնացորդի " +
                    "բացակայության դեպքում «Արագ օվերդրաֆտ»-ի միջնորդավճարը գանձվի «Արագ օվերդրաֆտ»-ի գումարից: <br />•&emsp;Քարտային հաշվին «Արագ օվերդրաֆտ»-ի ակտիվացման դեպքում ՀՀ " +
                    "օրենսդրությամբ սահմանված պարտադիր ներկայացման ենթակա տեղեկատվությունը տրամադրվի դեբետային / կրեդիտային քարտի դիմումով սահմանված եղանակով: <br />Գիտակցում եմ, " +
                    "որ տրամադրված տեղեկությունները և տվյալները, կախված դրանց բովանդակությունից, կարող են ազդել «ԱԿԲԱ - ԿՐԵԴԻՏ ԱԳՐԻԿՈԼ ԲԱՆԿ» ՓԲԸ - ի կողմից կայացված համապատասխան " +
                    "որոշման վրա:Սույն համաձայնությունը կարդացել եմ և հավաստում եմ, որ այն ինձ համար ամբողջությամբ հասկանալի և ընդունելի է:» </body> </html> ";
                //կիսատ Language պետք է ավելացնել
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpGet("getpdfreport")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPDFReport()
        {
            if (ModelState.IsValid)
            {
                string reportName = "/ACBAReports/CardAccountDetails";
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add(key: "cardNumber", value: "4355053923943742");
                string languageCode = "en-us";

                byte[] reportContent = await _reportService.RenderReport(reportName, parameters, ExportFormat.PDF, "Test");

                Stream stream = new MemoryStream(reportContent);

                return new FileStreamResult(stream, "application/pdf");
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }

        /// <summary>
        /// Վերադարձնում է քարտի քաղվածքը pdf/excel ֆորմատով
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="exportFormat"></param>
        /// <returns></returns>
        [HttpPost("PrintCardStatement")]
        public async Task<IActionResult> PrintCardStatement(PrintCardStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PrintCardStatement(request.CardNumber, request.DateFrom, request.DateTo, request.ExportFormat);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է կոմունալ վճարման հանձնարարականը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost("PrintUtilitylOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public async Task<IActionResult> PrintUtilitylOrder(OrderIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PrintUtilitylOrder(request.OrderId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է միջազգային փոխանցման վճարման հանձնարարականը
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost("PrintInternationalOrder")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public async Task<IActionResult> PrintInternationalOrder(OrderIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PrintInternationalOrder(request.OrderId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է մեմորիալ օրդերը
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        [HttpPost("PrintMemorialOrder")]
        public async Task<IActionResult> PrintMemorialOrder(AccountNumberAndDateRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PrintMemorialOrder(request.AccountNumber, request.DateFrom, request.DateTo);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ավանդի քաղվածքի ստացում pdf/excel ֆորմատներով
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="accountNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="averageRest"></param>
        /// <param name="currencyRegulation"></param>
        /// <param name="payerData"></param>
        /// <param name="additionalInformationByCB"></param>
        /// <param name="includingExchangeRate"></param>
        /// <param name="exportFormat"></param>
        /// <returns></returns>
        [HttpPost("PrintDepositAccountStatement")]
        public async Task<IActionResult> PrintDepositAccountStatement(PrintDepositAccStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PrintDepositAccountStatement(request.ProductId, request.AccountNumber, request.DateFrom, request.DateTo,
                        request.IncludingExchangeRate, request.ExportFormat);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
        /// <summary>
        /// Վերադարձնում է էական պայմանների անհատական թերթիկը՝ base64 ֆորմատով։
        /// </summary>
        /// <returns></returns>
        [HttpPost("PrintLoanTermsSheetBase64")]
        public IActionResult PrintLoanTermsSheetBase64(PrintLoanTermSheetRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _contractManager.PrintLoanTermsSheetBase64(request.LoanType, request.Orderid);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Վերադարձնում է ընթացիկ հաշվի քաղվածքը PDF/Excel ֆորմատներով
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="includingExchangeRate"></param>
        /// <param name="exportFormat"></param>
        /// <returns></returns>
        [HttpPost("PrintCurrentAccountStatement")]
        public async Task<IActionResult> PrintCurrentAccountStatement(PrintCurrentAccStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PrintCurrentAccountStatement(request.AccountNumber, request.DateFrom, request.DateTo,
                        request.IncludingExchangeRate, request.ExportFormat);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտային հաշվի վավերապայմանները PDF-ով
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        [HttpPost("PrintCardSwiftDetails")]
        public async Task<IActionResult> PrintCardSwiftDetails(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PrintCardSwiftDetails(request.AccountNumber);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Փոխանցում սեփական հաշիվների միջև,ՀՀ տարածքում, բյուջե վճարման հանձնարակականի ստացում pdf/excel ֆորմատներով
        /// </summary>
        /// <param name="id"></param>
        /// <param name="exportFormat"></param>
        /// <returns></returns>
        [HttpPost("PrintTransfersAcbaStatement")]
        public async Task<IActionResult> PrintTransfersAcbaStatement(PrintTransfersAcbaStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                int lang = _cacheHelper.GetLanguage();
                SingleResponse<byte[]> response = await _reportManager.PrintTransfersAcbaStatement(request.Id, lang, request.ExportFormat);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Փոխանցում ռեեստրով ցանկով ստացում pdf/excel ֆորմատներով
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("PrintTransfersReestrByPage")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public async Task<IActionResult> PrintTransfersReestrByPage(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.TransfersReestrByPageStatement(request.Id);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }


        /// <summary>
        /// Փոխանցում ռեեստրով ստացում pdf/excel ֆորմատներով
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("PrintTransfersReestr")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public async Task<IActionResult> PrintTransfersReestr(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<Byte[]> response = await _reportManager.TransfersReestrStatement(request.Id);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ընթացիկ հաշվի պայմանագրի ստացում՝ նախքան հայտի ուղարկումը
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("GetCurrentAccountContractBefore")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCurrentAccountContractBefore(DocIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _contractManager.GetCurrentAccountContractBefore(request.DocId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է քարտից քարտ փոխանցման անդորրագիրը
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetCardToCardReceipt")]
        [TypeFilter(typeof(ValidateDocIdFilter))]
        public IActionResult GetCardToCardReceipt(IdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _contractManager.GetCardToCardReceipt(request.Id);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Սպառողական վարկի անհատական թերթիկ (PDF)
        /// </summary>
        /// <returns></returns>
        [HttpPost("PrintLoanTermsSheet")]
        public IActionResult PrintLoanTermsSheet(PrintLoanTermSheetRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _contractManager.PrintLoanTermsSheet(request.LoanType, request.Orderid);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Պարբերականի տպում
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("PeriodicTransfer")]
        public async Task<IActionResult> PeriodicTransfer(AppIdRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = await _reportManager.PeriodicTransfer(request.AppId);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է վարկի քաղվածքը pdf/excel ֆորմատով
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("PrintLoanStatement")]
        public async Task<IActionResult> PrintLoanStatement(PrintLoanStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                request.Language = _cacheHelper.GetLanguage();
                SingleResponse<byte[]> response = await _reportManager.PrintLoanStatement(request.ProductId, request.DateFrom, request.DateTo, request.Language, request.ExportFormat);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Ավանդի պայմանագրի ստացում ձևակերպված ավանդի համար
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("GetExistingDepositContract")]
        [TypeFilter(typeof(ValidateProductIdFilter), Arguments = new object[] { ProductType.Deposit })]
        public IActionResult GetExistingDepositContract(ProductIdRequest product)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _contractManager.GetExistingDepositContract(product.ProductId);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }


        /// <summary>
        /// վերադարձնում դրամական միջոցների գրավի պահպանված պայմանագիրը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetExistingLoansDramContract")]
        public IActionResult GetExistingLoansDramContract(AccountNumberRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<byte[]> response = new SingleResponse<byte[]>();
                response.Result = _xbService.GetExistingLoansDramContract(request.AccountNumber);
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }

        }

        /// <summary>
        /// Ավանդի գրավով վարկի/վարկային գծի պայմանագրի ստացում
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDepositLoanOrDepositCreditLineContract")]
        public IActionResult GetDepositLoanOrDepositCreditLineContract(LoanOrCreditLineContractRequest request)
        {
            if (ModelState.IsValid)
            {
                var response = new SingleResponse<byte[]>() { ResultCode = ResultCodes.normal };
                response.Result = _xbService.GetDepositLoanOrDepositCreditLineContract(request.LoanNumber, request.Type);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է վարկային գծի քաղվածքը
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("PrintCreditLineStatement")]
        public async Task<IActionResult> PrintCreditLineStatement(PrintLoanStatementRequest request)
        {
            if (ModelState.IsValid)
            {
                int langId = _cacheHelper.GetLanguage();
                SingleResponse<byte[]> response = await _reportManager.PrintCreditLineStatement(request.ProductId, request.DateFrom, request.DateTo, langId,request.ExportFormat);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("PrintCardAccountDetails")]
        public IActionResult PrintCardAccountDetails(CreditAccountDetailsRequest request)
        {
            if (ModelState.IsValid)
            {
                string cardNumber = _xbService.GetCardNumber(request.ProductId);
                SingleResponse<string> response = _reportManager.PrintCardAccountDetails(cardNumber, request.ExportFormat);
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        [HttpPost("PrintVisaVirtualCardCondition")]
        public IActionResult PrintVisaVirtualCardCondition()
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                response.Result = _contractManager.PrintVisaVirtualCardCondition();
                response.ResultCode = ResultCodes.normal;
                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }

        /// <summary>
        /// Վերադարձնում է առևտրի կետում քարտերով կատարված գործարքների քաղվածքը
        /// POS տերմինալի քաղվածք
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("PrintPOSStatement")]
        public async Task<IActionResult> PrintPOSStatement(POSStatementStatmentRequest request)
        {
            if (ModelState.IsValid)
            {
                SingleResponse<string> response = new SingleResponse<string>();
                if (request.ExportFormat.ToLower() == "xml")
                {
                    response.Result = await _xbService.GetStatement(request.AccountNumber, request.DateFrom, request.DateTo, request.ExportFormat);
                    response.ResultCode = ResultCodes.normal;
                }
                else
                {
                    short customerFilialCode = _xbService.GetCustomerFilial();
                    response = await _reportManager.PrintPOSStatement(request.AccountNumber, request.DateFrom, request.DateTo, request.ExportFormat, customerFilialCode);
                }

                return ResponseExtensions.ToHttpResponse(response);
            }
            else
            {
                return ValidationError.GetValidationErrorResponse(ModelState);
            }
        }
    }
}