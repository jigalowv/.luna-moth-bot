using Discord;
using Discord.Interactions;

namespace Luna.Presentation.Modules.Modals;

public class ExecutorsModal : IModal
{
    public string Title => "Выбор ведущих";

    [InputLabel("Выберите ведущих события")]
    [ModalUserSelect("event_executors_select", maxValues: 25)]
    [RequiredInput(true)]
    public IUser[] Users { get; set; } = [];
}