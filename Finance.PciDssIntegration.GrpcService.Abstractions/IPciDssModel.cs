using System;

namespace Finance.PciDssIntegration.GrpcService.Abstractions
{
    public interface IPciDssInvoiceModel
    {
        string CardNumber { get; set; }
        string Cvv { get; set; }
        DateTime ExpirationDate { get; set; }
        string FullName { get; set; }
        double Amount { get; set; }
        string Zip { get; set; }
        string City { get; set; }
        string Country { get; set; }
        string Address { get; set; }
        string OrderId { get; set; }
        string Email { get; set; }
        string TraderId { get; set; }
        string AccountId { get; set; }
        string PaymentProvider { get; set; }
        string Currency { get; set; }
        string Ip { get; set; }
        double PsAmount { get; set; }
        string PsCurrency { get; set; }
        string BrandName { get; set; }
        string PhoneNumber { get; set; }
        string KycVerified { get; set; }
        double TotalDeposit { get; set; }
    }
}