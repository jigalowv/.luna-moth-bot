using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public sealed class PostRawModal : IModal
{
    public string Title => "Создание поста";

    [InputLabel("Целевой канал")]
    [ModalChannelSelect("post_raw_channel")]
    [RequiredInput(false)]
    public IChannel? Channel { get; set; }

    [InputLabel("Содержание поста")]
    [ModalTextInput("post_raw_content", style: TextInputStyle.Paragraph)]
    [RequiredInput(true)]
    public string Content { get; set; } = string.Empty;
}