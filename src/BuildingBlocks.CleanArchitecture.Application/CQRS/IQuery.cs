using BuildingBlocks.CleanArchitecture.Domain.Output;
using MediatR;

namespace BuildingBlocks.CleanArchitecture.Application.CQRS;

public interface IQuery<TResponse>
    : IRequest<Result<TResponse>>
{ }
