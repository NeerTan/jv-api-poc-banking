using JV.Lib.CashManagement;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Driver.Cash
{
    class SingleSuccessImportAdHocBankTransaction : IPocTask
    {
        readonly Cash_ManagementPortClient _cash;
        readonly ILogger _logger;

        public SingleSuccessImportAdHocBankTransaction(
            Cash_ManagementPortClient cash,
            ILogger logger)
        {
            _cash = cash;
            _logger = logger;
        }

        public async Task<PocResult> Execute(CancellationToken cancellationToken = default)
        {
            var res = await _cash.Import_Ad_hoc_Bank_TransactionAsync(
                new Workday_Common_HeaderType(),
                new Import_Ad_hoc_Bank_Transaction_RequestType
                {
                    version = "v35.0",
                    Add_Only = true,
                    Business_Process_Parameters = new Financials_Business_Process_ParametersType
                    {
                        Auto_Complete = true,
                    },
                    Ad_hoc_Bank_Transaction_Data = new Ad_hoc_Bank_Transaction__HV__DataType
                    {
                        Transaction_Date = DateTime.Now.Date,
                        Transaction_Memo = "EWS Ad Hoc Transaction Import Request Test",
                        Company_Reference = new CompanyObjectType
                        {
                            ID = new JV.Lib.CashManagement.CompanyObjectIDType[]
                            {
                                new JV.Lib.CashManagement.CompanyObjectIDType
                                {
                                    Value = "UW1861",
                                    type = "Company_Reference_ID"
                                }
                            }
                        },
                        Currency_Reference = new JV.Lib.CashManagement.CurrencyObjectType
                        {
                            ID = new JV.Lib.CashManagement.CurrencyObjectIDType[]
                            {
                                new JV.Lib.CashManagement.CurrencyObjectIDType
                                {
                                    Value = "USD",
                                    type = "Currency_ID"
                                }
                            }
                        },                       
                        Bank_Account_Reference = new JV.Lib.CashManagement.Financial_AccountObjectType
                        {
                            ID = new JV.Lib.CashManagement.Financial_AccountObjectIDType[]
                            {
                                new JV.Lib.CashManagement.Financial_AccountObjectIDType
                                {
                                    Value = "UW_MAIN_BOFA_CONCENTRATION_ACCOUNT",
                                    type = "Bank_Account_ID"
                                }
                            }
                        },
                        Transaction_Amount = 13.15M,                       
                        Journal_Source_Reference = new JV.Lib.CashManagement.Journal_SourceObjectType
                        {
                            ID = new JV.Lib.CashManagement.Journal_SourceObjectIDType[]
                            {
                                new JV.Lib.CashManagement.Journal_SourceObjectIDType
                                {
                                    type = "Journal_Source_ID",
                                    Value = "JOURNAL_SOURCE-6-81"
                                }
                            }
                        },
                        ItemElementName = ItemChoiceType3.Deposit,
                        External_Reference = "BILLOLS-TEST-EXT",
                        Transaction_ID = "BILLOLS-TEST-TID",
                        Item = true
                    },
                });


            return new PocResult
            {
                Pass = true,
            };
        }
    }
}
