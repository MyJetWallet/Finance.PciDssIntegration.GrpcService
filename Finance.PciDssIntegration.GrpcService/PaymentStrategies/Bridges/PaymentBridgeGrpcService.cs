using System;
using Finance.PciDss.PciDssBridgeGrpc;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public class PaymentBridgeGrpcService : IPaymentBridgeGrpcService
    {
        private Lazy<IFinancePciDssBridgeGrpcService> _financePciDssBridgeGrpcServiceLazy;

        private PaymentBridgeGrpcService(string name, string url)
        {
            Name = name;
            ServiceUrl = url;
            _financePciDssBridgeGrpcServiceLazy = new Lazy<IFinancePciDssBridgeGrpcService>(() => GrpcChannel
                .ForAddress(url)
                .CreateGrpcService<IFinancePciDssBridgeGrpcService>());
        }

        public string Name { get; }
        public string ServiceUrl { get; }

        public IFinancePciDssBridgeGrpcService FinancePciDssBridgeGrpcService =>
            _financePciDssBridgeGrpcServiceLazy.Value;

        public void UpdateUrl(string url)
        {
            _financePciDssBridgeGrpcServiceLazy = new Lazy<IFinancePciDssBridgeGrpcService>(() => GrpcChannel
                .ForAddress(url)
                .CreateGrpcService<IFinancePciDssBridgeGrpcService>());
        }

        public static IPaymentBridgeGrpcService Create(string name, string url)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(name);

            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(url);

            return new PaymentBridgeGrpcService(name, url);
        }

        public override string ToString()
        {
            return $"{nameof(PaymentBridgeGrpcService)}-{Name}:{ServiceUrl}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PaymentBridgeGrpcService) obj);
        }

        protected bool Equals(PaymentBridgeGrpcService other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}