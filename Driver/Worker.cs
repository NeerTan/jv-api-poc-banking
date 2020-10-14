using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml;
using JV.Lib.Integrations;
using JV.Lib.FinancialManagement;
using JV.Lib.CashManagement;
using JV.Lib.RevenueManagement;
using JV.Lib.ResourceManagement;
using System.ServiceModel;
using Driver.Finance;
using Driver.Cash;

namespace Driver
{
    public class Worker : BackgroundService
    {
        readonly IntegrationsPortClient _integrations;
        readonly Financial_ManagementPortClient _financial;
        readonly Cash_ManagementPortClient _cash;
        readonly Revenue_ManagementPortClient _revenue;
        readonly Resource_ManagementPortClient _resource;
        readonly ILogger<Worker> _logger;
        readonly IHostApplicationLifetime _host;

        public Worker(
            IntegrationsPortClient integrations,
            Financial_ManagementPortClient financial,
            Cash_ManagementPortClient cash,
            Revenue_ManagementPortClient revenue,
            Resource_ManagementPortClient resource,
            ILogger<Worker> logger,
            IHostApplicationLifetime host)
        {
            _integrations = integrations;
            _financial = financial;
            _cash = cash;
            _revenue = revenue;
            _resource = resource;
            _logger = logger;
            _host = host;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing...");

            try
            {
                var poc = new MultiSuccessImportAdHocBankTransaction(_cash, _logger);

                var pass = await poc.Execute(stoppingToken);

                _logger.LogInformation($"{poc.GetType()} passed: {pass.Message}");
            }
            catch (Exception e)
            {
                HandleException(e);
                if (e != null) { }
            }
            finally
            {
                _host.StopApplication();
            }
        }

        void HandleException(Exception e)
        {
            switch (e)
            {
                case FaultException<JV.Lib.CashManagement.Validation_ErrorType[]> f:
                    _logger.LogError(f, "{ValidationError}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.FinancialManagement.Validation_ErrorType[]> f:
                    _logger.LogError(f, "{ValidationError}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.Integrations.Validation_ErrorType[]> f:
                    _logger.LogError(f, "{ValidationError}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.ResourceManagement.Validation_ErrorType[]> f:
                    _logger.LogError(f, "{ValidationError}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.RevenueManagement.Validation_ErrorType[]> f:
                    _logger.LogError(f, "{ValidationError}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.CashManagement.Processing_FaultType> f:
                    _logger.LogError(f, "{ProcessingFault}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.FinancialManagement.Processing_FaultType> f:
                    _logger.LogError(f, "{ProcessingFault}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.Integrations.Processing_FaultType> f:
                    _logger.LogError(f, "{ProcessingFault}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.ResourceManagement.Processing_FaultType> f:
                    _logger.LogError(f, "{ProcessingFault}...", Xml.Serialize(f.Detail));
                    return;

                case FaultException<JV.Lib.RevenueManagement.Processing_FaultType> f:
                    _logger.LogError(f, "{ProcessingFault}...", Xml.Serialize(f.Detail));
                    return;

                default:
                    _logger.LogError(e, "{Unknown}...");
                    return;
            }
        }
    }
}
