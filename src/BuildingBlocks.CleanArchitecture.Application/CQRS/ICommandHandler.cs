using BuildingBlocks.CleanArchitecture.Domain.Output;
using MediatR;

namespace BuildingBlocks.CleanArchitecture.Application.CQRS;

public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{ }

public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{ }

