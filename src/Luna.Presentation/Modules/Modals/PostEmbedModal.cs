using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public class PostEmbedModal : IModal
{
    [InputLabel("@everyone и/или @here")]
    [ModalTextInput("post_embed_header", style: TextInputStyle.Short)]
    [RequiredInput(false)]
    public string? Header { get; set; }

    [InputLabel("Упоминания")]
    [ModalRoleSelect("post_embed_mentions", maxValues: 25)]
    [RequiredInput(false)]
    public IRole[] Mentionables { get; set; } = [];

    [InputLabel("Содержание поста")]
    [ModalTextInput("post_embed_content", style: TextInputStyle.Paragraph)]
    [RequiredInput(true)]
    public string Content { get; set; } = null!;

    [InputLabel("Ссылка на изображение")]
    [ModalTextInput("post_embed_image", style: TextInputStyle.Short, placeholder: "https://...")]
    [RequiredInput(false)]
    public string? ImageUrl { get; set; }

    public string Title => "Создание поста";   
}