using Application.Vouchers.Commands;
using Application.Vouchers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VouchersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VouchersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Thêm Voucher
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoucherCommand command)
        {
            var id = await _mediator.Send(command);
            return Ok(new { Id = id, Message = "Tạo voucher thành công" });
        }

        // 2. Sửa Voucher
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateVoucherCommand command)
        {
            if (id != command.Id) return BadRequest("ID trên URL và ID trong body không khớp.");

            var success = await _mediator.Send(command);
            if (!success) return NotFound("Không tìm thấy Voucher.");

            return NoContent(); // 204 No Content
        }

        // 3. Xóa Voucher
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _mediator.Send(new DeleteVoucherCommand(id));
            if (!success) return NotFound();
            return NoContent();
        }

        // 4. Tìm kiếm / Lấy danh sách Voucher (Có phân trang)
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] string? keyword, [FromQuery] string? promotionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = new GetVouchersQuery(keyword, promotionId, page, pageSize);
            var (items, total) = await _mediator.Send(query);

            return Ok(new
            {
                Data = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        // 5. Import Vouchers từ file CSV
        [HttpPost("import")]
        public async Task<IActionResult> ImportCsv([FromQuery] string promotionId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File không hợp lệ.");

            // Mở luồng đọc file và gửi cho tầng Application xử lý
            using var stream = file.OpenReadStream();
            var command = new ImportVouchersCommand(promotionId, stream);

            var count = await _mediator.Send(command);
            return Ok(new { Message = $"Đã import thành công {count} vouchers." });
        }

        // 6. Export Vouchers ra file CSV
        [HttpGet("export")]
        public async Task<IActionResult> ExportCsv([FromQuery] string promotionId)
        {
            var query = new ExportVouchersQuery(promotionId);
            var csvBytes = await _mediator.Send(query);

            // Trả về file tải xuống cho trình duyệt
            return File(csvBytes, "text/csv", $"vouchers_{promotionId}.csv");
        }
    }
}
