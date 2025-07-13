using UnityEngine;

namespace RogueLibsCore
{
    public abstract class CustomAgentPart : HookBase<CustomAgent> // TODO: Later add position, physics, procedural animation and etc
    {
        public CustomAgentPartMetadata Metadata { get; internal set; } = null!;

        public Color Color
        {
            get => Instance.Colors.GetColor(Metadata.Id);
            set => Instance.Colors.SetColor(Metadata.Id, value);
        }
    }

    public interface IAgentPartWatertable // Here is layer editing to 25 OnEnter and to 5 OnExit (NEED ACCESS TO SPRITE)
    {
        void OnWaterEnter();
        void OnWaterExit();
    }

    public interface IAgentPartWalkable // Agent walking + multwalking
    {

    }

    public interface IAgentPartDancyable // Agent dancing
    {

    }

    public interface IAgentPartArrestable
    {

    }
}
