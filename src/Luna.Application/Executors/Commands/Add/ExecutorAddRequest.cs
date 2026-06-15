using ErrorOr;
using Luna.Domain.Enums;
using MediatR;

namespace Luna.Application.Executors.Commands.Add;

public record ExecutorAddRequest(
    ulong ExecutorDiscordId,
    ulong TargetDiscordId,
    ExecutorRole Role,
    string Name,
    string? ImageUrl
) : IRequest<ErrorOr<bool>>;