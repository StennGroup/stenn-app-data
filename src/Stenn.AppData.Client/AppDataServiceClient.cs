﻿using Stenn.AppData.Contracts;
using System.Linq.Expressions;
using System.Text.Json;
using Stenn.AppData.Expressions;

#nullable disable

namespace Stenn.AppData.Client
{
    public sealed class AppDataServiceClient<TBaseEntity> : IAppDataServiceClient<TBaseEntity>
        where TBaseEntity : class, IAppDataEntity
    {
        private readonly HttpClient _httpClient;
        private readonly ExpressionTreeValidator<TBaseEntity> _expressionValidator;
        private readonly IAppDataSerializer _serializer;

        public AppDataServiceClient(HttpClient httpClient, IAppDataSerializerFactory appDataSerializerFactory, ExpressionTreeValidator<TBaseEntity> expressionValidator = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _expressionValidator = expressionValidator ?? new ExpressionTreeValidator<TBaseEntity>();
            _serializer = appDataSerializerFactory.GetSerializer(); // TODO get serializer name from config and pass here
        }

        public TResult Execute<TResult>(Expression expression)
        {
            // заменяем в выражении обращение к типу Query<T> на вызов метода Query<T> на сервисе
            //var param = Expression.Parameter(_service.GetType(), "service");
            var param = Expression.Parameter(typeof(IAppDataService<TBaseEntity>), "service");
            var visitor = new ExpressionTreeModifier(param);
            var newExpression = visitor.Visit(expression);

            // строим лямбду которая будет принимать экземпляр сервиса
            var lambdaExpression = Expression.Lambda(newExpression, param);

            // валидируем полученное выражение
            lambdaExpression = (LambdaExpression)ValidateExpression(lambdaExpression);

            var serializer = new ExpressionSerializer();
            var slimExpression = serializer.Lift(lambdaExpression);
            var bonsai = serializer.Serialize(slimExpression);

            // передаем сериализованное выражение в метод сервиса который умеет его исполнять и получаем массив байт сериализованных данных
            var bytes = ExecuteRemote(bonsai);
            var result = Deserialize<TResult>(bytes);

            return result!;
        }

        private T Deserialize<T>(byte[] bytes)
        {
            return _serializer.Deserialize<T>(bytes);
        }

        private byte[] ExecuteRemote(string serializedExpression)
        {
            // no requestUri passed. httpClient's base address should already be configured to use correct uri
            var response = _httpClient.PostAsync((string)null, new StringContent(serializedExpression)).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsByteArrayAsync().Result;
        }

        private Expression ValidateExpression(Expression expression)
        {
            return _expressionValidator.Visit(expression);
        }
    }
}