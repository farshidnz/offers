using Mapster;
using MassTransit;
using CashrewardsOffers.Application.Common.Interfaces;
using CashrewardsOffers.Domain.Entities;
using CashrewardsOffers.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashrewardsOffers.Application.MerchantSuggestions.Mappings;
using CashrewardsOffers.Application.MerchantSuggestions.Services;
using CashrewardsOffers.Domain.Enums;

namespace CashrewardsOffers.Application.MerchantSuggestions.Queries.GetMerchantSuggestions.v1
{
    public class GetMerchantSuggestionsQuery
    {
        public int StartingRank { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public int[] Categories { get; set; }
    }

    public class GetMerchantSuggestionsResponse
    {
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public MerchantSuggestion[] Merchants { get; set; }
    }

    public class MerchantSuggestionsConsumer : IConsumer<GetMerchantSuggestionsQuery>
    {
        private readonly IRankedMerchantPersistenceContext persistenceContext;
        private readonly IRankedMerchantRandomizationService rankedMerchantRandomizationService;
        private readonly IMerchantSuggestionMapper mapper;

        public MerchantSuggestionsConsumer(IRankedMerchantPersistenceContext applicationPersistenceContext,
            IRankedMerchantRandomizationService rankedMerchantRandomizationService,
            IMerchantSuggestionMapper mapper)
        {
            this.persistenceContext = applicationPersistenceContext;
            this.rankedMerchantRandomizationService = rankedMerchantRandomizationService;
            this.mapper = mapper;
        }

        public async Task Consume(ConsumeContext<GetMerchantSuggestionsQuery> context)
        {
            var rankedMerchants = 
                await persistenceContext.GetRankedMerchants(
                    (int)Client.Cashrewards,
                    null, 
                    context.Message.Categories);

            var shuffledMerchants = rankedMerchantRandomizationService.ShuffleRankedMerchants(rankedMerchants);

            var suggestions = mapper.MapFromRankedMerchants(shuffledMerchants,
                context.Message.StartingRank,
                context.Message.PageSize, 
                context.Message.Categories);

            await context.RespondAsync(new GetMerchantSuggestionsResponse()
            {
                Count = suggestions.Count,
                TotalCount = suggestions.TotalCount,
                Merchants = suggestions.Merchants.Adapt<MerchantSuggestion[]>()
            });
        }
    }
}