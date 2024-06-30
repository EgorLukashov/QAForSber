using QAForSber.Data;
using QAForSber.Models;
using Microsoft.AspNetCore.Mvc;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using static QAForSber.Models.Admin;

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
            IEnumerable<Admin> admins = await _db.Admin.ToListAsync();

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

        [HttpGet("api/admins/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation($"Attempting to fetch admin with ID: {id}");

            var admin = await _db.Admin.FindAsync(id);
            if (admin == null)
            {
                _logger.LogWarning($"Admin with ID: {id} not found.");
                return NotFound();
            }

            _logger.LogInformation($"Admin with ID: {id} found. Preparing message for Kafka.");

            var kafkaMessage = new KafkaMessage<Admin>
            {
                Operation = "GETBYID",
                Data = admin
            };

            var message = JsonConvert.SerializeObject(kafkaMessage);
            var kafkaMsg = new Message<string, string>
            {
                Key = null,
                Value = message
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(_topic, kafkaMsg);
                _logger.LogInformation($"Kafka message delivered. Value: '{deliveryResult.Value}', TopicPartitionOffset: '{deliveryResult.TopicPartitionOffset}'");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"Kafka ProduceException: {ex.Error.Reason}");
                return StatusCode(500, "Error producing message to Kafka.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }

            return Ok(admin);
        }



        [HttpPost("api/admins")]
        public async Task<IActionResult> Create(Admin admin)
        {
            _logger.LogInformation($"Creating new admin with login: {admin.AdminLogin}");

            _db.Admin.Add(admin);
            await _db.SaveChangesAsync();

            var kafkaMessage = new KafkaMessage<Admin>
            {
                Operation = "CREATE",
                Data = admin
            };

            var message = JsonConvert.SerializeObject(kafkaMessage);
            var kafkaMsg = new Message<string, string>
            {
                Key = null,
                Value = message
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(_topic, kafkaMsg);
                _logger.LogInformation($"Kafka message delivered. Value: '{deliveryResult.Value}', TopicPartitionOffset: '{deliveryResult.TopicPartitionOffset}'");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"Kafka ProduceException: {ex.Error.Reason}");
                return StatusCode(500, "Error producing message to Kafka.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }

            return CreatedAtAction(nameof(GetById), new { id = admin.IDAdmin }, admin);
        }


        [HttpPut("api/admins/{id}")]
        public async Task<IActionResult> Update(int id, Admin updatedAdmin)
        {
            _logger.LogInformation($"Updating admin with ID: {id}");

            if (id != updatedAdmin.IDAdmin)
            {
                _logger.LogWarning($"ID in URL ({id}) does not match ID in body ({updatedAdmin.IDAdmin})");
                return BadRequest("ID in URL does not match ID in body");
            }

            _db.Entry(updatedAdmin).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Admin.Any(e => e.IDAdmin == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var kafkaMessage = new KafkaMessage<Admin>
            {
                Operation = "UPDATE",
                Data = updatedAdmin
            };

            var message = JsonConvert.SerializeObject(kafkaMessage);
            var kafkaMsg = new Message<string, string>
            {
                Key = null,
                Value = message
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(_topic, kafkaMsg);
                _logger.LogInformation($"Kafka message delivered. Value: '{deliveryResult.Value}', TopicPartitionOffset: '{deliveryResult.TopicPartitionOffset}'");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"Kafka ProduceException: {ex.Error.Reason}");
                return StatusCode(500, "Error producing message to Kafka.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }

            return NoContent();
        }


        [HttpDelete("api/admins/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"Deleting admin with ID: {id}");

            var admin = await _db.Admin.FindAsync(id);
            if (admin == null)
            {
                _logger.LogWarning($"Admin with ID: {id} not found.");
                return NotFound();
            }

            _db.Admin.Remove(admin);
            await _db.SaveChangesAsync();

            var kafkaMessage = new KafkaMessage<Admin>
            {
                Operation = "DELETE",
                Data = admin
            };

            var message = JsonConvert.SerializeObject(kafkaMessage);
            var kafkaMsg = new Message<string, string>
            {
                Key = null,
                Value = message
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(_topic, kafkaMsg);
                _logger.LogInformation($"Kafka message delivered. Value: '{deliveryResult.Value}', TopicPartitionOffset: '{deliveryResult.TopicPartitionOffset}'");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError($"Kafka ProduceException: {ex.Error.Reason}");
                return StatusCode(500, "Error producing message to Kafka.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Unexpected error occurred.");
            }

            return NoContent();
        }
    }
}


//using QAForSber.Data;
//using QAForSber.Models;
//using Microsoft.AspNetCore.Mvc;
//using Confluent.Kafka;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

//namespace QAForSber.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class AdminController : ControllerBase
//    {
//        private readonly ApplicationDbContext _db;
//        private readonly IProducer<string, string> _producer;
//        private readonly string _topic;
//        private readonly ILogger<AdminController> _logger;

//        public AdminController(ApplicationDbContext db, IConfiguration configuration, ILogger<AdminController> logger)
//        {
//            _db = db;
//            _logger = logger;

//            var config = new ProducerConfig
//            {
//                BootstrapServers = configuration["Kafka:BootstrapServers"]
//            };

//            _producer = new ProducerBuilder<string, string>(config).Build();
//            _topic = configuration["Kafka:Topic"];
//        }

//        [HttpGet("api/admins")]
//        public async Task<IActionResult> Get()
//        {
//            IEnumerable<Admin> admins = _db.Admin.ToList();

//            var message = JsonConvert.SerializeObject(admins);

//            var kafkaMessage = new Message<string, string>
//            {
//                Key = null,
//                Value = message
//            };

//            try
//            {
//                var deliveryResult = await _producer.ProduceAsync(_topic, kafkaMessage);
//                _logger.LogInformation($"Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Error producing message: {ex.Message}");
//            }

//            return Ok(admins);
//        }

//        
//    }
//}
