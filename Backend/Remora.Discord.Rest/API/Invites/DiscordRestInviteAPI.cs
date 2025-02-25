//
//  DiscordRestInviteAPI.cs
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

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Core;
using Remora.Discord.Rest.Extensions;
using Remora.Results;

namespace Remora.Discord.Rest.API
{
    /// <inheritdoc cref="Remora.Discord.API.Abstractions.Rest.IDiscordRestInviteAPI" />
    [PublicAPI]
    public class DiscordRestInviteAPI : AbstractDiscordRestAPI, IDiscordRestInviteAPI
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordRestInviteAPI"/> class.
        /// </summary>
        /// <param name="discordHttpClient">The Discord HTTP client.</param>
        /// <param name="jsonOptions">The JSON options.</param>
        public DiscordRestInviteAPI(DiscordHttpClient discordHttpClient, IOptions<JsonSerializerOptions> jsonOptions)
            : base(discordHttpClient, jsonOptions)
        {
        }

        /// <inheritdoc />
        public virtual Task<Result<IInvite>> GetInviteAsync
        (
            string inviteCode,
            Optional<bool> withCounts = default,
            Optional<bool> withExpiration = default,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.GetAsync<IInvite>
            (
                $"invites/{inviteCode}",
                b =>
                {
                    if (withCounts.HasValue)
                    {
                        b.AddQueryParameter("with_counts", withCounts.Value.ToString());
                    }

                    if (withExpiration.HasValue)
                    {
                        b.AddQueryParameter("with_expiration", withExpiration.Value.ToString());
                    }
                },
                ct: ct
            );
        }

        /// <inheritdoc />
        public virtual Task<Result<IInvite>> DeleteInviteAsync
        (
            string inviteCode,
            Optional<string> reason = default,
            CancellationToken ct = default
        )
        {
            return this.DiscordHttpClient.DeleteAsync<IInvite>
            (
                $"invites/{inviteCode}",
                b => b.AddAuditLogReason(reason),
                ct: ct
            );
        }
    }
}
