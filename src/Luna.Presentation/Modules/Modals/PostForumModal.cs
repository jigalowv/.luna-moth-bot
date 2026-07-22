using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public sealed class PostForumModal : IModal
{
    public string Title => "Создание поста в форуме";
    
    [InputLabel("Заголовок")]
    [ModalTextInput("post_forum_title", maxLength: 100)]
    [RequiredInput(true)]
    public string ForumTitle { get; set; } = string.Empty;

    [InputLabel("Содержание поста")]
    [ModalTextInput("post_forum_description", style: TextInputStyle.Paragraph, maxLength: 1000)]
    [RequiredInput(true)]
    public string Description { get; set; } = string.Empty;

    [InputLabel("Ссылка на изображение")]
    [ModalTextInput("post_forum_image_url", placeholder: "https://...")]
    [RequiredInput(true)]
    public string ImageUrl { get; set; } = string.Empty;

    [InputLabel("Форум")]
    [ModalChannelSelect("post_forum_channel")]
    [RequiredInput(true)]
    public IForumChannel[] Forums { get; init; } = [];
}