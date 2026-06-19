using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules;

public class PostForumModal : IModal
{
    public string Title => "Создание поста в форуме";
    
    [InputLabel("Заголовок")]
    [ModalTextInput("post_forum_title", maxLength: 100)]
    public string ForumTitle { get; set; } = null!;

    [InputLabel("Содержание поста")]
    [ModalTextInput("post_forum_description", style: TextInputStyle.Paragraph, maxLength: 1000)]
    public string Description { get; set; } = null!;

    [InputLabel("Ссылка на изображение")]
    [ModalTextInput("post_forum_image_url", placeholder: "https://...")]
    public string ImageUrl { get; set; } = null!;

    [InputLabel("Форум")]
    [ModalChannelSelect("post_forum_channel")]
    public IForumChannel[] Forums { get; set; } = [];
}