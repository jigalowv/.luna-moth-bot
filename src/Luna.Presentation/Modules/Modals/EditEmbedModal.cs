using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public class EditEmbedModal : IModal
{
    public string Title => "Редактирование embed-сообщения";

    [InputLabel("Содержание поста")]
    [ModalTextInput("edit_embed_content", style: TextInputStyle.Paragraph)]
    [RequiredInput(true)]
    public string Content { get; set; } = null!;

    [InputLabel("Ссылка на изображение")]
    [ModalTextInput("edit_embed_image", style: TextInputStyle.Short, placeholder: "https://...")]
    [RequiredInput(false)]
    public string? ImageUrl { get; set; }
}