using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules;

public class PostRawModal : IModal
{
    public string Title => "Создание сообщения";

    [InputLabel("Содержимое")]
    [ModalTextInput("post_raw_content", TextInputStyle.Paragraph, maxLength: 2000)]
    public string Content { get; set; } = null!;
}