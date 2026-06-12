using ErrorOr;
using MediatR;

namespace Luna.Application.EventEdit.Commands.Cancel;

public record EventEditCancelRequest(
    ulong ExecutorDiscordId,
    int? EventId,
    string? EndCode
) : IRequest<ErrorOr<string?>>;