using System;

namespace RogueLibsCore
{
    public static class HookEvents
    {
        internal static readonly HookEventDispatcher<IDoUpdate> updateDispatcher = new();
        internal static readonly HookEventDispatcher<IDoLateUpdate> lateUpdateDispatcher = new();
        internal static readonly HookEventDispatcher<IDoFixedUpdate> fixedUpdateDispatcher = new();
        internal static readonly HookEventDispatcher<ITriggerOnLevelStart> triggerOnLevelStartDispatcher = new();

        internal static void RegisterHookEvents(IHook hook)
        {
            updateDispatcher.TryRegister(hook);
            lateUpdateDispatcher.TryRegister(hook);
            fixedUpdateDispatcher.TryRegister(hook);
            triggerOnLevelStartDispatcher.TryRegister(hook);
            RegisteredHook?.Invoke(hook);
        }
        internal static void UnRegisterHookEvents(IHook hook)
        {
            updateDispatcher.TryUnRegister(hook);
            lateUpdateDispatcher.TryUnRegister(hook);
            fixedUpdateDispatcher.TryUnRegister(hook);
            triggerOnLevelStartDispatcher.TryUnRegister(hook);
            UnRegisteredHook?.Invoke(hook);
        }

        internal static void UpdateRegisteredHooks()
            => updateDispatcher.DispatchEvent(static h => h.Update());
        internal static void LateUpdateRegisteredHooks()
            => lateUpdateDispatcher.DispatchEvent(static h => h.LateUpdate());
        internal static void FixedUpdateRegisteredHooks()
            => fixedUpdateDispatcher.DispatchEvent(static h => h.FixedUpdate());
        internal static void TriggerOnLevelStartHooks()
            => triggerOnLevelStartDispatcher.DispatchEvent(static h => h.LevelStart());

        public static event Action<IHook>? RegisteredHook;
        public static event Action<IHook>? UnRegisteredHook;

    }
}
