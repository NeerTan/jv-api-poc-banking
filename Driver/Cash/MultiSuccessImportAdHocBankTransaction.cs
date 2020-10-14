using JV.Lib.CashManagement;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Driver.Cash
{
    class MultiSuccessImportAdHocBankTransaction : IPocTask
    {
        readonly Cash_ManagementPortClient _cash;
        readonly ILogger _logger;

        public MultiSuccessImportAdHocBankTransaction(
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
                        Submit=false,
                        Transaction_Memo = "Neer_Transaction Description-Test",
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
                                    Value = "WELLS_FARGO",
                                    type = "Bank_Account_ID"
                                }
                            }
                        },                        
                        Transaction_Amount = 45.00M,
                        
                         Ad_hoc_Bank_Transaction_ID= "0004",
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
                        Remove_Bank_Account_Worktag_on_Offset="N",
                        Eliminate_FX_Gain_Loss="N",
                        Transaction_Line_Replacement_Data=new Ad_hoc_Bank_Transaction_Line__HV__DataType[]
                        {
                         new Ad_hoc_Bank_Transaction_Line__HV__DataType
                         {
                             Line_Order="ab",
                             Ledger_Account_Reference=new Ledger_AccountObjectType
                             {
                                ID= new JV.Lib.CashManagement.Ledger_AccountObjectIDType[]
                                {
                                    new Ledger_AccountObjectIDType
                                    {
                                        parent_id="UW_Finance",
                                        parent_type="Account_Set_ID",
                                        type="Ledger_Account_ID",
                                        Value="66300"
                                    }
                                }
                               
                             },
                             Worktags_Reference= new Accounting_WorktagObjectType[]
                             {

                                 new Accounting_WorktagObjectType
                                 {
                                     ID=new Accounting_WorktagObjectIDType[]
                                     {
                                        new Accounting_WorktagObjectIDType
                                        {
                                            type="Organization_Reference_ID",
                                           Value="FN100"
                                        }
                                     }
                                 },
                                 new Accounting_WorktagObjectType
                                 {
                                    ID=new Accounting_WorktagObjectIDType[]
                                    {
                                        new Accounting_WorktagObjectIDType
                                        {
                                            type="Cost_Center_Reference_ID",
                                            Value="039367"
                                        }
                                    }
                                 },
                                  new Accounting_WorktagObjectType
                                 {
                                    ID=new Accounting_WorktagObjectIDType[]
                                    {
                                        new Accounting_WorktagObjectIDType
                                        {
                                            type="Fund_ID",
                                            Value="FD103"
                                        }
                                    }
                                 }
                                
                                 
                             },
                            
                             Line_Amount=45.00M,
                             Line_Memo="My Test Memo",
                         }
                        
                        },
                       
                        
                        ItemElementName = ItemChoiceType3.Deposit,
                        External_Reference = "Neer-TEST-EXT",
                        Transaction_ID = "Neer-TEST-TID",
                       
                        Item = true
                    },
                }) ; 


            return new PocResult
            {
                Pass = true,
            };
        }
    }
}
