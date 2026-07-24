using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public sealed class PostEventModal2 : IModal
{
    public string Title => "Событие (Шаг 2/2)";

    [InputLabel("@everyone и/или @here")]
    [ModalTextInput("header", style: TextInputStyle.Short)]
    [RequiredInput(false)]
    public string? Header { get; set; }

    [InputLabel("Упоминания сверху")]
    [ModalRoleSelect("mentions", maxValues: 25)]
    [RequiredInput(true)]
    public IRole[] Mentionables { get; set; } = [];

    [InputLabel("Место проведения")]
    [ModalChannelSelect("place")]
    [RequiredInput(true)]
    [ChannelTypes(ChannelType.Text, ChannelType.Voice, ChannelType.Stage)]
    public IChannel Place { get; set; } = null!;

    [InputLabel("Ведущие")]
    [ModalUserSelect("leaders", maxValues: 25)]
    [RequiredInput(true)]
    public IUser[] Leaders { get; set; } = [];
}