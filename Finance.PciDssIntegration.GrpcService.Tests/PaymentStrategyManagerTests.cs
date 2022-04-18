using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies;
using MyDependencies;
using NSubstitute;
using NUnit.Framework;
using SimpleTrading.Deposit.Postgresql.Repositories;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class PaymentStrategyManagerTests
    {
        private readonly IServiceResolver _serviceResolver;

        public PaymentStrategyManagerTests()
        {
            _serviceResolver = Substitute.For<IServiceResolver>();
            _serviceResolver.GetService<DepositRepository>()
                .Returns(new DepositRepository(string.Empty, string.Empty, string.Empty));
            ServiceLocator.Init(_serviceResolver);
        }

        [Test]
        [Ignore("Cannot mock repository")]
        public Task MakeDepositAsync_ShouldReturnSuccessAndSaveToDbSuccess_WhenCascadingSuccess()
        {
            var paymentStrategyManager = new PaymentStrategyManager();

            var payment = paymentStrategyManager.MakeDepositAsync(new MakeDepositRequest());
            return Task.CompletedTask;
        }
    }
}
