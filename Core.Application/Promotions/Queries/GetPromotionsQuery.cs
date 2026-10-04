using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Promotions.Queries
{
    public record GetPromotionsQuery() : IRequest<IEnumerable<Promotion>>;

    public class GetPromotionsQueryHandler : IRequestHandler<GetPromotionsQuery, IEnumerable<Promotion>>
    {
        private readonly IElasticSearchService<Promotion> _esService;

        public GetPromotionsQueryHandler(IElasticSearchService<Promotion> esService)
        {
            _esService = esService;
        }

        public async Task<IEnumerable<Promotion>> Handle(GetPromotionsQuery request, CancellationToken ct)
        {
            return await _esService.GetAllAsync(ct);
        }
    }
}
