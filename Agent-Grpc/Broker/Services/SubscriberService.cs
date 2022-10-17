using Broker.Models;
using Broker.Services.Interfaces;
using Grpc.Core;
using GrpcAgeny;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Broker.Services
{
    public class SubscriberService : Subscriber.SubscriberBase
    {
        private readonly IConnectionStorageService _connectionStorage;
        public SubscriberService(IConnectionStorageService connectionStorage)
        {
            _connectionStorage = connectionStorage;
        }
        public override Task<SubscribeReply> Subscribe(SubscribeRequest request, ServerCallContext context)
        {

            Console.WriteLine($"Новый подписчик пытается соединиться: {request.Address} {request.Topic}");
            try
            {
                var connection = new Connection(request.Address, request.Topic);
                _connectionStorage.Add(connection);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Соединение не добавлено: {request.Address} {request.Topic} {e.Message}");
            }

            
            /**
            var client = new Notifier.NotifierClient(connection.Channel);
            var reply = client.Notify(request);
            /**/
            return Task.FromResult(new SubscribeReply()
            {
                IsSuccess = true
            }); 
        }
    }
}
