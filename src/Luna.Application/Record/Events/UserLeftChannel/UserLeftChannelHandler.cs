using ErrorOr;
using Luna.Application.Common.Interfaces;
using MediatR;

namespace Luna.Application.Record.Events.UserLeftChannel;

public sealed class UserLeftChannelHandler 
    : IRequestHandler<UserLeftChannelRequest, ErrorOr<bool>>
{
    private readonly IRecordRepository _trackedChannelRepository;
    private readonly IRecordAttendanceRepository _trackedAttendanceRepository;

    public UserLeftChannelHandler(
        IRecordRepository trackedChannelRepository,
        IRecordAttendanceRepository trackedAttendanceRepository
    )
    {
        _trackedChannelRepository = trackedChannelRepository;
        _trackedAttendanceRepository = trackedAttendanceRepository;
    }

    public async Task<ErrorOr<bool>> Handle(
        UserLeftChannelRequest request, 
        CancellationToken ct)
    {
        var record = await _trackedChannelRepository
            .GetAsync(request.ChannelId, ct);

        if (record is null)
            return Error.NotFound(
                "TrackedChannel.NotFound", "Tracked Channel not found");

        bool success = await _trackedAttendanceRepository.EndAsync(
            record.Id, request.DiscordUserId, ct);

        if (!success)
            return Error.Failure(
                "Repository.Failure", "Repository error.");

        return true;
    }
}