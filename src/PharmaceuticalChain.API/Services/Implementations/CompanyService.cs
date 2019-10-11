﻿using Nethereum.Hex.HexTypes;
using PharmaceuticalChain.API.Models;
using PharmaceuticalChain.API.Repositories.Interfaces;
using PharmaceuticalChain.API.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaceuticalChain.API.Services.Implementations
{
    public class CompanyService : ICompanyService
    {
        private readonly IEthereumService ethereumService;

        private readonly IDrugTransactionService drugTransactionService;

        private readonly IReceiptRepository receiptRepository;

        private readonly ICompanyRepository companyRepository;

        public CompanyService(
            IEthereumService ethereumService,
            IDrugTransactionService drugTransactionService,
            IReceiptRepository receiptRepository,
            ICompanyRepository companyRepository)
        {
            this.ethereumService = ethereumService;
            this.drugTransactionService = drugTransactionService;
            this.receiptRepository = receiptRepository;
            this.companyRepository = companyRepository;
        }

        async Task<Guid> ICompanyService.Create(string name, string address, string phoneNumber, string taxCode, string BRCLink, string GPCLink)
        {
            var function = ethereumService.GetFunction(EthereumFunctions.AddChainPoint);
            try
            {
                Guid newCompanyId = companyRepository.CreateAndReturnId(new Models.Database.Tenant()
                {
                    Name = name,
                    Address = address,
                    PhoneNumber = phoneNumber,
                    TaxCode = taxCode,
                    BRCLink = BRCLink,
                    GPCLink = GPCLink
                });
                
                var result = await function.SendTransactionAsync(
                    ethereumService.GetEthereumAccount(),
                    new HexBigInteger(1000000),
                    new HexBigInteger(0),
                    functionInput: new object[] {
                        newCompanyId.ToString(),
                        name,
                        address,
                        phoneNumber,
                        taxCode,
                        BRCLink,
                        GPCLink
                    });

                return newCompanyId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async Task<string> ICompanyService.GetContractAddress(Guid id)
        {
            var function = ethereumService.GetFunction("getAddressByID");
            try
            {
                var result = await function.CallDecodingToDefaultAsync(
                   ethereumService.GetEthereumAccount(),
                   new HexBigInteger(600000),
                   new HexBigInteger(0),
                   functionInput: new object[]
                   {
                       id.ToString()
                   });
                return result[0].Result.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        async Task<List<CompanyInformation>> ICompanyService.GetInformationOfAllCompanies()
        {
            try
            {
                var getCompanyFunction = ethereumService.GetFunction("getCompany");
                var getTotalFunction = ethereumService.GetFunction("getTotalCompanies");

                var total = await ethereumService.CallFunction(getTotalFunction);
                var result = new List<CompanyInformation>();
                CompanyInformation companyInfo = null;
                for (int i = 0; i < total; i++)
                {
                    companyInfo = await getCompanyFunction.CallDeserializingToObjectAsync<CompanyInformation>(
                        "0xa5eE58Df60d9f6c2FE211D287926948292DffbD3",
                        new HexBigInteger(300000),
                        new HexBigInteger(0),
                        functionInput: new object[] { i }
                        );

                    //companyInfo.Receipts = receiptRepository.GetReceipts(companyInfo.CompanyId);

                    result.Add(companyInfo);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        async Task<List<DrugStorageInformation>> ICompanyService.GetStorageInformation(uint companyId)
        {
            List<StorageInformation> result = new List<StorageInformation>();

            var allTransactions = await drugTransactionService.GetInformationOfAllDrugTransactions();

            var lastTransactions = new List<DrugTransactionInformation>();
            foreach (var transaction in allTransactions)
            {
                if (transaction.ToCompanyId == companyId)
                {
                    lastTransactions.Add(transaction);
                }
            }

            // Find parent transactions all the way to the first
            var chainedTransactions = new List<DrugTransactionInformation>();
            chainedTransactions.AddRange(lastTransactions);
            foreach (var transaction in lastTransactions)
            {
                var iteratorTransaction = transaction;
                var transactionPool = allTransactions.Where(t => t.PackageId == iteratorTransaction.PackageId).ToList();
                bool parentMatchCondition(DrugTransactionInformation t) => t.ToCompanyId == iteratorTransaction.FromCompanyId;
                while (iteratorTransaction.HasParent(transactionPool, parentMatchCondition))
                {
                    iteratorTransaction = iteratorTransaction.Parent(transactionPool, parentMatchCondition);
                    chainedTransactions.Add(iteratorTransaction);
                }
            }

            var transactionsGroupedByDrugName = (from t in chainedTransactions
                                                 group t by new { t.DrugName, t.PackageId } into g
                                                 select new DrugStorageInformation(
                                                     g.Key.DrugName,
                                                     g.Key.PackageId,
                                                     g.Select(p => new ChainHistory(p.FromCompanyId, p.Amount)).Reverse().ToList()
                                                 )).ToList();

            var companies = await (this as ICompanyService).GetInformationOfAllCompanies();

            foreach (var transaction in transactionsGroupedByDrugName)
            {
                foreach (var item in transaction.ChainHistories)
                {
                    item.CompanyName = companies.Where(c => c.CompanyId == item.CompanyId).Single().Name;
                }

                // Add the last node which is the company is being the parent search.
                ChainHistory rootCompany = new ChainHistory(companyId, 0);
                rootCompany.CompanyName = companies.Where(c => c.CompanyId == rootCompany.CompanyId).Single().Name;
                transaction.ChainHistories.Add(rootCompany);
            }

            return transactionsGroupedByDrugName;
        }

        async Task<int> ICompanyService.GetTotalCompanies()
        {
            var function = ethereumService.GetFunction("getTotalCompanies");
            try
            {
                var result = await ethereumService.CallFunction(function);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
