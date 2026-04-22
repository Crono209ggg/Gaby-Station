// SPDX-FileCopyrightText: 2026 Goob Station Contributors
// SPDX-FileCopyrightText: 2026 punkzebu <punkzebub@email.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JiuJitsoSuplexPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JiuJitsoGuillotineChokePerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JiuJitsoReversalPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JiuJitsoSubmissionPerformedEvent : EntityEventArgs;
