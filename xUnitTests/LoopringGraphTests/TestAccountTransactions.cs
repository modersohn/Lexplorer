﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Lexplorer.Models;
using Lexplorer.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using xUnitTests.Utils;

namespace xUnitTests.LoopringGraphTests
{
    [Collection("LoopringGraphQL collection")]
    public class TestAccountTransactions
    {
        GraphQLTestsFixture fixture;
        LoopringGraphQLService service;

        public TestAccountTransactions(GraphQLTestsFixture fixture)
        {
            this.fixture = fixture;
            this.service = fixture!.LGS;
        }

        [Fact]
        public async void GetAccountTransactions()
        {

            var response = await service.GetAccountTransactionsResponse(0, 10, fixture.testAccountId);
            Assert.NotNull(response);
            JObject jresponse = JObject.Parse(response!);
            JToken token = jresponse["data"]!["account"]!["transactions"]!;

            //generte a .json file nested in 2 arrays, so it can be used with JsonFileData
            //and EnsureTransactionsDescend theory below
            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), "AccountTransactions.json");
            JArray arrayParamToken = new JArray();
            arrayParamToken.Add(token);
            JArray arrayTestsToken = new JArray();
            arrayTestsToken.Add(arrayParamToken);
            using (StreamWriter file = File.CreateText(path))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                arrayTestsToken.WriteTo(writer);
            }

            IList<Transaction>? transactions = service.GetTransactionsFromJToken(token!);
            Assert.NotEmpty(transactions);
        }

        [Theory]
        [JsonFileData("AccountTransactions.json")]
        public void EnsureTransactionsDescend(JArray transactionsJArray)
        {
            IList<Transaction>? transactions = service.GetTransactionsFromJToken(transactionsJArray);
            Assert.NotEmpty(transactions);
            for (int i = 0; i < transactions!.Count; i++)
            {
                EnsureTransactionDescends(transactions[i]);
            }
        }

        internal void EnsureTransactionDescends(Transaction? transaction)
        {
            Assert.NotNull(transaction);
            //all transactions should descend from Transaction, never be of exactly the same type
            Assert.IsNotType<Transaction>(transaction);
        }

    }
}
