using ErrorOr;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Auth.ExecutorHasAccess;

public record AuthExecutorHasAccessRequest(
    ulong ExecutorDiscordId,
    ExecutorRole MinRole
) : IRequest<ErrorOr<bool>>;