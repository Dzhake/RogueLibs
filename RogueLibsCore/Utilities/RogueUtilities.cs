﻿﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Linq;
using BepInEx;
using UnityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RogueLibsCore
{
    /// <summary>
    ///   <para>Various utility methods and fields.</para>
    /// </summary>
    public static class RogueUtilities
    {
        private static readonly ConditionalWeakTable<byte[], SpriteList> spriteCache = new();
        private static readonly ConditionalWeakTable<byte[], AudioClip> audioCache = new();

        /// <summary>
        ///   <para>Creates a readable copy of the specified <paramref name="texture"/>.</para>
        /// </summary>
        /// <param name="texture">The texture to create a readable copy of.</param>
        /// <returns>The readable copy of the specified <paramref name="texture"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="texture"/> is <see langword="null"/>.</exception>
        public static Texture2D MakeTextureReadable(Texture2D texture)
        {
            if (texture is null) throw new ArgumentNullException(nameof(texture));
            if (texture.isReadable) return texture;

            RenderTexture tmp = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, tmp);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = tmp;

            Texture2D copy = new Texture2D(texture.width, texture.height);
            copy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            copy.Apply();

            RenderTexture.active = prev;
            return copy;
        }

        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with a texture from <paramref name="rawData"/> with the specified <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="rawData">The byte array containing a raw PNG- or JPEG-encoded image.</param>
        /// <param name="ppu">The pixels-per-unit of the custom sprite's texture.</param>
        /// <returns>The created sprite.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
        public static Sprite ConvertToSprite(byte[] rawData, float ppu = 64f)
            => ConvertToSprite(rawData, null, ppu);
        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with a texture from <paramref name="rawData"/> with the specified <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="rawData">The byte array containing a raw PNG- or JPEG-encoded image.</param>
        /// <param name="region">The region of the texture for the sprite to use.</param>
        /// <param name="ppu">The pixels-per-unit of the custom sprite's texture.</param>
        /// <returns>The created sprite.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
        public static Sprite ConvertToSprite(byte[] rawData, Rect region, float ppu = 64f)
            => ConvertToSprite(rawData, (Rect?)region, ppu);
        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with a texture from <paramref name="rawData"/> with the specified <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="rawData">The byte array containing a raw PNG- or JPEG-encoded image.</param>
        /// <param name="region">The region of the texture for the sprite to use, or <see langword="null"/> to use the entire texture.</param>
        /// <param name="ppu">The pixels-per-unit of the custom sprite's texture.</param>
        /// <returns>The created sprite.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rawData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
        public static Sprite ConvertToSprite(byte[] rawData, Rect? region, float ppu = 64f)
        {
            if (rawData is null) throw new ArgumentNullException(nameof(rawData));
            if (ppu <= 0f) throw new ArgumentOutOfRangeException(nameof(ppu), ppu, $"{nameof(ppu)} is less than or equal to 0.");

            if (!spriteCache.TryGetValue(rawData, out SpriteList? spriteList) || !spriteList.TryGet(ppu, out Sprite? sprite))
            {
                Texture2D texture = new Texture2D(15, 9);
                texture.LoadImage(rawData);
                texture.filterMode = FilterMode.Point;
                Rect rect = region ?? new Rect(0f, 0f, texture.width, texture.height);
                sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), ppu, 0u, SpriteMeshType.FullRect, Vector4.zero, false);

                if (spriteList is null)
                    spriteCache.Add(rawData, spriteList = new SpriteList());
                spriteList.Add(sprite);
            }
            return sprite;
        }

        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with a texture created from a file at the specified <paramref name="filePath"/> with the specified <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="filePath">The path to the PNG- or JPEG-encoded image file.</param>
        /// <param name="ppu">The pixels-per-unit of the custom sprite's texture.</param>
        /// <returns>The created sprite.</returns>
        /// <exception cref="FileNotFoundException">A file at the specified <paramref name="filePath"/> does not exist.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
        public static Sprite ConvertToSprite(string filePath, float ppu = 64f)
            => ConvertToSprite(filePath, null, ppu);
        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with a texture created from a file at the specified <paramref name="filePath"/> with the specified <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="filePath">The path to the PNG- or JPEG-encoded image file.</param>
        /// <param name="region">The region of the texture for the sprite to use.</param>
        /// <param name="ppu">The pixels-per-unit of the custom sprite's texture.</param>
        /// <returns>The created sprite.</returns>
        /// <exception cref="FileNotFoundException">A file at the specified <paramref name="filePath"/> does not exist.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
        public static Sprite ConvertToSprite(string filePath, Rect region, float ppu = 64f)
            => ConvertToSprite(filePath, (Rect?)region, ppu);
        /// <summary>
        ///   <para>Creates a <see cref="Sprite"/> with a texture created from a file at the specified <paramref name="filePath"/> with the specified <paramref name="ppu"/>.</para>
        /// </summary>
        /// <param name="filePath">The path to the PNG- or JPEG-encoded image file.</param>
        /// <param name="region">The region of the texture for the sprite to use, or <see langword="null"/> to use the entire texture.</param>
        /// <param name="ppu">The pixels-per-unit of the custom sprite's texture.</param>
        /// <returns>The created sprite.</returns>
        /// <exception cref="FileNotFoundException">A file at the specified <paramref name="filePath"/> does not exist.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ppu"/> is less than or equal to 0.</exception>
        public static Sprite ConvertToSprite(string filePath, Rect? region, float ppu = 64f)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"The specified {nameof(filePath)} does not exist.", filePath);
            if (ppu <= 0f) throw new ArgumentOutOfRangeException(nameof(ppu), ppu, $"{nameof(ppu)} is less than or equal to 0.");
            return ConvertToSprite(File.ReadAllBytes(filePath), region, ppu);
        }

        /// <summary>
        ///   <para>Converts the specified <paramref name="rawData"/> into an <see cref="AudioClip"/> using the specified audio <paramref name="format"/>.</para>
        ///   <para>Supported audio formats: MP3 (.mp3), WAV (.wav, .wave) and Ogg (.ogg, .spx, .opus, .og_).</para>
        /// </summary>
        /// <param name="rawData">MP3-, WAV- or Ogg-encoded audio data.</param>
        /// <param name="format">Format of the audio file.</param>
        /// <returns>Created <see cref="AudioClip"/>.</returns>
        /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="NotSupportedException">The specified <paramref name="format"/> is not supported.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        [Obsolete("Use the overload without the AudioType parameter. RogueLibs v4.0.0 now auto-detects the audio format.")]
        public static AudioClip ConvertToAudioClip(byte[] rawData, AudioType format)
            => ConvertToAudioClip(rawData);
        /// <summary>
        ///   <para>Converts an audio file from the specified <paramref name="filePath"/> into an <see cref="AudioClip"/> using the specified audio <paramref name="format"/>.</para>
        ///   <para>Supported audio formats: MP3 (.mp3), WAV (.wav, .wave) and Ogg (.ogg, .spx, .opus, .og_).</para>
        /// </summary>
        /// <param name="filePath">Path to the MP3-, WAV- or Ogg-encoded audio file.</param>
        /// <param name="format">Format of the audio file.</param>
        /// <returns>Created <see cref="AudioClip"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is an empty string or contains one or more invalid characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
        /// <exception cref="PathTooLongException"><paramref name="filePath"/> exceeds the system-defined maximum length.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="filePath"/> is invalid.</exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="filePath"/> specifies a directory or the caller does not have the required permission.</exception>
        /// <exception cref="FileNotFoundException">File specified in <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in invalid format or the specified <paramref name="format"/> is not supported.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        [Obsolete("Use the overload without the AudioType parameter. RogueLibs v4.0.0 now auto-detects the audio format.")]
        public static AudioClip ConvertToAudioClip(string filePath, AudioType format)
            => ConvertToAudioClip(filePath);
        /// <summary>
        ///   <para>Converts the specified <paramref name="rawData"/> into an <see cref="AudioClip"/>. Automatically detects the audio format.</para>
        ///   <para>Supported audio formats: MP3 (.mp3), WAV (.wav, .wave) and Ogg (.ogg, .spx, .opus, .og_).</para>
        /// </summary>
        /// <param name="rawData">MP3-, WAV- or Ogg-encoded audio data.</param>
        /// <returns>Created <see cref="AudioClip"/>.</returns>
        /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="NotSupportedException">The audio format could not be identified.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        public static AudioClip ConvertToAudioClip(byte[] rawData)
        {
            if (!audioCache.TryGetValue(rawData, out AudioClip? audioClip))
            {
                string name = ".audio-request." + Convert.ToString(new System.Random().Next(), 16).ToLowerInvariant();
                string filePath = Path.Combine(Paths.CachePath, name);

                try
                {
                    File.WriteAllBytes(filePath, rawData);
                    audioClip = ConvertToAudioClip(filePath);
                    audioCache.Add(rawData, audioClip);
                }
                finally
                {
                    File.Delete(filePath);
                }
            }
            return audioClip;
        }
        /// <summary>
        ///   <para>Converts an audio file from the specified <paramref name="filePath"/> into an <see cref="AudioClip"/>. Automatically detects the audio format.</para>
        ///   <para>Supported audio formats: MP3 (.mp3), WAV (.wav, .wave) and Ogg (.ogg, .spx, .opus, .og_).</para>
        /// </summary>
        /// <param name="filePath">Path to the MP3-, WAV- or Ogg-encoded audio file.</param>
        /// <returns>Created <see cref="AudioClip"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is an empty string or contains one or more invalid characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is <see langword="null"/>.</exception>
        /// <exception cref="PathTooLongException"><paramref name="filePath"/> exceeds the system-defined maximum length.</exception>
        /// <exception cref="DirectoryNotFoundException"><paramref name="filePath"/> is invalid.</exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
        /// <exception cref="UnauthorizedAccessException"><paramref name="filePath"/> specifies a directory or the caller does not have the required permission.</exception>
        /// <exception cref="FileNotFoundException">File specified in <paramref name="filePath"/> was not found.</exception>
        /// <exception cref="NotSupportedException">The audio format could not be identified.</exception>
        /// <exception cref="NotSupportedException"><paramref name="filePath"/> is in invalid format or the audio format could not be identified.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        public static AudioClip ConvertToAudioClip(string filePath)
        {
            AudioType format = DetectAudioFormat(filePath);
            if (format is AudioType.UNKNOWN)
            {
                const string supported = $"{nameof(AudioType.MPEG)}, {nameof(AudioType.WAV)} and {nameof(AudioType.OGGVORBIS)}";
                throw new NotSupportedException($"This audio format is not supported. Supported audio formats: {supported}.");
            }

            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, format);
            request.SendWebRequest();
            while (!request.isDone) Thread.Sleep(1);
            return DownloadHandlerAudioClip.GetContent(request);
        }

        private static AudioType DetectAudioFormat(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[12];
                _ = stream.Read(buffer, 0, 12);
                return DetectAudioFormat(buffer);
            }
        }
        private static AudioType DetectAudioFormat(byte[] rawData)
        {
            static bool IsMP3(byte[] b) // https://github.com/hemanth/is-mp3
                => b[0] == 73 && b[1] == 68 && b[2] == 51 || b[0] == 255 && (b[1] == 251 || b[1] == 250);
            static bool IsOGG(byte[] b) // https://github.com/hemanth/is-ogg
                => b[0] == 79 && b[1] == 103 && b[2] == 103 && b[3] == 83;
            static bool IsWAV(byte[] b) // https://github.com/hemanth/is-wav
                => b[0] == 82 && b[1] == 73 && b[2] == 70 && b[3] == 70 && b[8] == 87 && b[9] == 65 && b[10] == 86 && b[11] == 69;

            if (IsMP3(rawData)) return AudioType.MPEG;
            if (IsOGG(rawData)) return AudioType.OGGVORBIS;
            if (IsWAV(rawData)) return AudioType.WAV;
            return AudioType.UNKNOWN;
        }

        /// <summary>
        ///   <para>Returns the <paramref name="interfaceType"/>'s <paramref name="methodName"/> implemented by the specified <paramref name="type"/>.</para>
        /// </summary>
        /// <param name="type">The type implementing the specified <paramref name="interfaceType"/>.</param>
        /// <param name="interfaceType">The type of the interface with the method.</param>
        /// <param name="methodName">The name of the interface's method.</param>
        /// <returns>The <see cref="MethodInfo"/> of the implemented method, if found; otherwise, <see langword="null"/>.</returns>
        public static MethodInfo? GetInterfaceMethod(this Type type, Type interfaceType, string methodName)
        {
            Type implementedInterface;
            if (interfaceType.IsGenericTypeDefinition)
            {
                implementedInterface = Array.Find(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
                if (implementedInterface is null) return null;
            }
            else
            {
                implementedInterface = interfaceType;
                if (!type.GetInterfaces().Contains(interfaceType)) return null;
            }
            InterfaceMapping mapping = type.GetInterfaceMap(implementedInterface);

            int index = Array.FindIndex(mapping.InterfaceMethods, m => m.Name == methodName);
            return index == -1 ? null : mapping.TargetMethods[index];
        }
        /// <summary>
        ///   <para>Returns the <paramref name="interfaceType"/> <paramref name="methodNames"/>, implemented by the specified <paramref name="type"/>.</para>
        /// </summary>
        /// <param name="type">The type implementing the specified <paramref name="interfaceType"/>.</param>
        /// <param name="interfaceType">The type of the interface with the methods.</param>
        /// <param name="methodNames">The names of the interface's method.</param>
        /// <returns>An array of <see cref="MethodInfo"/>s of the implemented methods, if found; otherwise, <see langword="null"/>.</returns>
        public static MethodInfo[]? GetInterfaceMethods(this Type type, Type interfaceType, params string[] methodNames)
        {
            Type implementedInterface;
            if (interfaceType.IsGenericTypeDefinition)
            {
                implementedInterface = Array.Find(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
                if (implementedInterface is null) return null;
            }
            else
            {
                implementedInterface = interfaceType;
                if (!type.GetInterfaces().Contains(interfaceType)) return null;
            }
            InterfaceMapping mapping = type.GetInterfaceMap(implementedInterface);

            MethodInfo[] methods = new MethodInfo[methodNames.Length];
            for (int i = 0; i < methodNames.Length; i++)
            {
                int index = Array.FindIndex(mapping.InterfaceMethods, m => m.Name == methodNames[i]);
                if (index == -1) return null;
                methods[i] = mapping.TargetMethods[index];
            }
            return methods;
        }

        private sealed class SpriteList
        {
            private readonly List<Sprite> sprites = new List<Sprite>(1);

            public bool TryGet(float ppu, [NotNullWhen(true)] out Sprite? sprite)
                => (sprite = sprites.Find(s => s.pixelsPerUnit == ppu)) is not null;
            public void Add(Sprite sprite)
                => sprites.Add(sprite);
        }
    }
}
