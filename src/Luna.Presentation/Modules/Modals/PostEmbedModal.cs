using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public class PostEmbedModal : IModal
{
    [InputLabel("Целевой канал")]
    [ModalChannelSelect("post_embed_channel")]
    [RequiredInput(false)]
    public IChannel? Channel { get; set; }

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