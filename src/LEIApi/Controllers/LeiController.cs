using Business.Workflow;
using Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace LEIApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeiController : ControllerBase
    {
        private LeiRecordBusiness leiRecordBusiness;
        public LeiController(LeiRecordBusiness leiRecordBusiness)
        {
            this.leiRecordBusiness = leiRecordBusiness;
        }
        [HttpPost(nameof(AddCsv))]
        public IActionResult AddCsv(IFormFile file)
        {
            if (file == null)
                return BadRequest("File has not to be null!");
            var result = leiRecordBusiness.AddCsv(file);
            return StatusCode(200, result);
        }
        [HttpPost(nameof(Add))]
        public IActionResult Add(LeiRecordModel leiRecordModel)
        {
            if (leiRecordModel == null)
                return BadRequest("Record has not to be null!");
            LeiRecordModel result = leiRecordBusiness.AddLeiRecord(leiRecordModel);
            if (result != null)
                return StatusCode(201, result);
            return BadRequest("An Error Occured!");
        }
        [HttpGet(nameof(GetById))]
        public IActionResult GetById(string id)
        {
            if (id == null)
                return NotFound();
            LeiRecordModel result = leiRecordBusiness.Get(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        [HttpGet(nameof(GetByLeiId))]
        public IActionResult GetByLeiId(string id)
        {
            if (id == null)
                return NotFound();
            LeiRecordModel result = leiRecordBusiness.Get(id, true);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet(nameof(GetAll))]
        public IActionResult GetAll()
        {
            return StatusCode(200, leiRecordBusiness.GetAll());
        }

        [HttpDelete(nameof(Delete))]
        public IActionResult Delete(string id)
        {
            if (leiRecordBusiness.Delete(id))
                return NoContent();
            return BadRequest("Not Deleted!");
        }

        [HttpPost(nameof(Update))]
        public IActionResult Update(LeiRecordModel leiRecordModel)
        {
            LeiRecordModel result = leiRecordBusiness.Update(leiRecordModel);
            if (result == null)
                return BadRequest("Not Updated!");
            return Ok("Successfuly updated!");
        }
    }
}
