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
    public record DeleteVoucherCommand(string Id) : IRequest<bool>;

    public class DeleteVoucherCommandHandler : IRequestHandler<DeleteVoucherCommand, bool>
    {
        private readonly IMongoRepository<Voucher> _mongoRepo;
        private readonly IElasticSearchService<Voucher> _esService;

        public DeleteVoucherCommandHandler(IMongoRepository<Voucher> mongoRepo, IElasticSearchService<Voucher> esService)
        {
            _mongoRepo = mongoRepo;
            _esService = esService;
        }

        public async Task<bool> Handle(DeleteVoucherCommand request, CancellationToken ct)
        {
          
            var voucher = await _mongoRepo.GetByIdAsync(request.Id, ct);

            if (voucher == null) return false;

            await _mongoRepo.DeleteAsync(request.Id, ct);
            await _esService.DeleteAsync(request.Id, ct);

            return true;
        }
    }
}
