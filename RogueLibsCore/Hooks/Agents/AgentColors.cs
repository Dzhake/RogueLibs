using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Represents a color settings for custom agent.</para>
    /// </summary>
    public sealed class AgentColors(CustomAgent agent) // TODO: Update XML docs
    {
        /// <summary>
        ///   <para>Gets the current <see cref="CustomAgent"/> instance.</para>
        /// </summary>
        public CustomAgent Instance { get; } = agent;

        private readonly Dictionary<int, Color> _colors = [];

        public void SetColor(int agentPartId, Color color) => _colors.Add(agentPartId, color);

        public bool TryGetColor(int agentPartId, [NotNullWhen(true)] out Color? color)
        {
            color = null;
            if (_colors.TryGetValue(agentPartId, out var result)) color = result;
            return color != null;
        }

        public Color GetColor(int agentPartId)
        {
            TryGetColor(agentPartId, out var color);
            return color ?? throw new InvalidOperationException($"Custom agent {Instance.Metadata.Name} not have color for part {agentPartId}.");
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent hair color. Don't use <see cref="AgentHitbox.hairColor"/></para>
        /// </summary>
        public Color HairColor
        {
            get => GetColor(VanillaAgentParts.Hair);
            set => SetColor(VanillaAgentParts.Hair, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent facial hair color. Don't use <see cref="AgentHitbox.facialHairColor"/></para>
        /// </summary>
        public Color FacialHairColor
        {
            get => GetColor(VanillaAgentParts.FacialHair);
            set => SetColor(VanillaAgentParts.FacialHair, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent head color. Don't use <see cref="AgentHitbox.skinColor"/></para>
        /// </summary>
        public Color HeadColor
        {
            get => GetColor(VanillaAgentParts.Head);
            set => SetColor(VanillaAgentParts.Head, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent eyes color. Don't use <see cref="AgentHitbox.eyesColor"/></para>
        /// </summary>
        public Color EyesColor
        {
            get => GetColor(VanillaAgentParts.Eyes);
            set => SetColor(VanillaAgentParts.Eyes, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent body color. Don't use <see cref="AgentHitbox.bodyColor"/></para>
        /// </summary>
        public Color BodyColor
        {
            get => GetColor(VanillaAgentParts.Body);
            set => SetColor(VanillaAgentParts.Body, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent arm1 color. Don't use <see cref="AgentHitbox.skinColor"/></para>
        /// </summary>
        public Color Arm1Color
        {
            get => GetColor(VanillaAgentParts.Arm1);
            set => SetColor(VanillaAgentParts.Arm1, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent arm2 color. Don't use <see cref="AgentHitbox.skinColor"/></para>
        /// </summary>
        public Color Arm2Color
        {
            get => GetColor(VanillaAgentParts.Arm2);
            set => SetColor(VanillaAgentParts.Arm2, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent leg1 color. Don't use <see cref="AgentHitbox.legsColor"/></para>
        /// </summary>
        public Color Leg1Color
        {
            get => GetColor(VanillaAgentParts.Leg1);
            set => SetColor(VanillaAgentParts.Leg1, value);
        }

        /// <summary>
        ///   <para><see cref="Color"/> that represents this agent leg2 color. Don't use <see cref="AgentHitbox.legsColor"/></para>
        /// </summary>
        public Color Leg2Color
        {
            get => GetColor(VanillaAgentParts.Leg2);
            set => SetColor(VanillaAgentParts.Leg2, value);
        }
    }
}
