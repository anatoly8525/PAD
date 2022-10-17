using Grpc.Net.Client;
using System;
using Common;
using GrpcAgeny;
using System.Threading.Tasks;

namespace Sender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("This is PUBLISHER");

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(EndpointsConstants.BrokerAddress);
            var client = new Publisher.PublisherClient(channel);
            
            while (true)
            {
                Console.WriteLine("Введите тему: ");
                var topic = Console.ReadLine().ToLower();

                Console.WriteLine("Введите текст: ");
                var content = Console.ReadLine();
                var mem = new Q(topic, content);
                var request = new PublishRequest() { Topic = topic, Content = content };

                try 
                {
                    var reply = await client.PublishMessageAsync(request);
                    Console.WriteLine($"Ответ публикации: {reply.IsSuccess}");
                }
                catch(Exception e) 
                {
                    Console.WriteLine($"Ошибка публикации: {e.Message}");
                    
                }
            }
        }
    }
}
