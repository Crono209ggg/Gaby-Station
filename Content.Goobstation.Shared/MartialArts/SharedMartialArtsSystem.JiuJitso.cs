// SPDX-FileCopyrightText: 2026 Goob Station Contributors
// SPDX-FileCopyrightText: 2026 punkzebu <punkzebub@email.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.GrabIntent;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Clothing;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Standing;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeJiuJitso()
    {
        SubscribeLocalEvent<CanPerformComboComponent, JiuJitsoSuplexPerformedEvent>(OnJiuJitsoSuplex);
        SubscribeLocalEvent<CanPerformComboComponent, JiuJitsoGuillotineChokePerformedEvent>(OnJiuJitsoGuillotine);
        SubscribeLocalEvent<CanPerformComboComponent, JiuJitsoReversalPerformedEvent>(OnJiuJitsoReversal);
        SubscribeLocalEvent<CanPerformComboComponent, JiuJitsoSubmissionPerformedEvent>(OnJiuJitsoSubmission);

        SubscribeLocalEvent<GrantJiuJitsoComponent, ClothingGotEquippedEvent>(OnGrantJiuJitso);
        SubscribeLocalEvent<GrantJiuJitsoComponent, ClothingGotUnequippedEvent>(OnRemoveJiuJitso);
        SubscribeLocalEvent<GrantJiuJitsoComponent, UseInHandEvent>(OnGrantJiuJitsoUse);
    }

    private void OnGrantJiuJitso(Entity<GrantJiuJitsoComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (!_netManager.IsServer)
            return;

        TryGrantMartialArt(args.Wearer, ent.Comp);
    }

    private void OnRemoveJiuJitso(Entity<GrantJiuJitsoComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (!_netManager.IsServer)
            return;

        TryRemoveMartialArt(args.Wearer, MartialArtsForms.JiuJitso);
    }

    private void OnGrantJiuJitsoUse(EntityUid uid, GrantJiuJitsoComponent comp, UseInHandEvent args)
    {
        if (!comp.GrantOnUse)
            return;

        OnGrantCQCUse(uid, comp, args);
    }

    private void OnJiuJitsoSuplex(Entity<CanPerformComboComponent> ent, ref JiuJitsoSuplexPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var targetDowned)
            || !IsDown(ent)
            || targetDowned
            || !TryComp<PullableComponent>(target, out var targetPullable))
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);

        var knockdownTime = TimeSpan.FromSeconds(proto.ParalyzeTime);
        _stun.TryKnockdown(target, knockdownTime, true, true, proto.DropItems);
        _stun.TryKnockdown(ent.Owner, knockdownTime, true, true, false);

        _pulling.TryStopPull(target, targetPullable, ent, true);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnJiuJitsoGuillotine(Entity<CanPerformComboComponent> ent, ref JiuJitsoGuillotineChokePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _)
            || !IsDown(ent)
            || !IsDown(target)
            || !TryComp<PullerComponent>(ent, out var puller)
            || !TryComp<GrabIntentComponent>(ent, out var grabIntent)
            || !TryComp<PullableComponent>(target, out var pullable)
            || !TryComp<GrabbableComponent>(target, out var grabbable)
            || puller.Pulling != target)
            return;

        _grab.TrySetGrabStages((ent, puller, grabIntent), (target, pullable, grabbable), GrabStage.Suffocate);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnJiuJitsoReversal(Entity<CanPerformComboComponent> ent, ref JiuJitsoReversalPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _)
            || !TryComp<PullableComponent>(ent, out var selfPullable)
            || selfPullable.Puller != target
            || !TryComp<GrabIntentComponent>(target, out var enemyGrabIntent)
            || !TryComp<PullerComponent>(target, out var enemyPuller)
            || enemyPuller.Pulling != ent
            || enemyGrabIntent.GrabStage is not (GrabStage.Hard or GrabStage.Suffocate)
            || !TryComp<PullerComponent>(ent, out var selfPuller)
            || !TryComp<GrabIntentComponent>(ent, out var selfGrabIntent)
            || !TryComp<PullableComponent>(target, out var targetPullable)
            || !TryComp<GrabbableComponent>(target, out var targetGrabbable)
            || !_pulling.TryStopPull(ent, selfPullable, target, true)
            || !_pulling.TryStartPull(ent, target, selfPuller))
            return;

        _grab.TrySetGrabStages((ent, selfPuller, selfGrabIntent), (target, targetPullable, targetGrabbable), GrabStage.Hard);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnJiuJitsoSubmission(Entity<CanPerformComboComponent> ent, ref JiuJitsoSubmissionPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _)
            || !IsDown(ent)
            || !IsDown(target)
            || !TryComp<PullerComponent>(ent, out var puller)
            || !TryComp<GrabIntentComponent>(ent, out var grabIntent)
            || !TryComp<GrabbableComponent>(target, out var grabbable)
            || puller.Pulling != target
            || grabIntent.GrabStage != GrabStage.Suffocate
            || grabbable.GrabStage != GrabStage.Suffocate)
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);

        TryDropSubmissionTargetedHand(target, ent);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private bool IsDown(EntityUid uid)
    {
        return TryComp<StandingStateComponent>(uid, out var standingState) && !standingState.Standing;
    }

    private void TryDropSubmissionTargetedHand(EntityUid target, EntityUid performer)
    {
        if (!TryComp<TargetingComponent>(performer, out var targeting)
            || !TryComp<HandsComponent>(target, out var hands))
            return;

        string? handId = targeting.Target switch
        {
            TargetBodyPart.LeftHand => "left",
            TargetBodyPart.RightHand => "right",
            _ => null,
        };

        if (handId == null)
            return;

        _hands.TryDrop((target, hands), handId, checkActionBlocker: false);
    }
}
