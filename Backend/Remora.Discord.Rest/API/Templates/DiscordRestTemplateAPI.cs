//
//  DiscordRestTemplateAPI.cs
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

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Discord.Rest.Extensions;
using Remora.Discord.Rest.Utility;
using Remora.Results;

namespace Remora.Discord.Rest.API
{
    /// <inheritdoc cref="Remora.Discord.API.Abstractions.Rest.IDiscordRestTemplateAPI" />
    [PublicAPI]
    public class DiscordRestTemplateAPI : AbstractDiscordRestAPI, IDiscordRestTemplateAPI
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestTemplateAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The Json options.</param>
        public DiscordRestTemplateAPI(DiscordHttpClient discordHttpClient, IOptions<JsonSerializerOptions> jsonOptions)
            : base(discordHttpClient, jsonOptions)
        {
        }

        /// <inheritdoc />
        public virtual Task<Result<ITemplate>> GetTemplateAsync
        (
            string templateCode,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.GetAsync<ITemplate>
            (
                $"guilds/templates/{templateCode}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual async Task<Result<IGuild>> CreateGuildFromTemplateAsync
        (
            string templateCode,
            string name,
            Optional<Stream> icon = default,
            CancellationToken ct = default
        )
        {
            Optional<string?> iconData = default;
            if (icon.IsDefined(out var iconStream))
            {
                var packIcon = await ImagePacker.PackImageAsync(iconStream, ct);
                if (!packIcon.IsSuccess)
                {
                    return Result<IGuild>.FromError(packIcon);
                }

                iconData = packIcon.Entity;
            }

            return await this.DiscordHttpClient.PostAsync<IGuild>
            (
                $"guilds/templates/{templateCode}",
                b => b.WithJson
                (
                    j =>
                    {
                        j.WriteString("name", name);
                        j.Write("icon", iconData);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IReadOnlyList<ITemplate>>> GetGuildTemplatesAsync
        (
            Snowflake guildID,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.GetAsync<IReadOnlyList<ITemplate>>
            (
                $"guilds/{guildID}/templates",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<ITemplate>> CreateGuildTemplateAsync
        (
            Snowflake guildID,
            string name,
            Optional<string?> description = default,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.PostAsync<ITemplate>
            (
                $"guilds/{guildID}/templates",
                b => b.WithJson
                (
                    j =>
                    {
                        j.WriteString("name", name);
                        j.Write("description", description, this.JsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<ITemplate>> SyncGuildTemplateAsync
        (
            Snowflake guildID,
            string templateCode,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.PutAsync<ITemplate>
            (
                $"guilds/{guildID}/templates/{templateCode}",
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<ITemplate>> ModifyGuildTemplateAsync
        (
            Snowflake guildID,
            string templateCode,
            string name,
            Optional<string> description,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.PatchAsync<ITemplate>
            (
                $"guilds/{guildID}/templates/{templateCode}",
                b => b.WithJson
                (
                    j =>
                    {
                        j.WriteString("name", name);
                        j.Write("description", description, this.JsonOptions);
                    }
                ),
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<ITemplate>> DeleteGuildTemplateAsync
        (
            Snowflake guildID,
            string templateCode,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.DeleteAsync<ITemplate>
            (
                $"guilds/{guildID}/templates/{templateCode}",
                ct: ct
            );
        }
    }
}
