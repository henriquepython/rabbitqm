using Consumer.Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;

namespace Consumer
{

    class Program
    {
        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "orderQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                //consumo
                var consumer = new EventingBasicConsumer(channel);
                
                //evento de recepção
                consumer.Received += (model, ea) =>
                {
                    try 
                    { 
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        //converter string em objeto de volta
                        var order = JsonSerializer.Deserialize<Order>(message);

                        Console.WriteLine($" [x] Order: {order.OrderNumber}|{order.ItemName}|{order.Price:N2}", message);

                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        //Logger,, Nack é não ok.
                        channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };
                channel.BasicConsume(queue: "orderQueue",
                                     autoAck: false, //colocar autoAck em false, pois se der problema no work,  voce consumiu, vc perde a mensagem
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}