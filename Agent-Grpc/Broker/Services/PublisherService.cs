using Broker.Models;
using Broker.Services.Interfaces;
using Grpc.Core;
using GrpcAgeny;
using System;
using System.Threading.Tasks;

namespace Broker.Services
{
    public class PublisherService : Publisher.PublisherBase
    {
        private readonly IMessageStorageService _messageStorage;
        private readonly IMessageStorageService _QStorage;
        public PublisherService(IMessageStorageService messageStorageService)
        {
            _messageStorage = messageStorageService;
            _QStorage = messageStorageService;
        }
        public override Task<PublishReply> PublishMessage(PublishRequest request,
            ServerCallContext context)
        {
            Console.WriteLine($"Получено: {request.Topic} {request.Content}");

            //var message = new Message(request.Topic, request.Content);
            _messageStorage.Add(new Message(request.Topic, request.Content));
            //_QStorage.Add(message);
            return Task.FromResult(new PublishReply()
            {
                IsSuccess = true
            });
        }
    }
}
