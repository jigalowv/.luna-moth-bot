namespace Luna.Presentation.Modules.Modals;

public record PendingForumPostDto(
    ulong UserId,
    ulong ForumChannelId,
    string Title,
    string Description,
    string ImageUrl
);