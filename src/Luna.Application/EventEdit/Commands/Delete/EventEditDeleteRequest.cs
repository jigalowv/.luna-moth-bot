using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Delete;

public record EventEditDeleteRequest(
    ulong ExecutorDiscordId,
    string? EndCode,
    int? EventId
) : IRequest<ErrorOr<string?>>;