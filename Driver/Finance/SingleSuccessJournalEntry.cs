using JV.Lib.FinancialManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.ServiceModel;

namespace Driver.Finance
{
    [PocTask(Description = "Send 1 successful Journal Entry transaction into Workday", Line = 39)]
    public class SingleSuccessJournalEntry : IPocTask
    {
        readonly Financial_ManagementPortClient _fin;
        readonly ILogger _logger;

        public SingleSuccessJournalEntry(
            Financial_ManagementPortClient fin,
            ILogger logger)
        {
            _fin = fin;
            _logger = logger;
        }

        public async Task<PocResult> Execute(CancellationToken cancellationToken = default)
        {
            var res = await _fin.Submit_Accounting_JournalAsync(
                    new JV.Lib.FinancialManagement.Workday_Common_HeaderType(),
                    new Submit_Accounting_Journal_RequestType
                    {
                        version = "v35.0",
                        Add_Only = true,
                        Accounting_Journal_Data = new Accounting_Journal_DataType
                        {
                            Disable_Optional_Worktag_Balancing = true,
                            Company_Reference = new JV.Lib.FinancialManagement.CompanyObjectType
                            {
                                ID = new JV.Lib.FinancialManagement.CompanyObjectIDType[]
                                {
                                    new JV.Lib.FinancialManagement.CompanyObjectIDType
                                    {
                                        Value = "UW1861",
                                        type = "Company_Reference_ID"
                                    }
                                }
                            },
                            Currency_Reference = new JV.Lib.FinancialManagement.CurrencyObjectType
                            {
                                ID = new JV.Lib.FinancialManagement.CurrencyObjectIDType[]
                                {
                                    new JV.Lib.FinancialManagement.CurrencyObjectIDType
                                    {
                                        Value = "USD",
                                        type = "Currency_ID"
                                    }
                                }
                            },
                            Accounting_Date = DateTime.Now.Date,
                            Journal_Source_Reference = new JV.Lib.FinancialManagement.Journal_SourceObjectType
                            {
                                ID = new JV.Lib.FinancialManagement.Journal_SourceObjectIDType[]
                                {
                                    new JV.Lib.FinancialManagement.Journal_SourceObjectIDType
                                    {
                                        type = "Journal_Source_ID",
                                        Value = "JOURNAL_SOURCE-6-79"
                                    }
                                }
                            },
                            Ledger_Type_Reference = new JV.Lib.FinancialManagement.Ledger_TypeObjectType
                            {
                                ID = new JV.Lib.FinancialManagement.Ledger_TypeObjectIDType[]
                                {
                                    new JV.Lib.FinancialManagement.Ledger_TypeObjectIDType
                                    {
                                        type = "Ledger_Type_ID",
                                        Value = "Actuals"
                                    }
                                }
                            }
                        }
                    });

            _logger.LogInformation("{Response}", Xml.Serialize(res));

            return new PocResult
            {
                Pass = true
            };
        }
    }
}
