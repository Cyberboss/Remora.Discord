﻿//
//  FeedbackService.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2017 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Themes;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Remora.Discord.Commands.Feedback.Services
{
    /// <summary>
    /// Handles sending formatted messages to the users.
    /// </summary>
    [PublicAPI]
    public class FeedbackService
    {
        private readonly ContextInjectionService _contextInjection;
        private readonly IDiscordRestChannelAPI _channelAPI;
        private readonly IDiscordRestUserAPI _userAPI;
        private readonly IDiscordRestInteractionAPI _interactionAPI;

        /// <summary>
        /// Gets the theme used by the feedback service.
        /// </summary>
        public IFeedbackTheme Theme { get; }

        /// <summary>
        /// Gets a value indicating whether the service, in the context of an interaction, has edited the original
        /// message.
        /// </summary>
        /// <remarks>This method always returns false in a message context.</remarks>
        public bool HasEditedOriginalMessage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackService"/> class.
        /// </summary>
        /// <param name="channelAPI">The channel API.</param>
        /// <param name="userAPI">The user API.</param>
        /// <param name="contextInjection">The context injection service.</param>
        /// <param name="interactionAPI">The webhook API.</param>
        /// <param name="feedbackTheme">The feedback theme to use.</param>
        public FeedbackService
        (
            IDiscordRestChannelAPI channelAPI,
            IDiscordRestUserAPI userAPI,
            ContextInjectionService contextInjection,
            IDiscordRestInteractionAPI interactionAPI,
            IFeedbackTheme feedbackTheme
        )
        {
            _channelAPI = channelAPI;
            _userAPI = userAPI;
            _contextInjection = contextInjection;
            _interactionAPI = interactionAPI;

            this.Theme = feedbackTheme;
        }

        /// <summary>
        /// Send an informational message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendInfoAsync
        (
            Snowflake channel,
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendMessageAsync(channel, new FeedbackMessage(contents, this.Theme.Primary), target, ct);

        /// <summary>
        /// Send an informational message wherever is most appropriate to the current context.
        /// </summary>
        /// <remarks>
        /// This method will either create a followup message (if the context is an interaction) or a normal channel
        /// message.
        /// </remarks>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendContextualInfoAsync
        (
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendContextualMessageAsync(new FeedbackMessage(contents, this.Theme.Primary), target, ct);

        /// <summary>
        /// Send an informational message to the given user as a direct message.
        /// </summary>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendPrivateInfoAsync
        (
            Snowflake user,
            string contents,
            CancellationToken ct = default
        )
            => SendPrivateMessageAsync(user, new FeedbackMessage(contents, this.Theme.Primary), ct);

        /// <summary>
        /// Send a positive, successful message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendSuccessAsync
        (
            Snowflake channel,
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendMessageAsync(channel, new FeedbackMessage(contents, this.Theme.Success), target, ct);

        /// <summary>
        /// Send a positive, successful message wherever is most appropriate to the current context.
        /// </summary>
        /// <remarks>
        /// This method will either create a followup message (if the context is an interaction) or a normal channel
        /// message.
        /// </remarks>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendContextualSuccessAsync
        (
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendContextualMessageAsync(new FeedbackMessage(contents, this.Theme.Success), target, ct);

        /// <summary>
        /// Send a positive, successful message to the given user as a direct message.
        /// </summary>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendPrivateSuccessAsync
        (
            Snowflake user,
            string contents,
            CancellationToken ct = default
        )
            => SendPrivateMessageAsync(user, new FeedbackMessage(contents, this.Theme.Success), ct);

        /// <summary>
        /// Send a neutral message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendNeutralAsync
        (
            Snowflake channel,
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendMessageAsync(channel, new FeedbackMessage(contents, this.Theme.Secondary), target, ct);

        /// <summary>
        /// Send a neutral message wherever is most appropriate to the current context.
        /// </summary>
        /// <remarks>
        /// This method will either create a followup message (if the context is an interaction) or a normal channel
        /// message.
        /// </remarks>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendContextualNeutralAsync
        (
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendContextualMessageAsync(new FeedbackMessage(contents, this.Theme.Secondary), target, ct);

        /// <summary>
        /// Send a neutral message to the given user as a direct message.
        /// </summary>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendPrivateNeutralAsync
        (
            Snowflake user,
            string contents,
            CancellationToken ct = default
        )
            => SendPrivateMessageAsync(user, new FeedbackMessage(contents, this.Theme.Secondary), ct);

        /// <summary>
        /// Send a warning message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendWarningAsync
        (
            Snowflake channel,
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendMessageAsync(channel, new FeedbackMessage(contents, this.Theme.Warning), target, ct);

        /// <summary>
        /// Send a warning message wherever is most appropriate to the current context.
        /// </summary>
        /// <remarks>
        /// This method will either create a followup message (if the context is an interaction) or a normal channel
        /// message.
        /// </remarks>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendContextualWarningAsync
        (
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendContextualMessageAsync(new FeedbackMessage(contents, this.Theme.Warning), target, ct);

        /// <summary>
        /// Send a warning message to the given user as a direct message.
        /// </summary>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendPrivateWarningAsync
        (
            Snowflake user,
            string contents,
            CancellationToken ct = default
        )
            => SendPrivateMessageAsync(user, new FeedbackMessage(contents, this.Theme.Warning), ct);

        /// <summary>
        /// Send a negative error message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendErrorAsync
        (
            Snowflake channel,
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendMessageAsync(channel, new FeedbackMessage(contents, this.Theme.FaultOrDanger), target, ct);

        /// <summary>
        /// Send a negative error message wherever is most appropriate to the current context.
        /// </summary>
        /// <remarks>
        /// This method will either create a followup message (if the context is an interaction) or a normal channel
        /// message.
        /// </remarks>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendContextualErrorAsync
        (
            string contents,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendContextualMessageAsync(new FeedbackMessage(contents, this.Theme.FaultOrDanger), target, ct);

        /// <summary>
        /// Send a negative error message to the given user as a direct message.
        /// </summary>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="contents">The contents of the message.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendPrivateErrorAsync
        (
            Snowflake user,
            string contents,
            CancellationToken ct = default
        )
            => SendPrivateMessageAsync(user, new FeedbackMessage(contents, this.Theme.FaultOrDanger), ct);

        /// <summary>
        /// Send a message.
        /// </summary>
        /// <param name="channel">The channel to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendMessageAsync
        (
            Snowflake channel,
            FeedbackMessage message,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendContentAsync(channel, message.Message, message.Colour, target, ct);

        /// <summary>
        /// Send a contextual message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendContextualMessageAsync
        (
            FeedbackMessage message,
            Snowflake? target = null,
            CancellationToken ct = default
        )
            => SendContextualContentAsync(message.Message, message.Colour, target, ct);

        /// <summary>
        /// Send a private message.
        /// </summary>
        /// <param name="user">The user to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IReadOnlyList<IMessage>>> SendPrivateMessageAsync
        (
            Snowflake user,
            FeedbackMessage message,
            CancellationToken ct = default
        )
            => SendPrivateContentAsync(user, message.Message, message.Colour, ct);

        /// <summary>
        /// Sends the given embed to the given channel.
        /// </summary>
        /// <param name="channel">The channel to send the embed to.</param>
        /// <param name="embed">The embed.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<Result<IMessage>> SendEmbedAsync
        (
            Snowflake channel,
            Embed embed,
            CancellationToken ct = default
        )
        {
            return _channelAPI.CreateMessageAsync(channel, embeds: new[] { embed }, ct: ct);
        }

        /// <summary>
        /// Sends the given embed to current context.
        /// </summary>
        /// <param name="embed">The embed.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Result<IMessage>> SendContextualEmbedAsync
        (
            Embed embed,
            CancellationToken ct = default
        )
        {
            if (_contextInjection.Context is null)
            {
                return new InvalidOperationError("Contextual sends require a context to be available.");
            }

            switch (_contextInjection.Context)
            {
                case MessageContext messageContext:
                {
                    return await _channelAPI.CreateMessageAsync
                    (
                        messageContext.ChannelID,
                        embeds: new[] { embed },
                        ct: ct
                    );
                }
                case InteractionContext interactionContext:
                {
                    var result = await _interactionAPI.CreateFollowupMessageAsync
                    (
                        interactionContext.ApplicationID,
                        interactionContext.Token,
                        embeds: new[] { embed },
                        ct: ct
                    );

                    if (!result.IsSuccess)
                    {
                        return result;
                    }

                    if (!this.HasEditedOriginalMessage)
                    {
                        this.HasEditedOriginalMessage = true;
                    }

                    return result;
                }
                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Sends the given embed to the given user in their private DM channel.
        /// </summary>
        /// <param name="user">The ID of the user to send the embed to.</param>
        /// <param name="embed">The embed.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Result<IMessage>> SendPrivateEmbedAsync
        (
            Snowflake user,
            Embed embed,
            CancellationToken ct = default
        )
        {
            var getUserDM = await _userAPI.CreateDMAsync(user, ct);
            if (!getUserDM.IsSuccess)
            {
                return Result<IMessage>.FromError(getUserDM);
            }

            var dm = getUserDM.Entity;

            return await SendEmbedAsync(dm.ID, embed, ct);
        }

        /// <summary>
        /// Sends the given string as one or more sequential embeds, chunked into sets of 1024 characters.
        /// </summary>
        /// <param name="channel">The channel to send the embed to.</param>
        /// <param name="contents">The contents to send.</param>
        /// <param name="color">The embed colour.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Result<IReadOnlyList<IMessage>>> SendContentAsync
        (
            Snowflake channel,
            string contents,
            Color color,
            Snowflake? target = null,
            CancellationToken ct = default
        )
        {
            var sendResults = new List<IMessage>();
            foreach (var chunk in CreateContentChunks(target, color, contents))
            {
                var send = await SendEmbedAsync(channel, chunk, ct);
                if (!send.IsSuccess)
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(send);
                }

                sendResults.Add(send.Entity);
            }

            return sendResults;
        }

        /// <summary>
        /// Sends the given string as one or more sequential embeds, chunked into sets of 1024 characters.
        /// </summary>
        /// <param name="contents">The contents to send.</param>
        /// <param name="color">The embed colour.</param>
        /// <param name="target">The target user to mention, if any.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Result<IReadOnlyList<IMessage>>> SendContextualContentAsync
        (
            string contents,
            Color color,
            Snowflake? target = null,
            CancellationToken ct = default
        )
        {
            var sendResults = new List<IMessage>();
            foreach (var chunk in CreateContentChunks(target, color, contents))
            {
                var send = await SendContextualEmbedAsync(chunk, ct);
                if (!send.IsSuccess)
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(send);
                }

                sendResults.Add(send.Entity);
            }

            return sendResults;
        }

        /// <summary>
        /// Sends the given string as one or more sequential embeds to the given user over DM, chunked into sets of 1024
        /// characters.
        /// </summary>
        /// <param name="user">The ID of the user to send the content to.</param>
        /// <param name="contents">The contents to send.</param>
        /// <param name="color">The embed colour.</param>
        /// <param name="ct">The cancellation token for this operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Result<IReadOnlyList<IMessage>>> SendPrivateContentAsync
        (
            Snowflake user,
            string contents,
            Color color,
            CancellationToken ct = default
        )
        {
            var getUserDM = await _userAPI.CreateDMAsync(user, ct);
            if (!getUserDM.IsSuccess)
            {
                return Result<IReadOnlyList<IMessage>>.FromError(getUserDM);
            }

            var dm = getUserDM.Entity;
            return await SendContentAsync(dm.ID, contents, color, null, ct);
        }

        /// <summary>
        /// Creates a feedback embed.
        /// </summary>
        /// <param name="target">The invoking mentionable.</param>
        /// <param name="color">The colour of the embed.</param>
        /// <param name="contents">The contents of the embed.</param>
        /// <returns>A feedback embed.</returns>
        [Pure]
        private Embed CreateFeedbackEmbed(Snowflake? target, Color color, string contents)
        {
            if (target is null)
            {
                return new Embed { Colour = color } with { Description = contents };
            }

            return new Embed { Colour = color } with { Description = $"<@{target}> | {contents}" };
        }

        /// <summary>
        /// Chunks an input string into one or more embeds. Discord places an internal limit on embed lengths of 2048
        /// characters, and we collapse that into 1024 for readability's sake.
        /// </summary>
        /// <param name="target">The target user, if any.</param>
        /// <param name="color">The color of the embed.</param>
        /// <param name="contents">The complete contents of the message.</param>
        /// <returns>The chunked embeds.</returns>
        [Pure]
        private IEnumerable<Embed> CreateContentChunks(Snowflake? target, Color color, string contents)
        {
            // Sometimes the content is > 2048 in length. We'll chunk it into embeds of 1024 here.
            if (contents.Length < 1024)
            {
                yield return CreateFeedbackEmbed(target, color, contents.Trim());
                yield break;
            }

            var words = contents.Split(' ');
            var messageBuilder = new StringBuilder();
            foreach (var word in words)
            {
                if (messageBuilder.Length >= 1024)
                {
                    yield return CreateFeedbackEmbed(target, color, messageBuilder.ToString().Trim());
                    messageBuilder.Clear();
                }

                messageBuilder.Append(word);
                messageBuilder.Append(' ');
            }

            if (messageBuilder.Length > 0)
            {
                yield return CreateFeedbackEmbed(target, color, messageBuilder.ToString().Trim());
            }
        }
    }
}
