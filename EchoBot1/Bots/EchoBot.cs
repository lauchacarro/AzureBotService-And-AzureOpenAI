// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using Azure.AI.OpenAI;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly IStatePropertyAccessor<CustomChatGptState> _chatgptState;
        private readonly ConversationState _conversationState;
        public EchoBot(ConversationState conversationState)
        {
            _chatgptState = conversationState.CreateProperty<CustomChatGptState>(nameof(CustomChatGptState));
            _conversationState = conversationState;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var state = await _chatgptState.GetAsync(turnContext, () => new(), cancellationToken);


            var AOAI_ENDPOINT = "https://[nombre de tu recurso de Azure OpenAI].openai.azure.com/";
            var AOAI_KEY = "[Clave Secreta del Recurso]";
            var AOAI_DEPLOYMENTID = "[Nombre de la implementación]";

            var endpoint = new Uri(AOAI_ENDPOINT);
            var credentials = new Azure.AzureKeyCredential(AOAI_KEY);
            var openAIClient = new OpenAIClient(endpoint, credentials);


            var completionOptions = new ChatCompletionsOptions
            {
                MaxTokens = 400,
                Temperature = 1f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,
                NucleusSamplingFactor = 0.95f // Top P
            };


            if (state.Messages is null || !state.Messages.Any())
            {
                var system =
   """
    You are a hiking enthusiast who helps people discover fun hikes in their area. You are upbeat and friendly. You introduce yourself when first saying hello. When helping people out, you always ask them for this information to inform the hiking recommendation you provide:

    1. Where they are located
    2. What hiking intensity they are looking for

    You will then provide three suggestions for nearby hikes that vary in length after you get that information. You will also share an interesting fact about the local nature on the hikes when making a recommendation.
    """;


                completionOptions.Messages.Add(new ChatMessage(ChatRole.System, system));
            }
            else
            {
                foreach (var item in state.Messages)
                {
                    completionOptions.Messages.Add(item);
                }
            }

            completionOptions.Messages.Add(new ChatMessage(ChatRole.User, turnContext.Activity.Text));


            ChatCompletions response = await openAIClient.GetChatCompletionsAsync(AOAI_DEPLOYMENTID, completionOptions, cancellationToken);

            completionOptions.Messages.Add(response.Choices[0].Message);


            var replyText = response.Choices[0].Message.Content;
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

            state.Messages = completionOptions.Messages;
            await _chatgptState.SetAsync(turnContext, state, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
