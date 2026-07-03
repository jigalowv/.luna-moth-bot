using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules;

public class PostEmbedModal : IModal
{
    [InputLabel("Текст сверху")]
    [RequiredInput(false)]
    [ModalRoleSelect("post_embed_raw_mention", maxValues: 10)]
    public IRole[] Mentions { get; set; } = [];

    [InputLabel("Содержание поста")]
    [ModalTextInput("post_embed_description", style: TextInputStyle.Paragraph)]
    public string Description { get; set; } = null!;

    [InputLabel("Ссылка на изображение")]
    [RequiredInput(false)]
    [ModalTextInput("post_embed_image_url", placeholder: "https://...")]
    public string ImageUrl { get; set; } = null!;

    public string Title => "Создание поста";
}

public class PostEditEmbedModal : IModal
{
    [InputLabel("Содержание поста")]
    [ModalTextInput("post_edit_embed_description", style: TextInputStyle.Paragraph)]
    public string Description { get; set; } = null!;

    [InputLabel("Ссылка на изображение")]
    [RequiredInput(false)]
    [ModalTextInput("post_edit_embed_image_url", placeholder: "https://...")]
    public string ImageUrl { get; set; } = null!;

    public string Title => "Создание поста";
}