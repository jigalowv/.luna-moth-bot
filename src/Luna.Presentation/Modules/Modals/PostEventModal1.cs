using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public sealed class PostEventModal1 : IModal
{
    public string Title => "Детали события (1/2)";

    [InputLabel("Тема")]
    [ModalTextInput("topic", TextInputStyle.Short, placeholder: "Введите тему события...", maxLength: 256)]
    [RequiredInput(true)]
    public string Topic { get; set; } = string.Empty;

    [InputLabel("Описание")]
    [ModalTextInput("description", TextInputStyle.Paragraph, placeholder: "Введите подробное описание...", maxLength: 4000)]
    [RequiredInput(true)]
    public string Description { get; set; } = string.Empty;

    [InputLabel("Ссылка на картинку")]
    [ModalTextInput("image_url", TextInputStyle.Short, placeholder: "https://...")]
    [RequiredInput(false)]
    public string ImageUrl { get; set; } = string.Empty;

    [InputLabel("Ссылка на правила")]
    [ModalTextInput("rules_url", TextInputStyle.Short, placeholder: "https://...")]
    [RequiredInput(true)]
    public string RulesUrl { get; set; } = string.Empty;
}