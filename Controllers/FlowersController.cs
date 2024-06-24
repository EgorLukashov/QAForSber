//using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;
//using QAForSber.Models;
//using QAForSber.Data;
////using System.Xml.Linq;

//namespace WebApiFlowersStore.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class FlowersController : ControllerBase
//    {
//        private readonly ApplicationDbContext _db;
//        public FlowersController(ApplicationDbContext db)
//        {
//            _db = db;
//        }

//        [HttpGet("api/flowers")]
//        public IActionResult Get()
//        {
//            IEnumerable<Flowers> flowers = _db.flowers_available.ToList();

//            return Ok(flowers);
//        }

//        [HttpPatch("api/flowers/{id}/{name}")]
//        public async Task<IActionResult> Update(int id, string name)
//        {
//            // Assuming you have a model named Flowers with properties Name, Price, and Profit
//            Flowers flowers = _db.flowers_available.Find(id);
//            flowers.Name = name;
//            _db.SaveChanges();

//            return Ok(flowers);
//        }
//        [HttpDelete]
//        public void Delete(int? id)
//        {
//            if (id == null)
//            {
//                // Handle the case where id is null, perhaps return a BadRequest result.
//                // return BadRequest();
//            }

//            Flowers fl = _db.flowers_available.Find(id);

//            if (fl != null)
//            {
//                _db.Remove(fl);
//                _db.SaveChanges();
//            }

//            //return RedirectToAction("Index");
//        }


//    }
//}
