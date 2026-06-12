using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.End;

public record EventEditEndRequest(
    ulong ExecutorDiscordId,
    string? EndCode,
    int? EventId
) : IRequest<ErrorOr<string?>>;