using WealthMap.Application.Common.Messaging;
using WealthMap.Application.Features.Stores.DTOs;

namespace WealthMap.Application.Features.Stores.Queries.GetStoreById;

public record GetStoreByIdQuery(Guid Id, Guid UserId) : IQuery<StoreDto>;