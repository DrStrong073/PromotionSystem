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
    public record GetVouchersQuery(string? Keyword, string? PromotionId, int Page = 1, int PageSize = 20)
    : IRequest<(IEnumerable<Voucher> Items, long Total)>;

    public class GetVouchersQueryHandler : IRequestHandler<GetVouchersQuery, (IEnumerable<Voucher>, long)>
    {
        private readonly IElasticSearchService<Voucher> _esService;

        public GetVouchersQueryHandler(IElasticSearchService<Voucher> esService)
        {
            _esService = esService;
        }

        public async Task<(IEnumerable<Voucher>, long)> Handle(GetVouchersQuery request, CancellationToken ct)
        {
            return await _esService.SearchAsync(
                request.Keyword,
                request.PromotionId,
                request.Page,
                request.PageSize,
                ct);
        }
    }
}
