using Discord.Interactions;
using MediatR;

namespace Luna.Presentation.Modules;

[Group("event", "Управление событиями.")]
public class EventModule
    : InteractionModuleBase<SocketInteractionContext>
{
    [Group("edit", "Изменение событий.")]
    public class EventEditSubgroup : EventEditModule
    {
        public EventEditSubgroup(
            IMediator mediator, 
            ILogger<EventEditModule> logger) 
            : base(mediator, logger)
        { }
    }
}