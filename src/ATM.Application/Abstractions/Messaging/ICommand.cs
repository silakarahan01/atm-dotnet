using ATM.Domain.Common;
using MediatR;

namespace ATM.Application.Abstractions.Messaging;

/// <summary>Değer döndürmeyen, sistemin durumunu değiştiren bir komut.</summary>
public interface ICommand : IRequest<Result>;

/// <summary>Bir değer döndüren komut.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
