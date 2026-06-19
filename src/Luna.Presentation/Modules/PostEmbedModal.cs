using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules;

public class PostEmbedModal : IModal
{
    [InputLabel("Содержание поста")]
    [ModalTextInput("post_forum_description", style: TextInputStyle.Paragraph)]
    public string Description { get; set; } = null!;

    [InputLabel("Ссылка на изображение")]
    [RequiredInput(false)]
    [ModalTextInput("post_forum_image_url", placeholder: "https://...")]
    public string ImageUrl { get; set; } = null!;

    public string Title => "Создание поста";
}