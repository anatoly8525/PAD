﻿using Broker.Services.Interfaces;
using Grpc.Core;
using GrpcAgeny;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Broker.Services
{
    public class SenderWorker : IHostedService
    {
        
        private Timer _timer;
        private const int TimeToWait = 2000;
        private readonly IMessageStorageService _messageStorage;
        private readonly IConnectionStorageService _connectionStorage;

        public SenderWorker(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _messageStorage = scope.ServiceProvider.GetRequiredService<IMessageStorageService>();
                _connectionStorage = scope.ServiceProvider.GetRequiredService<IConnectionStorageService>();
            }
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoSendWork, null, 0, TimeToWait);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;

        }
        private void DoSendWork(object state)
        {
            while (!_messageStorage.IsEmpty())
            {
                var message = _messageStorage.GetNext();

                if (message != null)
                {
                    Console.WriteLine($"На тему{message.Topic} подписано " +
                        $"{_connectionStorage.GetConnectionsByTopic(message.Topic).Count} человек");
                    int i = 0;
                    while (i==0)
                    {
                        /* if (_connectionStorage.GetConnectionsByTopic(message.Topic).Count == 0)
                         {
                             Console.WriteLine("Ждем подключения");
                             Thread.Sleep(5000);
                         }*/

                        switch (_connectionStorage.GetConnectionsByTopic(message.Topic).Count)
                        {
                            case 0:

                                break;

                            default:
                                

                            

                                Console.WriteLine($"На тему{message.Topic} подписано " +
                                                            $"{_connectionStorage.GetConnectionsByTopic(message.Topic).Count} человек");
                                var connections = _connectionStorage.GetConnectionsByTopic(message.Topic);
                                foreach (var connection in connections)
                                {
                                    var client = new Notifier.NotifierClient(connection.Channel);
                                    var request = new NotifyRequest() { Content = message.Content };

                                    try
                                    {
                                        var reply = client.Notify(request);
                                        // Console.WriteLine($"Подписчик уведомлен {connection.Address} o {message.Content}. Ответ: {reply.IsSuccess}");
                                    }
                                    catch (RpcException rpcException)
                                    {
                                        if (rpcException.StatusCode == StatusCode.Internal)
                                        {
                                            _connectionStorage.Remove(connection.Address);
                                        }
                                        Console.WriteLine($"Ошибка подписчика {connection.Address}. {rpcException.Message}");
                                    }
                                    catch (Exception exception)
                                    {
                                        Console.WriteLine($"Ошибка уведомления подписчика {connection.Address}. {exception.Message}");
                                    }

                                }
                                i++;
                                break;
                        }
                        
                    }
                }
            }
        }
    }
}
