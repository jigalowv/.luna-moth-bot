using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public class EditRawModal : IModal
{
    public string Title => "Редактирование поста";

    [InputLabel("Содержание поста")]
    [ModalTextInput("edit_raw_content", style: TextInputStyle.Paragraph)]
    [RequiredInput(true)]
    public string Content { get; set; } = null!;
}