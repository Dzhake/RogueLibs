﻿using System.Collections.Generic;
using Mono.Cecil;

namespace RogueLibsPatcher
{
    // ReSharper disable once InconsistentNaming
    public static class RogueLibsPatcher_Gen2
    {
        public static IEnumerable<string> TargetDLLs { get; } = new string[1] { "Assembly-CSharp.dll" };

        private static ModuleDefinition module = null!;
        private static TypeReference objRef = null!;

        public static void Patch(AssemblyDefinition assembly)
        {
            module = assembly.MainModule;

            if (!module.TryGetTypeReference(typeof(object).FullName, out objRef))
                objRef = module.ImportReference(typeof(object));

            const string rlHooks = "__RogueLibsHooks";
            const string rlContainer = "__RogueLibsContainer";
            const string rlCustom = "__RogueLibsCustom";

            PatchField(nameof(InvItem), rlHooks);

            PatchField(nameof(PlayfieldObject), rlHooks);

            PatchField(nameof(StatusEffect), rlHooks);
            PatchField(nameof(StatusEffect), rlContainer);

            PatchField(nameof(Trait), rlHooks);
            PatchField(nameof(Trait), rlContainer);

            PatchField(nameof(Unlock), rlCustom);

            PatchField(nameof(ButtonData), rlCustom);

            PatchField(nameof(tk2dSpriteDefinition), rlCustom);

            // 2nd generation of patches

            PatchField(nameof(MainGUI), rlHooks);
            PatchField(nameof(WorldSpaceGUI), rlHooks);
        }

        public static void PatchField(string typeName, string fieldName)
        {
            TypeDefinition type = module.GetType(typeName);

            // Note: Do not use LINQ here. Apparently, some versions of Mono may be missing System.Func and System.Action.
            // (see https://github.com/SugarBarrel/ECTD/issues/4)
            for (int i = 0, count = type.Fields.Count; i < count; i++)
                if (type.Fields[i].Name == fieldName)
                    return;

            const FieldAttributes attr = FieldAttributes.Public | FieldAttributes.NotSerialized;
            type.Fields.Add(new FieldDefinition(fieldName, attr, objRef));
        }

    }
}
