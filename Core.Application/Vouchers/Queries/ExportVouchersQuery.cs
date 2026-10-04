using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Vouchers.Queries
{
    // Trả về mảng byte (byte[]) của file CSV để cho API download
    public record ExportVouchersQuery(string PromotionId) : IRequest<byte[]>;

    public class ExportVouchersQueryHandler : IRequestHandler<ExportVouchersQuery, byte[]>
    {
        private readonly IElasticSearchService<Voucher> _esService;

        public ExportVouchersQueryHandler(IElasticSearchService<Voucher> esService)
        {
            _esService = esService;
        }

        public async Task<byte[]> Handle(ExportVouchersQuery request, CancellationToken ct)
        {
            // Lấy toàn bộ voucher của Promotion này từ Elasticsearch
            var (items, _) = await _esService.SearchAsync(null, request.PromotionId, 1, 10000, ct);

            // Tạo nội dung CSV trên RAM
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            // Ghi dòng Header
            await writer.WriteLineAsync("Code,DiscountAmount,MinOrderValue,UsageLimit");

            // Ghi dữ liệu
            foreach (var item in items)
            {
                await writer.WriteLineAsync($"{item.Code},{item.DiscountAmount},{item.MinOrderValue},{item.UsageLimit}");
            }

            await writer.FlushAsync();
            return memoryStream.ToArray();
        }
    }
}
