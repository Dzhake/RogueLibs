using UnityEngine;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a color settings for custom agent.</para>
    /// </summary>
    public sealed class AgentColors
    {
        /// <summary>
        ///   <para>Gets the current <see cref="CustomAgent"/> instance.</para>
        /// </summary>
        public CustomAgent Instance { get; }
        public AgentColors(CustomAgent agent) => Instance = agent;
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent hair color. Don't use <see cref="AgentHitbox.hairColor"/></para>
        /// </summary>
        public Color HairColor { get => Instance.Metadata.hairColor; set => Instance.Metadata.hairColor = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent facial hair color. Don't use <see cref="AgentHitbox.facialHairColor"/></para>
        /// </summary>
        public Color FacialHairColor { get => Instance.Metadata.facialHairColor; set => Instance.Metadata.facialHairColor = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent head color. Don't use <see cref="AgentHitbox.skinColor"/></para>
        /// </summary>
        public Color HeadColor { get => Instance.Metadata.headColor; set => Instance.Metadata.headColor = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent eyes color. Don't use <see cref="AgentHitbox.eyesColor"/></para>
        /// </summary>
        public Color EyesColor { get => Instance.Metadata.eyesColor; set => Instance.Metadata.eyesColor = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent body color. Don't use <see cref="AgentHitbox.bodyColor"/></para>
        /// </summary>
        public Color BodyColor { get => Instance.Metadata.bodyColor; set => Instance.Metadata.bodyColor = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent arm1 color. Don't use <see cref="AgentHitbox.skinColor"/></para>
        /// </summary>
        public Color Arm1Color { get => Instance.Metadata.arm1Color; set => Instance.Metadata.arm1Color = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent arm2 color. Don't use <see cref="AgentHitbox.skinColor"/></para>
        /// </summary>
        public Color Arm2Color { get => Instance.Metadata.arm2Color; set => Instance.Metadata.arm2Color = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent leg1 color. Don't use <see cref="AgentHitbox.legsColor"/></para>
        /// </summary>
        public Color Leg1Color { get => Instance.Metadata.leg1Color; set => Instance.Metadata.leg1Color = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent leg2 color. Don't use <see cref="AgentHitbox.legsColor"/></para>
        /// </summary>
        public Color Leg2Color { get => Instance.Metadata.leg2Color; set => Instance.Metadata.leg2Color = value; }
        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent footwear color. Don't use <see cref="AgentHitbox.footwearColor"/></para>
        /// </summary>
        public Color FootwearColor { get => Instance.Metadata.footwearColor; set => Instance.Metadata.footwearColor = value; }
    }
}
