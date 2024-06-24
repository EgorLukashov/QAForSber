using QAForSber.Data;
using QAForSber.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


//namespace QAForSber.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class AdminController : Controller
//    {

//        private readonly ApplicationDbContext _db;
//        public AdminController(ApplicationDbContext db)
//        {
//            _db = db;
//        }

//        [HttpGet("api/admins")]
//        public IActionResult Get()
//        {
//            IEnumerable<Admin> admins = _db.Admin.ToList();

//            return Ok(admins);
//        }


//    }
//}

//using QAForSber.Data;
//using QAForSber.Models;
//using Microsoft.AspNetCore.Mvc;
//using Confluent.Kafka;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace QAForSber.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class AdminController : ControllerBase
//    {
//        private readonly ApplicationDbContext _db;
//        private readonly IProducer<string, string> _producer;

//        public AdminController(ApplicationDbContext db, IConfiguration configuration)
//        {
//            _db = db;

//            var config = new ProducerConfig
//            {
//                BootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"]
//            };

//            _producer = new ProducerBuilder<string, string>(config).Build();
//        }

//        [HttpGet("api/admins")]
//        public IActionResult Get()
//        {
//            IEnumerable<Admin> admins = _db.Admin.ToList();

//            var message = JsonConvert.SerializeObject(admins);

//            var kafkaMessage = new Message<string, string>
//            {
//                Key = null,
//                Value = message
//            };

//            _producer.ProduceAsync("admin-topic", kafkaMessage);

//            return Ok(admins);
//        }


//    }
//}
using QAForSber.Data;
using QAForSber.Models;
using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QAForSber.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext db, IConfiguration configuration, ILogger<AdminController> logger)
        {
            _db = db;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _topic = configuration["Kafka:Topic"];
        }

        [HttpGet("api/admins")]
        public async Task<IActionResult> Get()
        {
            IEnumerable<Admin> admins = _db.Admin.ToList();

            var message = JsonConvert.SerializeObject(admins);

            var kafkaMessage = new Message<string, string>
            {
                Key = null,
                Value = message
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(_topic, kafkaMessage);
                _logger.LogInformation($"Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error producing message: {ex.Message}");
            }

            return Ok(admins);
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _producer?.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}

