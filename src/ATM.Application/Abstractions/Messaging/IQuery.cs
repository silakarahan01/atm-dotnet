using ATM.Domain.Common;
using MediatR;

namespace ATM.Application.Abstractions.Messaging;

/// <summary>Durumu değiştirmeyen, veri okuyan bir sorgu.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
