using Discord;
using Discord.Interactions;
using Luna.Application.EventTypes.Queries.EventTypeSearch;
using MediatR;

namespace Luna.Presentation.Modules;

public sealed class EventTypeAutocomplete : AutocompleteHandler
{
    private readonly ILogger<EventTypeAutocomplete> _logger;

    public EventTypeAutocomplete(
        ILogger<EventTypeAutocomplete> logger)
    {
        _logger = logger;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context, 
        IAutocompleteInteraction autocompleteInteraction, 
        IParameterInfo parameter, 
        IServiceProvider services)
    {
        try
        {
            var mediator = services.GetRequiredService<IMediator>();

            string? userInput = autocompleteInteraction.Data.Current.Value?.ToString();

            if (userInput is null)
            {
                return AutocompletionResult.FromSuccess();
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var request = new EventTypeSearchRequest(
                ExecutorDiscordId: context.User.Id,
                SearchTerm: userInput
            );

            var response = await mediator.Send(request, cts.Token);

            if (response.IsError)
                return AutocompletionResult.FromSuccess();

            var suggestions = response.Value.Titles
                .Select(i => new AutocompleteResult(i, i))
                .Take(25)
                .ToList();

            return AutocompletionResult.FromSuccess(suggestions);
        }
        catch (OperationCanceledException)
        {
            return AutocompletionResult.FromSuccess(); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured while searching eventType.");
            return AutocompletionResult.FromError(ex);
        }
    }
}