using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using XBS;

namespace OnlineBankingApi.Models.Requests
{
    public class AttachCardRequest
    {
        /// <summary>
        /// Քարտի վրա նշված Անուն Ազգանուն
        /// </summary>
        [Required]
        public string CardHolderName { get; set; }
        /// <summary>
        /// Քարտի համար
        /// </summary>
        [Required]
        public ulong CardNumber { get; set; }
        /// <summary>
        /// Քարտի cvv
        /// </summary>
        [Required]
        public int Cvv { get; set; }
        /// <summary>
        /// Քարտի վավերականության ժամկետ
        /// </summary>
        [Required]
        public string ExpireMonth { get; set; }
        /// <summary>
        /// Քարտի վավերականության ժամկետ
        /// </summary>
        [Required]
        public string ExpireYear { get; set; }
    }
    public class AttachCardCompletionRequest
    {
        [Required]
        public string MdOrder { get; set; }
    }
    public class AttachCardDeleteRequest
    {
        [Required]
        public int Id { get; set; }
    }

    public class AttachCardBindingRequest
    {
        public AttachCardGenericPaymentOrder Order { get; set; }
        public class AttachCardGenericPaymentOrder
        {
            [Required]
            public string BindingId { get; set; }
            [Required]
            public OrderType Type { get; set; }
            [Required]
            public int Amount { get; set; }
            public int ID { get; set; }
            public ulong ProductId { get; set; }
            public long PoliceResponseDetailsId { get; set; }
            public short LTACode { get; set; }
            public byte SubType { get; set; }
            public string Currency { get; set; }
            public int TransferFee { get; set; }
            public string CreditCardNumber { get; set; }
            public string DebitCardNumber { get; set; }
            public string EmbossingName { get; set; }
            public int ReceiverBankCode { get; set; }
            public string Receiver { get; set; }
            public string Description { get; set; }
            public bool UseCreditLine { get; set; }
            public string Branch { get; set; }
            public ushort AbonentFilialCode { get; set; }
            public ushort PaymentType { get; set; }
            public int MatureMode { get; set; }
            public MatureType MatureType { get; set; }
            public CommunalTypes CommunalType { get; set; }
            public int AbonentType { get; set; }
            public string Code { get; set; }
            public AttachCardGenericPaymentOrderAccount Account { get; set; }
            public AttachCardGenericPaymentOrderAccount ReceiverAccount { get; set; }
            public AttachCardGenericPaymentOrderAccount DebitAccount { get; set; }
            public AttachCardGenericPaymentOrderAccount FeeAccount { get; set; }
            public class AttachCardGenericPaymentOrderAccount
            {
                public string AccountNumber { get; set; }

                public string Currency { get; set; }
            }

        }
    }
    public class AttachCardPaymentApprovalRequest
    {
        public long Id { get; set; }
        public string OTP { get; set; }
        public OrderType Type { get; set; }
        public byte SubType { get; set; }
    }
}
