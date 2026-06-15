using Microsoft.AspNetCore.Mvc;
using TOSWeb.Database;
using TOSWeb.Models;

namespace TOSWeb.Controllers
{
    [ApiController]
    [Route("api")]
    public class TachinomiApiController : ControllerBase
    {
        private readonly DatabaseManager _db;

        public TachinomiApiController(DatabaseManager db)
        {
            _db = db;
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { isSqlAvailable = _db.IsSqlAvailable });
        }

        [HttpGet("menu")]
        public IActionResult GetMenu()
        {
            return Ok(_db.GetMenuItems());
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            return Ok(_db.GetCheckoutHistory());
        }

        [HttpPost("order")]
        public IActionResult SaveOrder([FromBody] CheckoutRequest request)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { message = "Invalid order details." });
            }

            bool success = _db.SaveTachinomiOrder(request.TotalAmount, request.Items, out int orderId);
            if (success)
            {
                return Ok(new { success = true, orderId });
            }
            else
            {
                if (!_db.IsSqlAvailable)
                {
                    return Ok(new { success = true, orderId = -1, isDemo = true });
                }
                return StatusCode(500);
            }
        }

        [HttpPost("history/clear")]
        public IActionResult ClearHistory()
        {
            bool success = _db.ClearAllHistory();
            if (success)
            {
                return Ok(new { success = true });
            }
            else
            {
                if (!_db.IsSqlAvailable)
                {
                    return Ok(new { success = true, isDemo = true });
                }
                return StatusCode(500);
            }
        }
    }
}
