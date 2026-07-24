using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public sealed class PostRawModal : IModal
{
    public string Title => "Создание поста";

    [InputLabel("Упоминания сверху")]
    [ModalRoleSelect("post_embed_mentions", maxValues: 25)]
    [RequiredInput(false)]
    public IRole[] Mentionables { get; set; } = [];

    [InputLabel("Содержание поста")]
    [ModalTextInput("post_raw_content", style: TextInputStyle.Paragraph)]
    [RequiredInput(true)]
    public string Content { get; set; } = string.Empty;
}