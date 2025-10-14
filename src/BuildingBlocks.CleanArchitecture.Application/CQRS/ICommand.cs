using BuildingBlocks.CleanArchitecture.Domain.Output;
using MediatR;

namespace BuildingBlocks.CleanArchitecture.Application.CQRS;

public interface ICommand
    : IRequest<Result>
{ }

public interface ICommand<TResponse>
    : IRequest<Result<TResponse>>
{ }
