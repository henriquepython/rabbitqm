using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using rabbitqm.Domain;

namespace rabbitqm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult InsertOrder(Order order)
        {
            try
            {
                //rabbitMQ producer
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    //alterar queue para nome da fila ex "orderQueue"
                    channel.QueueDeclare(queue: "orderQueue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    //pega o pedido e converte ele no formato texto como mensagem
                    string message = JsonSerializer.Serialize(order);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "orderQueue",//colocar o mesmo nome da queue
                                         basicProperties: null,
                                         body: body);
                }
                //retorna Accepted 202
                return Accepted(order);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentar criar um novo pedido", ex);

                return new StatusCodeResult(500);
            }
        }
    }
}
