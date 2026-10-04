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
    public record UpdateVoucherCommand(
    string Id,
    decimal DiscountAmount,
    decimal MinOrderValue,
    bool IsActive
) : IRequest<bool>;

    public class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand, bool>
    {
        private readonly IMongoRepository<Voucher> _mongoRepo;
        private readonly IElasticSearchService<Voucher> _esService;

        public UpdateVoucherCommandHandler(IMongoRepository<Voucher> mongoRepo, IElasticSearchService<Voucher> esService)
        {
            _mongoRepo = mongoRepo;
            _esService = esService;
        }

        public async Task<bool> Handle(UpdateVoucherCommand request, CancellationToken ct)
        {
          
            var voucher = await _mongoRepo.GetByIdAsync(request.Id, ct);
            if (voucher == null) return false;

            voucher.DiscountAmount = request.DiscountAmount;
            voucher.MinOrderValue = request.MinOrderValue;
            voucher.IsActive = request.IsActive;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _mongoRepo.UpdateAsync(voucher.Id, voucher, ct);
            await _esService.IndexAsync(voucher, ct); 

            return true;
        }
    }
}
