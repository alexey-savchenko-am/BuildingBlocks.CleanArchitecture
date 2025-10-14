using BuildingBlocks.CleanArchitecture.Domain.Output;
using MediatR;

namespace BuildingBlocks.CleanArchitecture.Application.CQRS;

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{ }

