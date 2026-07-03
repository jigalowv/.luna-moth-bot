using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules;

public class PostEventModal : IModal
{
    public string Title => "Создание поста";

    [InputLabel("Тема")]
    [ModalTextInput("post_topic")]
    public string Topic { get; set; } = null!;

    [InputLabel("Содержание поста")]
    [ModalTextInput("post_description", style: TextInputStyle.Paragraph)]
    public string Description { get; set; } = null!;

    [InputLabel("Дата и время начала")]
    [ModalTextInput("post_start_at")]
    public string StartAt { get; set; } = null!;

    [InputLabel("Место проведения")]
    [ModalChannelSelect("post_voice_channel")]
    public IChannel[] Place { get; set; } = [];
}

public class PostEventModal2 : IModal
{
    public string Title => "Продолжение...";

    [InputLabel("Ведущие")]
    [ModalUserSelect("post_leaders", maxValues: 5)]
    public IUser[] Leaders { get; set; } = [];

    [InputLabel("Упомянуть")]
    [RequiredInput(false)]
    [ModalRoleSelect("post_roles", maxValues: 5)]
    public IRole[] Mentions { get; set; } = [];

    [InputLabel("Изображение")]
    [ModalTextInput("post_image_url", placeholder: "https://...")]
    public string ImageUrl { get; set; } = null!;
}