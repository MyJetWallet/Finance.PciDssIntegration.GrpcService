using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public class PaymentBridgeGrpcServiceManager : IPaymentBridgeGrpcServiceManager
    {
        private ILogger Logger => ServiceLocator.Logger;
        private readonly IPaymentBridgeGrpcServiceFactory _paymentBridgeGrpcServiceFactory;
        private readonly ISettingsModelProvider _settingsModelProvider;
        private static readonly object LockObject = new object();
        private static readonly object LockObjectForParsingProvidersUrl = new object();
        private string _paymentBridgeGrpcServiceMapping;

        private IDictionary<string, IPaymentBridgeGrpcService> _paymentBridgeServices =
            new Dictionary<string, IPaymentBridgeGrpcService>();

        private IReadOnlyDictionary<string, string> _providersUrlByName;

        public PaymentBridgeGrpcServiceManager(IPaymentBridgeGrpcServiceFactory paymentBridgeGrpcServiceFactory,
            ISettingsModelProvider settingsModelProvider)
        {
            _paymentBridgeGrpcServiceFactory = paymentBridgeGrpcServiceFactory;
            _settingsModelProvider = settingsModelProvider;
            Reload();
        }

        private SettingsModel SettingsModel => _settingsModelProvider.Get();

        public IPaymentBridgeGrpcService GetOrCreate(string paymentProviderName)
        {
            var providersUrlByName = GetProvidersUrlByName();
            lock (LockObject)
            {
                if (providersUrlByName.TryGetValue(paymentProviderName, out var serviceGrpcUrl))
                {
                    if (_paymentBridgeServices.TryGetValue(paymentProviderName, out var bridgeService))
                    {
                        if (!bridgeService.ServiceUrl.Equals(serviceGrpcUrl, StringComparison.OrdinalIgnoreCase))
                            bridgeService.UpdateUrl(serviceGrpcUrl);

                        return bridgeService;
                    }

                    bridgeService = _paymentBridgeGrpcServiceFactory.Create(paymentProviderName, serviceGrpcUrl);
                    _paymentBridgeServices.Add(paymentProviderName, bridgeService);

                    return bridgeService;
                }
            }

            return default;
        }

        public IReadOnlyCollection<IPaymentBridgeGrpcService> GetAll()
        {
            lock (LockObject)
            {
                return _paymentBridgeServices.Values.ToList();
            }
        }

        public void Reload()
        {
            var providersUrlByName = GetProvidersUrlByName();
            lock (LockObject)
            {
                var newPaymentBridgeServices = new Dictionary<string, IPaymentBridgeGrpcService>();
                foreach (var (name, serviceGrpcUrl) in providersUrlByName)
                {
                    var bridgeService = _paymentBridgeGrpcServiceFactory.Create(name, serviceGrpcUrl);
                    newPaymentBridgeServices.Add(name, bridgeService);
                }

                _paymentBridgeServices = newPaymentBridgeServices;
            }
        }

        private IReadOnlyDictionary<string, string> GetProvidersUrlByName()
        {
            lock (LockObjectForParsingProvidersUrl)
            {
                if (_providersUrlByName is null ||
                    !SettingsModel.PaymentBridgeGrpcServiceMapping.Equals(_paymentBridgeGrpcServiceMapping,
                        StringComparison.OrdinalIgnoreCase))
                {
                    var paymentBridgeGrpcServiceMapping = SettingsModel.PaymentBridgeGrpcServiceMapping;
                    try
                    {                        
                        var providersUrlByName =
                            paymentBridgeGrpcServiceMapping
                                .Split("|")
                                .Select(item => item.Split("@"))
                                .ToDictionary(item => item[0], item => item[1]);

                        _paymentBridgeGrpcServiceMapping = paymentBridgeGrpcServiceMapping;
                        _providersUrlByName = providersUrlByName;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "PaymentBridgeGrpcServiceManager. GetProvidersUrlByName failed with paymentBridgeGrpcServiceMapping {paymentBridgeGrpcServiceMapping}", paymentBridgeGrpcServiceMapping);
                    }
                    
                }

                return _providersUrlByName;
            }
        }
    }
}