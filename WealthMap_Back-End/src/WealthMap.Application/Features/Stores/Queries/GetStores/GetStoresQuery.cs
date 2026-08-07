using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;

namespace WealthMap.Application.Features.Stores.Queries.GetStores;

public record GetStoresQuery(Guid UserId) : IQuery<IReadOnlyList<StoreDto>>;