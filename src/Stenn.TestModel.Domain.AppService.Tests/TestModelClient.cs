﻿using Stenn.AppData;
using Stenn.AppData.Client;
using System.Net.Http;

namespace Stenn.TestModel.Domain.AppService.Tests
{
    internal class TestModelClient : AppDataServiceClient<ITestModelEntity>
    {
        public TestModelClient(HttpClient httpClient, ExpressionTreeValidator<ITestModelEntity> expressionTreeValidator = null) : base(httpClient, expressionTreeValidator)
        {
        }
    }
}
