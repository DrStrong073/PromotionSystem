using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Vouchers.Commands
{
    
    public record ImportVouchersCommand(string PromotionId, Stream CsvStream) : IRequest<int>;

    public class ImportVouchersCommandHandler : IRequestHandler<ImportVouchersCommand, int>
    {
        private readonly IMongoRepository<Voucher> _mongoRepo;
        private readonly IElasticSearchService<Voucher> _esService;

        public ImportVouchersCommandHandler(IMongoRepository<Voucher> mongoRepo, IElasticSearchService<Voucher> esService)
        {
            _mongoRepo = mongoRepo;
            _esService = esService;
        }

        public async Task<int> Handle(ImportVouchersCommand request, CancellationToken ct)
        {
            var vouchers = new List<Voucher>();
            using var reader = new StreamReader(request.CsvStream);

            string? line;
            bool isFirstLine = true;

            // Đọc từng dòng của file CSV
            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                if (isFirstLine) { isFirstLine = false; continue; } 

                var parts = line.Split(',');
                if (parts.Length < 4) continue; 

                vouchers.Add(new Voucher
                {
                    PromotionId = request.PromotionId,
                    Code = parts[0].Trim().ToUpper(),
                    DiscountAmount = decimal.Parse(parts[1].Trim()),
                    MinOrderValue = decimal.Parse(parts[2].Trim()),
                    UsageLimit = int.Parse(parts[3].Trim())
                });
            }

            if (vouchers.Count == 0) return 0;

            await _mongoRepo.AddRangeAsync(vouchers, ct);
            await _esService.BulkIndexAsync(vouchers, ct);

            return vouchers.Count;
        }
    }
}
