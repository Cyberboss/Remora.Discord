//
//  ISelectOption.cs
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

using JetBrains.Annotations;
using Remora.Discord.Core;

namespace Remora.Discord.API.Abstractions.Objects
{
    /// <summary>
    /// Represents a single selectable option.
    /// </summary>
    [PublicAPI]
    public interface ISelectOption
    {
        /// <summary>
        /// Gets the user-facing name of the option. Max 100 characters.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the developer-defined value of the option. Max 100 characters.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Gets an additional description of the option. Max 100 characters.
        /// </summary>
        Optional<string> Description { get; }

        /// <summary>
        /// Gets an emoji that will render along with the option.
        /// </summary>
        Optional<IPartialEmoji> Emoji { get; }

        /// <summary>
        /// Gets a value indicating whether this option will be selected by default. May be <value>true</value> for more
        /// than one option in a multi-select menu.
        /// </summary>
        Optional<bool> IsDefault { get; }
    }
}
