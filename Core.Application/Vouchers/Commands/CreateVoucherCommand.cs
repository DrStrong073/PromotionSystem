using Application.Exceptions;
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
    
    public record CreateVoucherCommand(
        string PromotionId,
        string Code,
        decimal DiscountAmount,
        decimal MinOrderValue,
        int UsageLimit
    ) : IRequest<string>;

    // 2. Handler xử lý logic
    public class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, string>
    {
        private readonly IMongoRepository<Voucher> _voucherRepo;
        private readonly IMongoRepository<Promotion> _promotionRepo;
        private readonly IElasticSearchService<Voucher> _esService;

        public CreateVoucherCommandHandler(
            IMongoRepository<Voucher> voucherRepo,
            IMongoRepository<Promotion> promotionRepo,
            IElasticSearchService<Voucher> esService)
        {
            _voucherRepo = voucherRepo;
            _promotionRepo = promotionRepo;
            _esService = esService;
        }

        public async Task<string> Handle(CreateVoucherCommand request, CancellationToken ct)
        {
            
            var promotion = await _promotionRepo.GetByIdAsync(request.PromotionId, ct);
            if (promotion == null)
            {
                throw new BadRequestException($"Chương trình khuyến mãi với ID '{request.PromotionId}' không tồn tại!");
            }

            var voucher = new Voucher
            {
                PromotionId = request.PromotionId,
                Code = request.Code.ToUpper().Trim(),
                DiscountAmount = request.DiscountAmount,
                MinOrderValue = request.MinOrderValue,
                UsageLimit = request.UsageLimit
            };

            await _voucherRepo.AddAsync(voucher, ct);
            await _esService.IndexAsync(voucher, ct);

            return voucher.Id;
        }
    }
}
